using LogAspectSG.Diagnostics;
using Xunit;
using Fixer = LogAspectSGTest.Helpers.CSharpCodeFixVerifier<LogAspectSG.Analyzer.InterceptorStoreAnalyzer, LogAspectSG.CodeFix.TypeCodeFix>;

namespace LogAspectSGTest.Tests
{
    public class ReturnTypeNotNullableTests
    {
        //No diagnostics expected to show up
        [Fact]
        public async Task ReturnTypeNotNullableEmpty()
        {
            string test = @"";

            await Fixer.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task ReturnTypeNotNullableFix()
        {
            string test = @$"
using System;

namespace Test
{{
    public class TestClass
    {{
        public static string? Test => TestMethod(""Test"");

        public static {{|{GeneratorDiagnostic.ReturnTypeNotNullable.Id}:string|}} TestMethod(string value)
        {{
            return value;
        }}
    }}
}}
";

            string fix = @"
using System;

namespace Test
{
    public class TestClass
    {
        public static string? Test => TestMethod(""Test"");

        public static string? TestMethod(string value)
        {
            return value;
        }
    }
}
";
            await Fixer.VerifyCodeFixAsync(test, fix);
        }
    }
}