using LogAspectSG.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace LogAspectSG.Engine
{
    internal static class Verifier
    {
        public static IEnumerable<GeneratorDiagnostic> Verify(this InterceptorStore store, Compilation _)
        {
            List<GeneratorDiagnostic> diagnostics = new();

            diagnostics.AddRange(store.Method.Verify(_));

            return diagnostics;
        }

        public static IEnumerable<GeneratorDiagnostic> Verify(this IMethodSymbol method, Compilation _)
        {
            List<GeneratorDiagnostic> diagnostics = new();

            if (!method.ReturnsVoid && method.ReturnNullableAnnotation is not NullableAnnotation.Annotated)
            {
                MethodDeclarationSyntax? md = (MethodDeclarationSyntax?)method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

                if (md is not null)
                {
                    Location? loc = md.ReturnType.GetLocation();
                    if (loc is not null)
                    {
                        diagnostics.Add(new GeneratorDiagnostic(GeneratorDiagnostic.ReturnTypeNotNullable, loc, md.ReturnType.ToString()));
                    }
                }
            }

            return diagnostics;
        }
    }
}