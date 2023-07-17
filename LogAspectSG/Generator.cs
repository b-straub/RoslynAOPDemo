﻿using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using LogAspectSG.Engine;
using Microsoft.CodeAnalysis;

namespace LogAspectSG
{
    [Generator]
    public class Generator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var md = context.MetadataReferencesProvider.Collect();

            IncrementalValuesProvider<AdditionalText> logFile = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith("LogEntries.txt"));

            IncrementalValuesProvider<string> logContent = logFile
                .Select((text, cancellationToken) => text.GetText(cancellationToken)?.ToString())
                .Where(static t => t is not null)
                .Select((s, _) => s!.ToLowerInvariant());

            IncrementalValuesProvider<InterceptorRecordBase> records = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (syntaxNode, _) => syntaxNode.MatchDeclaration(),
                    transform: static (context, ct) => context.GetBaseRecords(ct))
                .Where(static i => i is not null)
                .Select((t, _) => t!)
                .Combine(logContent.Collect())
                .Where(c => SymbolMatcher.FilterTypes(c.Left, c.Right))
                .Select((p, _) => p.Left);

            IncrementalValueProvider<(Compilation, ImmutableArray<InterceptorRecordBase>)> compilationWithRecords
                = context.CompilationProvider.Combine(records.Collect());

            // Generate the source using the compilation and declarations
            context.RegisterImplementationSourceOutput(compilationWithRecords,
                static (context, compilationWithRecords) => AddInterceptors(context, compilationWithRecords));
        }

        private static void AddInterceptors(SourceProductionContext context, (Compilation compilation, ImmutableArray<InterceptorRecordBase> records) compilationWithRecords)
        {
            var interceptorRecords = compilationWithRecords.records.InterceptorRecords(compilationWithRecords.compilation, context.CancellationToken)
                .Where(r => !r.InMethod);

            var nameSpaces = interceptorRecords
                .Select(r => r.Method.ContainingType.ContainingNamespace.Name).Distinct();

            var name = Assembly.GetExecutingAssembly().GetName().Name;
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            string generated = $@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by {name} Version: {version}
// </auto-generated>
//------------------------------------------------------------------------------

";
            string source = string.Empty;

            foreach (string? nameSpace in nameSpaces)
            {
                var usedNS = nameSpace == "<global namespace>" ? "GlobalNamespace" : nameSpace;
                source = interceptorRecords.DumpNamespace(usedNS);
               
                string sourceName = $"{usedNS}.Generated.cs";

                if (source.Length != 0)
                {
                    context.AddSource(sourceName, generated + source);
                }
            }
        }
    }
}
