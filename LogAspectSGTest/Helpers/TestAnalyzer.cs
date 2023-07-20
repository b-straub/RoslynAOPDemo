using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace LogAspectSGTest.Helpers
{
    internal static class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        public static DiagnosticResult Diagnostic()
        {
            return CSharpCodeFixVerifier<TAnalyzer, TCodeFix, XUnitVerifier>.Diagnostic();
        }

        public static DiagnosticResult Diagnostic(string diagnosticId)
        {
            return CSharpCodeFixVerifier<TAnalyzer, TCodeFix, XUnitVerifier>.Diagnostic(diagnosticId);
        }

        public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        {
            return new(descriptor);
        }

        public static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            Test<TAnalyzer, TCodeFix> test = new() { TestCode = source };

            IEnumerable<(string Name, Microsoft.CodeAnalysis.Text.SourceText Source)> generatedSources = GeneratorFactory.RunGenerator(source, AdditionalTextProvider.GetAdditionalTexts());
            test.TestState.GeneratedSources.AddRange(generatedSources);

            test.ExpectedDiagnostics.AddRange(expected);
            return test.RunAsync();
        }

        public static Task VerifyCodeFixAsync(string source, string fixedSource, int? codeActionIndex = null)
        {
            return VerifyCodeFixAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource, codeActionIndex);
        }

        public static Task VerifyCodeFixAsync(string source, DiagnosticResult expected, string fixedSource, int? codeActionIndex = null)
        {
            return VerifyCodeFixAsync(source, new[] { expected }, fixedSource, codeActionIndex);
        }

        public static Task VerifyCodeFixAsync(string source, DiagnosticResult[] expected, string fixedSource, int? codeActionIndex = null)
        {
            Test<TAnalyzer, TCodeFix> test = new()
            {
                TestCode = source,
                FixedCode = fixedSource,
                CodeActionIndex = codeActionIndex,
                CodeFixTestBehaviors = codeActionIndex is not null ? CodeFixTestBehaviors.SkipFixAllCheck : CodeFixTestBehaviors.None
            };

            IEnumerable<(string Name, Microsoft.CodeAnalysis.Text.SourceText Source)> generatedSources = GeneratorFactory.RunGenerator(source, AdditionalTextProvider.GetAdditionalTexts());
            test.TestState.GeneratedSources.AddRange(generatedSources);

            test.ExpectedDiagnostics.AddRange(expected);
            return test.RunAsync();
        }
    }

    internal class Test<TAnalyzer, TCodeFix> : CSharpCodeFixTest<TAnalyzer, TCodeFix, XUnitVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        protected override AnalyzerOptions GetAnalyzerOptions(Project project)
        {
            AnalyzerOptions options = base.GetAnalyzerOptions(project).WithAdditionalFiles(AdditionalTextProvider.GetAdditionalTexts());
            return options;
        }
    }
}