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
        private static readonly Dictionary<long, ISymbol> _emptyDictionary = new Dictionary<long, ISymbol>();

        #region switch expression

        public static IReadOnlyDictionary<long, ISymbol> GetLabelSymbols(
            this SwitchExpressionSyntax switchExpression,
            SemanticModel model,
            CancellationToken cancellationToken)
        {
            if (switchExpression is null)
            {
                throw new ArgumentNullException(nameof(switchExpression));
            }

            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var caseLabels = switchExpression.GetCaseSwitchLabels();
            if (caseLabels == null)
            {
                return _emptyDictionary;
            }

            var labelSymbols = ProvideLabelSymbols(
                model,
                cancellationToken,
                caseLabels
                );

            return labelSymbols;
        }

        public static List<ExpressionSyntax>? GetCaseSwitchLabels(
            this SwitchExpressionSyntax switchExpression
            )
        {
            if (switchExpression is null)
            {
                throw new ArgumentNullException(nameof(switchExpression));
            }

            List<ExpressionSyntax>? caseLabels = null;

            foreach (SwitchExpressionArmSyntax arm in switchExpression.Arms)
            {
                if (arm.Pattern is ConstantPatternSyntax cps)
                {
                    if (caseLabels == null)
                    {
                        caseLabels = new List<ExpressionSyntax>();
                    }

                    caseLabels.Add(cps.Expression);
                }
            }

            return caseLabels;
        }

        #endregion

        #region switch statement
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

            var caseLabels = switchStatement.GetCaseSwitchLabels();
            if (caseLabels == null)
            {
                return _emptyDictionary;
            }

            var labelSymbols = ProvideLabelSymbols(
                model,
                cancellationToken,
                caseLabels
                );

            return labelSymbols;
        }

        public static IReadOnlyList<ExpressionSyntax>? GetCaseSwitchLabels(
            this SwitchStatementSyntax switchStatement
            )
        {
            if (switchStatement is null)
            {
                throw new ArgumentNullException(nameof(switchStatement));
            }

            List<ExpressionSyntax>? caseLabels = null;

            foreach (var section in switchStatement.Sections)
            {
                var labels = section.Labels;
                if (labels != null)
                {
                    foreach (var label in labels)
                    {
                        if (label is CasePatternSwitchLabelSyntax casePatternLabel)
                        {
                            if (caseLabels == null)
                            {
                                caseLabels = new List<ExpressionSyntax>();
                            }

                            if (casePatternLabel.Pattern is ConstantPatternSyntax cps)
                            {
                                caseLabels.Add(cps.Expression);
                            }
                        }
                        if (label is CaseSwitchLabelSyntax caseLabel)
                        {
                            if (caseLabels == null)
                            {
                                caseLabels = new List<ExpressionSyntax>();
                            }

                            caseLabels.Add(caseLabel.Value);
                        }
                    }
                }
            }

            return caseLabels;
        }

        #endregion

        #region private code

        private static IReadOnlyDictionary<long, ISymbol> ProvideLabelSymbols(
            SemanticModel model,
            CancellationToken cancellationToken,
            IReadOnlyList<ExpressionSyntax> caseLabels
            )
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (caseLabels is null)
            {
                throw new ArgumentNullException(nameof(caseLabels));
            }

            var labelSymbols = new Dictionary<long, ISymbol>(caseLabels.Count); //set capacity to prevent possible reallocations

            foreach (var label in caseLabels)
            {
                if (!(model.GetSymbolInfo(label, cancellationToken).Symbol is IFieldSymbol fieldSymbol))
                {
                    // something is wrong with the label and the SemanticModel was unable to determine its symbol
                    // or the symbol is not a field symbol which should be for case labels of switchs on enum types
                    // abort analyzer
                    return _emptyDictionary;
                }
                if (fieldSymbol.ConstantValue == null)
                {
                    //something wrong with access to value of the enum
                    return _emptyDictionary;
                }

                var enumValue = fieldSymbol.ConstantValue.ToInt64();
                labelSymbols.Add(enumValue, fieldSymbol);
            }

            return labelSymbols;
        }

        #endregion
    }
}
