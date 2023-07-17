using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;

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
                var identifiers = invocation.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>();
                var identifier = identifiers.Where(i => i.Identifier.ValueText == methodSymbol.Name).FirstOrDefault();

                if (identifier is not null)
                {
                    return new(methodSymbol, identifier);
                }
            }

            return null;
        }

        public static bool FilterTypes(InterceptorRecordBase record, ImmutableArray<string> log)
        {
            var methodName = record.Method.ToDisplayString().CutBefore('(').ToLowerInvariant();
            return log.Any(l => l.Contains(methodName));
        }
    }
}
