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
            var parent = baseRecord.Identifier.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            var inMethod = parent is not null && parent.Identifier.ValueText == baseRecord.Identifier.Identifier.ValueText;

            var path = baseRecord.Identifier.SyntaxTree.GetInterceptorFilePath(compilation);
            var linePosition = baseRecord.Identifier.SyntaxTree.GetLineSpan(baseRecord.Identifier.Span, cancellationToken).StartLinePosition;

            return new(baseRecord.Method, baseRecord.Identifier, inMethod, path, linePosition.Line, linePosition.Character);
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
