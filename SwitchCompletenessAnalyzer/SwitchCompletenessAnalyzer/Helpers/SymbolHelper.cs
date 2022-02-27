using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace SwitchCompletenessAnalyzer.Helpers
{
    internal static class SymbolHelper
    {
        public static IReadOnlyDictionary<long, List<ISymbol>> GetAllPossibleEnumSymbols(
            this INamedTypeSymbol enumType
            )
        {
            if (enumType is null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            var enumMembers = enumType.GetMembers();
            var enumValues = new Dictionary<long, List<ISymbol>>(enumMembers.Length); //set capacity to prevent possible reallocations

            foreach (var enumMember in enumMembers)
            {
                // skip '.ctor' and '__value'
                var fieldSymbol = enumMember as IFieldSymbol;
                if (fieldSymbol == null || fieldSymbol.Type.SpecialType != SpecialType.None)
                {
                    continue;
                }

                if (fieldSymbol.ConstantValue == null)
                {
                    // We have an enum that has problems with it (i.e. non-const members).  We won't
                    // be able to determine properly if the switch is complete.  Assume it is so we
                    // don't offer to do anything.
                    return new Dictionary<long, List<ISymbol>>();
                }

                // Multiple enum members may have the same value.
                var enumValue = fieldSymbol.ConstantValue.ToInt64();
                if (!enumValues.ContainsKey(enumValue))
                {
                    enumValues.Add(enumValue, new List<ISymbol> { fieldSymbol });
                }
                else
                {
                    enumValues[enumValue].Add(fieldSymbol);
                }
            }

            return enumValues;
        }

    }
}
