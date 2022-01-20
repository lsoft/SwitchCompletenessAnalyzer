using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SwitchCompletenessAnalyzer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace SwitchCompletenessAnalyzer
{
    /// <summary>
    /// Check for switch over enum completness regardless of default branch existense.
    /// This is a minor modification of https://github.com/edumserrano/roslyn-analyzers/blob/master/Source/RoslynAnalyzers/Analyzers/CodeAnalysis/Enums/SwitchOnEnumMustHandleAllCases/SwitchOnEnumMustHandleAllCasesDiagnosticAnalyzer.cs
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SwitchCompletenessAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Maintainability";
        public const string DiagnosticId = "SWITCHCOMPLETENESS001";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
        }

        private void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var switchStatement = context.TryGetSyntaxNode<SwitchStatementSyntax>();
            if (switchStatement == null)
            {
                return;
            }

            var semanticModel = context.SemanticModel;
            var enumType = semanticModel.GetTypeInfo(switchStatement.Expression, context.CancellationToken).Type as INamedTypeSymbol;

            if (!IsValidSwitch(enumType))
            {
                return;
            }

            //if (!switchStatement.HasDefaultSwitchStatement())
            //{
            //    context.ReportDiagnostic(Diagnostic.Create(_rule, switchStatement.SwitchKeyword.GetLocation()));
            //    return;
            //}

            var labelSymbols = switchStatement.GetLabelSymbols(semanticModel, context.CancellationToken);
            if (!labelSymbols.Any())
                return;

            var possibleEnumSymbols = enumType.GetAllPossibleEnumSymbols();
            if (!possibleEnumSymbols.Any())
                return;

            // possibleEnumSymbols and labelSymbols has a dictionary with the enum value and its corresponding symbols
            // I'm not using the symbols but I could compare a one symbol to another
            // right now I'm comparing only the enum values
            var possibleEnumValues = possibleEnumSymbols.Keys
                .OrderBy(x => x)
                .ToList();
            var declaredEnumValues = labelSymbols.Keys
                .OrderBy(x => x)
                .ToList();

            if (declaredEnumValues.SequenceEqual(possibleEnumValues))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(_rule, switchStatement.SwitchKeyword.GetLocation()));
        }

        private bool IsValidSwitch(INamedTypeSymbol enumType)
        {
            //only allow switch on enum types
            if (enumType == null || enumType.TypeKind != TypeKind.Enum)
            {
                return false;
            }

            // ignore enums marked with Flags
            foreach (var attribute in enumType.GetAttributes())
            {
                var containingClass = attribute.AttributeClass.ToDisplayString();
                if (containingClass == typeof(System.FlagsAttribute).FullName)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
