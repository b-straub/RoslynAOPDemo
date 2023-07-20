using LogAspectSG;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace LogAspectSGTest.Helpers
{
    internal static class AdditionalTextProvider
    {
        private static readonly string[] additionalTextPaths = {
            @"..\..\..\LogEntries.txt",
        };

        public static ImmutableArray<AdditionalText> GetAdditionalTexts()
        {
            return additionalTextPaths.Select(t => (AdditionalText)new CustomAdditionalText(t)).ToImmutableArray();
        }
    }

    internal class CustomAdditionalText(string path) : AdditionalText
    {
        private readonly string _text = File.ReadAllText(path);

        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = new CancellationToken())
        {
            return SourceText.From(_text);
        }
    }

    internal class GeneratorFactory
    {
        public static IEnumerable<(string Name, SourceText Source)> RunGenerator(string source, IEnumerable<AdditionalText> additionalTexts)
        {
            List<SyntaxTree> syntaxTrees = new();

            string? st = source;
            SyntaxTree? syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(st, Encoding.UTF8), CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11));
            syntaxTrees.Add(syntaxTree);

            CSharpCompilationOptions? compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable)
                .WithOptimizationLevel(OptimizationLevel.Debug)
                .WithGeneralDiagnosticOption(ReportDiagnostic.Default);

            IEnumerable<MetadataReference> references = AppDomain.CurrentDomain.GetAssemblies()
                               .Where(assembly => !assembly.IsDynamic)
                               .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
                               .Cast<MetadataReference>();

            Compilation compilation = CSharpCompilation.Create("testgenerator", syntaxTrees, references, compilationOptions);
            CSharpParseOptions? parseOptions = syntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions;

            Generator? generator = new();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(ImmutableArray.Create(generator.AsSourceGenerator()), additionalTexts, parseOptions: parseOptions);
            _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation? generatorCompilation, out ImmutableArray<Diagnostic> generatorDiagnostics);

            string? t = generatorCompilation.SyntaxTrees.FirstOrDefault()?.ToString();
            return generatorCompilation.SyntaxTrees.Skip(1).Select((s, i) => ($"Generated{i}", SourceText.From(s.ToString())));
        }
    }
}
