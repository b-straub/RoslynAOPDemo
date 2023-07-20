using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace LogAspectSG.Engine
{
    internal static class Collector
    {
        public static bool MatchDeclaration(this SyntaxNode node)
        {
            return node is InvocationExpressionSyntax;
        }

        public static InterceptorStoreBase? GetInterceptorBaseStores(this GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;

            return invocation.CreateInterceptorStoreBase(context.SemanticModel, cancellationToken);
        }

        public static InterceptorStoreBase? CreateInterceptorStoreBase(this InvocationExpressionSyntax invocation, SemanticModel model, CancellationToken cancellationToken)
        {
            SymbolInfo invocationSymbolInfo = model.GetSymbolInfo(invocation.Expression, cancellationToken);

            return invocationSymbolInfo.Symbol.CreateInterceptorStoreBase(invocation);
        }

        public static InterceptorStoreBase? CreateInterceptorStoreBase(this MemberDeclarationSyntax member, SemanticModel model, CancellationToken cancellationToken)
        {
            ISymbol? memberSymbolInfo = model.GetDeclaredSymbol(member, cancellationToken);

            return memberSymbolInfo.CreateInterceptorStoreBase(member);
        }

        private static InterceptorStoreBase? CreateInterceptorStoreBase(this ISymbol? symbol, SyntaxNode node)
        {
            if (symbol is IMethodSymbol methodSymbol)
            {
                IEnumerable<SimpleNameSyntax> names = node.DescendantNodesAndSelf().OfType<SimpleNameSyntax>();
                SimpleNameSyntax? name = names.Where(i => i.Identifier.ValueText == methodSymbol.Name).FirstOrDefault();

                return new(methodSymbol, name);
            }

            return null;
        }

        private static readonly Regex r_replaceMethodParts = new(pattern: @"[<(][[\w,.\[\]? ]*[>)]", options: RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool FilterType(InterceptorStoreBase storeBase, IEnumerable<string> log)
        {
            string methodName = storeBase.Method.ToDisplayString();
            string methodSearch = r_replaceMethodParts.Replace(methodName, string.Empty).ToLowerInvariant();

            if (storeBase.Method.TypeArguments.Any())
            {
                methodSearch += $"`{storeBase.Method.TypeArguments.Length}";
            }

            return log.Any(l => l.Contains(methodSearch));
        }

        public static IEnumerable<InterceptorStore> CreateInterceptorStores(this IEnumerable<InterceptorStoreBase> baseRecords, Compilation compilation, CancellationToken cancellationToken)
        {
            return baseRecords
               .Select(ma => ma.CreateInterceptorStore(compilation, cancellationToken))
               .WhereNotNull();
        }

        public static InterceptorStore? CreateInterceptorStore(this InterceptorStoreBase storeBase, Compilation compilation, CancellationToken cancellationToken)
        {
            if (storeBase.NameSyntax is null)
            {
                throw new ArgumentNullException(nameof(storeBase.NameSyntax), nameof(storeBase.NameSyntax));
            }

            MethodDeclarationSyntax? parent = storeBase.NameSyntax.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            bool inMethod = parent is not null && parent.Identifier.ValueText == storeBase.NameSyntax.Identifier.ValueText;

            string path = storeBase.NameSyntax.SyntaxTree.GetInterceptorFilePath(compilation);
            Microsoft.CodeAnalysis.Text.LinePosition linePosition = storeBase.NameSyntax.SyntaxTree.GetLineSpan(storeBase.NameSyntax.Span, cancellationToken).StartLinePosition;

            return new(storeBase.Method, storeBase.NameSyntax, inMethod, path, linePosition.Line, linePosition.Character);
        }

        private static string GetInterceptorFilePath(this SyntaxTree tree, Compilation compilation)
        {
            return compilation.Options.SourceReferenceResolver?.NormalizePath(tree.FilePath, baseFilePath: null) ?? tree.FilePath;
        }
    }
}
