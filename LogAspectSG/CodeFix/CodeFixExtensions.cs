using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace LogAspectSG.CodeFix
{
    internal static class GeneratorDiagnosticExtensions
    {
        public static bool EqualsId(this DiagnosticDescriptor source, DiagnosticDescriptor other)
        {
            return source.Id == other.Id;
        }

        public static string CodeFixMessage(this DiagnosticDescriptor source, params string[] parameters)
        {
            string message = source.CustomTags.Any() ? source.CustomTags.ElementAt(0) : string.Empty;
            if (parameters.Any())
            {
                message = string.Format(message, parameters);
            }

            return message;
        }

        public static string? IdentifierName(this SyntaxNode node)
        {
            string? name = node.Kind() switch
            {
                SyntaxKind.Parameter => ((ParameterSyntax)node).Identifier.ValueText,
                SyntaxKind.PropertyDeclaration => ((PropertyDeclarationSyntax)node).Identifier.ValueText,
                _ => null,
            };

            return name;
        }

        public static Document MakeNullable(this Document document, SyntaxNode root, TypeSyntax typeSyntax)
        {
            NullableTypeSyntax nullableType = SyntaxFactory.NullableType(typeSyntax.WithoutTrivia()).WithTriviaFrom(typeSyntax);

            return document.WithSyntaxRoot(root.ReplaceNode(
                        typeSyntax, nullableType));
        }
    }
}
