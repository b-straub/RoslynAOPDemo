using LogAspectSG.Diagnostics;
using LogAspectSG.Engine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LogAspectSG.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InterceptorStoreAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor[] Diagnostics =
        {
            GeneratorDiagnostic.ReturnTypeNotNullable
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Diagnostics.ToImmutableArray();

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeDeclaration, SyntaxKind.MethodDeclaration);
        }

        private static IEnumerable<string>? _logs;

        private static void AnalyzeDeclaration(SyntaxNodeAnalysisContext context)
        {
            MethodDeclarationSyntax? member = context.Node as MethodDeclarationSyntax;

            if (member is not null)
            {
                InterceptorStoreBase? storeBase = member.CreateInterceptorStoreBase(context.SemanticModel, context.CancellationToken);

                if (_logs is null)
                {
                    IEnumerable<AdditionalText> logFiles = context.Options.AdditionalFiles.Where(static file => file.Path.EndsWith("LogEntries.txt"));
                    _logs = logFiles
                        .Select((text, cancellationToken) => text.GetText(context.CancellationToken)?.ToString())
                        .Where(static t => t is not null)
                        .Select((s, _) => s!.ToLowerInvariant());
                }

                if (storeBase is not null && Collector.FilterType(storeBase, _logs))
                {
                    System.Collections.Generic.IEnumerable<GeneratorDiagnostic> diagnostics = storeBase.Method.Verify(context.Compilation);
                    foreach (GeneratorDiagnostic diagnostic in diagnostics)
                    {
                        diagnostic.ReportDiagnostic(context.ReportDiagnostic);
                    }
                }
            }
        }
    }
}
