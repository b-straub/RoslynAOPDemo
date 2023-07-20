using LogAspectSG.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace LogAspectSG.CodeFix
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeCodeFix)), Shared]
    internal class TypeCodeFix : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(GeneratorDiagnostic.ReturnTypeNotNullable.Id);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics.First();

            if (!FixableDiagnosticIds.Contains(diagnostic.Id))
            {
                return;
            }

            SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (root is null)
            {
                throw new ArgumentNullException(nameof(root), nameof(root));
            }

            Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
            string? codeFixMessage = null;

            SyntaxNode? node = root.FindToken(diagnosticSpan.Start).Parent;
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node), nameof(node));
            }

            TypeSyntax? typeSyntax = null;
            string? name = node.ToString();
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name), nameof(name));
            }

            switch (diagnostic.Descriptor)
            {

                case var _ when diagnostic.Descriptor.EqualsId(GeneratorDiagnostic.ReturnTypeNotNullable):

                    // index must be a type
                    typeSyntax = node.DescendantNodesAndSelf().OfType<TypeSyntax>().Last();

                    if (typeSyntax is null)
                    {
                        throw new ArgumentNullException(nameof(typeSyntax), nameof(typeSyntax));
                    }

                    codeFixMessage = diagnostic.Descriptor.CodeFixMessage(name);
                    if (string.IsNullOrEmpty(codeFixMessage))
                    {
                        throw new ArgumentException("No title for: " + diagnostic.Descriptor.Id);
                    }

                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: codeFixMessage,
                            createChangedDocument: _ => Task.FromResult(context.Document.MakeNullable(root, typeSyntax)),
                            equivalenceKey: diagnostic.Descriptor.Id),
                        diagnostic);

                    break;
            }
        }
    }
}
