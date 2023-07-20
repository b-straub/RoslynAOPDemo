using Microsoft.CodeAnalysis;

#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)' doesn't work with target typed new

namespace LogAspectSG.Diagnostics
{
    internal partial class GeneratorDiagnostic
    {
        #region General
        public static DiagnosticDescriptor Internal =
            new DiagnosticDescriptor(
                "LASG000",
                "Internal error",
                "Internal error '{0}'",
                "LAGGenerator",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public static DiagnosticDescriptor Success =
            new DiagnosticDescriptor(
                "LASG001",
                "Generator success",
                "'{0}': successfully created 'LogAspectInterceptors'",
                "LAGGenerator",
                DiagnosticSeverity.Info,
                isEnabledByDefault: true);

        public static DiagnosticDescriptor SyntaxReceiver =
           new DiagnosticDescriptor(
                "LASG002",
                "SyntaxReceiver error",
                "SyntaxReceiver creation failed",
                "LAGGenerator",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public static DiagnosticDescriptor Error =
            new DiagnosticDescriptor(
                "LASG003",
                "Generator error",
                "'{0}': failed to create 'LogAspectInterceptors' check build and analyzer errors",
                "LAGGenerator",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);
        #endregion General

        #region InterceptorRecord
        public static DiagnosticDescriptor ReturnTypeNotNullable =
            new(
                "LASG100",
                "Method error: Return type is not nullable",
                "Return type '{0}' is not nullable",
                "LAGGenerator",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                "Return type is not nullable.",
                null,
                "Make '{0}' nullable");
        #endregion
    }
}
#pragma warning restore IDE0090 // Use 'new DiagnosticDescriptor(...)'

