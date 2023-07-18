using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using System.Text.RegularExpressions;

namespace LogAspectSG.Engine
{
    internal static class SymbolMatcher
    {
        public static bool MatchDeclaration(this SyntaxNode node)
        {
            return node is InvocationExpressionSyntax;
        }

        public static InterceptorRecordBase? GetBaseRecords(this GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var invocationSymbolInfo = context.SemanticModel.GetSymbolInfo(invocation.Expression, cancellationToken);

            if (invocationSymbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                var names = invocation.DescendantNodesAndSelf().OfType<SimpleNameSyntax>();
                var name = names.Where(i => i.Identifier.ValueText == methodSymbol.Name).FirstOrDefault();

                if (name is not null)
                {
                    return new(methodSymbol, name);
                }
            }

            return null;
        }

        private static readonly Regex r_replaceMethodParts = new(pattern: @"[<(][[\w,.\[\]? ]*[>)]", options: RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool FilterTypes(InterceptorRecordBase record, ImmutableArray<string> log)
        {
            var methodName = record.Method.ToDisplayString();
            var methodSearch = r_replaceMethodParts.Replace(methodName, string.Empty).ToLowerInvariant();

            if (record.Method.TypeArguments.Any())
            {
                methodSearch += $"`{record.Method.TypeArguments.Length}";
            }

            return log.Any(l => l.Contains(methodSearch));
        }
    }
}
