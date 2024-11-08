using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataBindingGenerator
{
    public static class SymbolExtension
    {
        private static readonly SymbolDisplayFormat format = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                    | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        public static string Display(this ITypeSymbol symbol)
        {
            return symbol?.ToDisplayString(format) ?? string.Empty;
        }
    }
}
