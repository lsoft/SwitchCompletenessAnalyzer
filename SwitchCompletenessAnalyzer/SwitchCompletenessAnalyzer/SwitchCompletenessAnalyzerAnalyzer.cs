using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SwitchCompletenessAnalyzer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace SwitchCompletenessAnalyzer
{
    /// <summary>
    /// Check for switch over enum completness regardless of default branch existense.
    /// This is a modification of https://github.com/edumserrano/roslyn-analyzers/blob/master/Source/RoslynAnalyzers/Analyzers/CodeAnalysis/Enums/SwitchOnEnumMustHandleAllCases/SwitchOnEnumMustHandleAllCasesDiagnosticAnalyzer.cs
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SwitchCompletenessAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Maintainability";
        public const string DiagnosticId = "SWITCHCOMPLETENESS001";
        private const string Delimiter = "|";

        private static readonly string _flagAttributeName = nameof(FlagsAttribute);
        private static readonly string _flagAttributeFullName = typeof(FlagsAttribute).FullName;

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
            
            context.RegisterSyntaxNodeAction(CheckSwithStatement, SyntaxKind.SwitchStatement);
            context.RegisterSyntaxNodeAction(CheckSwithExpression, SyntaxKind.SwitchExpression);
        }

        private void CheckSwithExpression(SyntaxNodeAnalysisContext context)
        {
            var switchExpression = context.TryGetSyntaxNode<SwitchExpressionSyntax>();
            if (switchExpression == null)
            {
                return;
            }

            var semanticModel = context.SemanticModel;
            var enumType = semanticModel.GetTypeInfo(switchExpression.GoverningExpression, context.CancellationToken).Type as INamedTypeSymbol;
            if (enumType == null)
            {
                return;
            }

            var silentEnumList = ParseSilentEnum(context);

            if (!IsValidSwitch(enumType, ref silentEnumList))
            {
                return;
            }

            var labelSymbols = switchExpression.GetLabelSymbols(semanticModel, context.CancellationToken);
            var possibleEnumSymbols = enumType.GetAllPossibleEnumSymbols();

            var fail = CheckForFail(labelSymbols, possibleEnumSymbols);
            if (fail)
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, switchExpression.SwitchKeyword.GetLocation()));
            }
        }

        private void CheckSwithStatement(SyntaxNodeAnalysisContext context)
        {
            var switchStatement = context.TryGetSyntaxNode<SwitchStatementSyntax>();
            if (switchStatement == null)
            {
                return;
            }

            var semanticModel = context.SemanticModel;
            var enumType = semanticModel.GetTypeInfo(switchStatement.Expression, context.CancellationToken).Type as INamedTypeSymbol;
            if (enumType == null)
            {
                return;
            }

            var silentEnumList = ParseSilentEnum(context);
            if (!IsValidSwitch(enumType, ref silentEnumList))
            {
                return;
            }

            var labelSymbols = switchStatement.GetLabelSymbols(semanticModel, context.CancellationToken);
            var possibleEnumSymbols = enumType.GetAllPossibleEnumSymbols();

            var fail = CheckForFail(labelSymbols, possibleEnumSymbols);
            if (fail)
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, switchStatement.SwitchKeyword.GetLocation()));
            }
        }

        private bool CheckForFail(
            IReadOnlyDictionary<long, ISymbol> labelSymbols,
            IReadOnlyDictionary<long, List<ISymbol>> possibleEnumSymbols
            )
        {
            if (labelSymbols.Count != possibleEnumSymbols.Count)
            {
                return true;
            }

            foreach (var dev in labelSymbols.Keys)
            {
                foreach (var pev in possibleEnumSymbols.Keys)
                {
                    if (dev == pev)
                    {
                        goto ok;
                    }
                }

                //appropriate value does not found
                return true;

            ok:
                //next iteration
                ;
            }

            return false;
        }

        private bool IsValidSwitch(
            INamedTypeSymbol enumType,
            ref string silentEnumList
            )
        {
            //only allow switch on enum types
            if (enumType == null || enumType.TypeKind != TypeKind.Enum)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(silentEnumList))
            {
                if (silentEnumList.Contains(enumType.Name)) //fast and dirty check; helpful for better performance and lesser allocation
                {
                    var fullName = enumType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var fullName2 = fullName + Delimiter;
                    if (silentEnumList.Contains(fullName2))
                    {
                        return false;
                    }
                }
            }

            // ignore enums marked with Flags
            foreach (var attribute in enumType.GetAttributes())
            {
                if (attribute.AttributeClass != null)
                {
                    if (attribute.AttributeClass.Name.Contains(_flagAttributeName)) //fast and dirty check; helpful for better performance and lesser allocation
                    {
                        var containingClass = attribute.AttributeClass.ToDisplayString();
                        if (containingClass == _flagAttributeFullName)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private string ParseSilentEnum(SyntaxNodeAnalysisContext context)
        {
            var globalOptions = context.Options.AnalyzerConfigOptionsProvider.GlobalOptions;

            string? silentEnumList = null;
            globalOptions?.TryGetValue("build_property.SwitchCompletenessMuteEnums", out silentEnumList);

            if (string.IsNullOrWhiteSpace(silentEnumList))
            {
                return Delimiter;
            }

            if (!silentEnumList!.EndsWith(Delimiter))
            {
                //System.IO.File.AppendAllText(@"C:\temp\_a.txt", ((silentEnumList + Delimiter)?.ToString() ?? "no silentEnumList + Delimiter") + Environment.NewLine);
                return silentEnumList + Delimiter;
            }

            return silentEnumList;
        }

    }
}
