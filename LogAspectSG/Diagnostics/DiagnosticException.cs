using LogAspectSG.Engine;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LogAspectSG.Diagnostics
{
    internal partial class GeneratorDiagnostic
    {
        private readonly DiagnosticDescriptor _descriptor;
        private readonly Location? _location;
        private readonly object?[]? _messageArgs;
        private readonly Dictionary<string, string?> _properties = new();

        public string Id => _descriptor.Id;

        public GeneratorDiagnostic(DiagnosticDescriptor descriptor, params string?[] message)
        {
            _descriptor = descriptor;
            if (message is not null)
            {
                _messageArgs = message;
            }
        }

        public GeneratorDiagnostic(DiagnosticDescriptor descriptor, Location location, params string?[] message)
        {
            _descriptor = descriptor;
            _location = location;

            if (message is not null)
            {
                _messageArgs = message;
            }

            if (message is not null)
            {
                _messageArgs = message;
            }
        }

        public GeneratorDiagnostic(DiagnosticDescriptor descriptor, Location location, KeyValuePair<string, string?> property)
        {
            _descriptor = descriptor;
            _location = location;
            AddProperties(property);
        }

        public GeneratorDiagnostic(DiagnosticDescriptor descriptor, IMethodSymbol symbol)
        {
            _descriptor = descriptor;
            _location = symbol.Locations.First();
            _messageArgs = new[] { symbol.Name };
        }

        public GeneratorDiagnostic(DiagnosticDescriptor descriptor, IMethodSymbol symbol, params string?[] message)
        {
            _descriptor = descriptor;
            _location = symbol.Locations.First();
            List<string?>? messages = new()
            {
                symbol.Name
            };
            messages.AddRange(message);

            _messageArgs = messages.ToArray();
        }

        public GeneratorDiagnostic(DiagnosticDescriptor descriptor, InterceptorStore iRecord)
        {
            _descriptor = descriptor;
            _location = iRecord.Method.Locations.First();
            _messageArgs = new[] { iRecord.Method.Name };
        }

        public GeneratorDiagnostic(DiagnosticDescriptor descriptor, InterceptorStore iRecord, params string?[] message)
        {
            _descriptor = descriptor;
            _location = iRecord.Method.Locations.First();
            List<string?>? messages = new()
            {
                iRecord.Method.Name
            };
            messages.AddRange(message);

            _messageArgs = messages.ToArray();
        }

        public void ReportDiagnostic(Action<Diagnostic> reportDiagnostics)
        {
            reportDiagnostics(Diagnostic.Create(_descriptor, _location, _properties.ToImmutableDictionary(), _messageArgs));
        }

        public void AddProperties(KeyValuePair<string, string?> property)
        {
            _properties.Add(property.Key, property.Value);
        }
    }

    internal class DiagnosticException : Exception
    {
        public GeneratorDiagnostic Diagnostic { get; }

        public DiagnosticException()
        {
            Diagnostic = new(GeneratorDiagnostic.Internal, "Unknown error");
        }

        public DiagnosticException(string message)
            : base(message)
        {
            Diagnostic = new(GeneratorDiagnostic.Internal, message);
        }

        public DiagnosticException(string message, Exception inner)
            : base(message, inner)
        {
            Diagnostic = new(GeneratorDiagnostic.Internal, message);
        }

        public DiagnosticException(GeneratorDiagnostic diagnostic)
        {
            Diagnostic = diagnostic;
        }
    }
}
