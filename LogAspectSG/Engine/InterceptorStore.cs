using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace LogAspectSG.Engine
{
    internal record InterceptorStoreBase(IMethodSymbol Method, SimpleNameSyntax? NameSyntax);

    internal record InterceptorStore(IMethodSymbol Method, SimpleNameSyntax? NameSyntax, bool InMethod, string Path, int Line, int Character) :
        InterceptorStoreBase(Method, NameSyntax);

    internal static class StoreExtensions
    {
        public static string DumpStore(this InterceptorStore store)
        {
            StringBuilder sb = new();
            string returnType = store.Method.ReturnType.ToString();
            string taT = MakeTypeArguments(store.Method.ContainingType.TypeArguments);

            _ = store.Method.IsStatic
                ? sb.Append($@"
        [InterceptsLocation(@""{store.Path}"", {store.Line + 1}, {store.Character + 1})]
        {store.Method.ContainingType.AccessToString()} static {returnType} Intercept{store.Method.Name}_{store.Line}{store.Character}({store.MakeParameters()})
        {{
            {store.MakeMethod(store.Method.ReturnsVoid, true)}
        }}
")
                : sb.Append($@"
        [InterceptsLocation(@""{store.Path}"", {store.Line + 1}, {store.Character + 1})]
        {store.Method.ContainingType.AccessToString()} static {returnType} Intercept{store.Method.Name}_{store.Line}{store.Character}(this {store.Method.ContainingType.Name}{taT} type, {store.MakeParameters()})
        {{
            {store.MakeMethod(store.Method.ReturnsVoid, false)}
        }}
");

            return sb.ToString();
        }

        private static readonly char[] trim = new[] { ' ', ',', '.' };

        public static string MakeParameters(this InterceptorStore store)
        {
            StringBuilder sb = new();

            foreach (IParameterSymbol parameter in store.Method.Parameters)
            {
                _ = sb.Append(parameter.ToDisplayString());
                _ = sb.Append(", ");
            }

            return sb.ToString().TrimEnd(trim);
        }

        public static string MakeStaticType(this InterceptorStore store)
        {
            INamedTypeSymbol? typeSymbol = store.Method.ContainingType;
            string typeName = string.Empty;

            while (typeSymbol is not null && typeSymbol.ContainingNamespace.Equals(store.Method.ContainingType.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                string tn = typeSymbol.Name;
                string taT = MakeTypeArguments(typeSymbol.TypeArguments);
                tn += taT;
                tn += ".";

                typeName = tn + typeName;

                typeSymbol = typeSymbol.ContainingType;
            }

            return typeName.TrimEnd(trim);
        }

        private static string MakeTypeArguments(this ImmutableArray<ITypeSymbol> typeArguments)
        {
            string taT = string.Empty;

            if (typeArguments.Any())
            {
                taT += "<";

                foreach (ITypeSymbol ta in typeArguments)
                {
                    taT += $"{ta}, ";
                }

                taT = taT.TrimEnd(trim);
                taT += ">";
            }

            return taT;
        }

        public static string MakeMethod(this InterceptorStore store, bool isVoid, bool isStatic)
        {
            StringBuilder sbm = new();
            string returnType = store.Method.ReturnType.ToString();

            if (!isVoid)
            {
                _ = sbm.Append("result = ");
            }

            _ = isStatic ? sbm.Append(store.MakeStaticType()) : sbm.Append("type");

            _ = sbm.Append('.');
            _ = sbm.Append(store.Method.Name);
            string taM = MakeTypeArguments(store.Method.TypeArguments);
            _ = sbm.Append(taM);
            _ = sbm.Append('(');

            StringBuilder sbp = new();

            foreach (IParameterSymbol parameter in store.Method.Parameters)
            {
                _ = sbp.Append(parameter.Name);
                _ = sbp.Append(", ");
            }

            _ = sbm.Append(sbp.ToString().TrimEnd(trim));

            _ = sbm.Append(");");

            StringBuilder sb = new();

            if (!isVoid)
            {
                _ = sb.Append($@"
            {returnType} result = null;
");
            }

            _ = sb.Append($@"
            try
            {{
                {sbm}
                Console.WriteLine($""Success for {store.Method.Name} in {Path.GetFileName(store.Path)} Line {store.Line + 1} Character {store.Character + 1}"");
            }}
            catch(Exception e)
            {{
                Console.WriteLine($""Error {{e.Message}} for {store.Method.Name} in {Path.GetFileName(store.Path)} Line {store.Line + 1} Character {store.Character + 1}"");
                throw;
            }}
");

            if (!isVoid)
            {
                _ = sb.Append(@"
            return result;");
            }

            return sb.ToString();
        }
    }
}
