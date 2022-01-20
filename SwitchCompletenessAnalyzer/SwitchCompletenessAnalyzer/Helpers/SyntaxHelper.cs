using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SwitchCompletenessAnalyzer.Helpers
{
    internal static class SyntaxHelper
    {
        public static IReadOnlyDictionary<long, ISymbol> GetLabelSymbols(
            this SwitchStatementSyntax switchStatement,
            SemanticModel model,
            CancellationToken cancellationToken)
        {
            if (switchStatement is null)
            {
                throw new ArgumentNullException(nameof(switchStatement));
            }

            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var labelSymbols = new Dictionary<long, ISymbol>();

            var caseLabels = switchStatement.GetCaseSwitchLabels();
            foreach (var label in caseLabels)
            {
                if (!(model.GetSymbolInfo(label, cancellationToken).Symbol is IFieldSymbol fieldSymbol))
                {
                    // something is wrong with the label and the SemanticModel was unable to determine its symbol
                    // or the symbol is not a field symbol which should be for case labels of switchs on enum types
                    // abort analyzer
                    return new Dictionary<long, ISymbol>();
                }

                var enumValue = fieldSymbol.ConstantValue.ToInt64();
                labelSymbols.Add(enumValue, fieldSymbol);
            }

            return labelSymbols;
        }

        public static List<ExpressionSyntax> GetCaseSwitchLabels(
            this SwitchStatementSyntax switchStatement
            )
        {
            if (switchStatement is null)
            {
                throw new ArgumentNullException(nameof(switchStatement));
            }

            var caseLabels = new List<ExpressionSyntax>();

            foreach (var section in switchStatement.Sections)
            {
                foreach (var label in section.Labels)
                {
                    if (label is CaseSwitchLabelSyntax caseLabel)
                    {
                        caseLabels.Add(caseLabel.Value);
                    }
                }
            }

            return caseLabels;
        }

    }
}
