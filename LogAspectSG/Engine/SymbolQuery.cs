using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

namespace LogAspectSG.Engine
{
    internal static class SymbolQuery
    {
        public static InterceptorRecord? CreateInterceptorRecord(this InterceptorRecordBase baseRecord, Compilation compilation, CancellationToken cancellationToken)
        {
            var parent = baseRecord.NameSyntax.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            var inMethod = parent is not null && parent.Identifier.ValueText == baseRecord.NameSyntax.Identifier.ValueText;

            var path = baseRecord.NameSyntax.SyntaxTree.GetInterceptorFilePath(compilation);
            var linePosition = baseRecord.NameSyntax.SyntaxTree.GetLineSpan(baseRecord.NameSyntax.Span, cancellationToken).StartLinePosition;

            return new(baseRecord.Method, baseRecord.NameSyntax, inMethod, path, linePosition.Line, linePosition.Character);
        }

        public static IEnumerable<InterceptorRecord> InterceptorRecords(this IEnumerable<InterceptorRecordBase> baseRecords, Compilation compilation, CancellationToken cancellationToken)
        {
            return baseRecords
               .Select(ma => ma.CreateInterceptorRecord(compilation, cancellationToken))
               .WhereNotNull();
        }

        private static string GetInterceptorFilePath(this SyntaxTree tree, Compilation compilation)
        {
            return compilation.Options.SourceReferenceResolver?.NormalizePath(tree.FilePath, baseFilePath: null) ?? tree.FilePath;
        }
    }
}
