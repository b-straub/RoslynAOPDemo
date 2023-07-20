namespace LogAspectDemo
{
    internal class InterceptableBaseType()
    {
        public virtual void MyMethod(string caller, string param, string? secondLine = null, params string[]? additionalLines)
        {
            Console.WriteLine($"Hello from {caller} {param}");
            if (secondLine is not null)
            {
                Console.WriteLine(secondLine);
            }

            if (additionalLines is not null)
            {
                foreach (string additionalLine in additionalLines)
                {
                    Console.WriteLine(additionalLine);
                }
            }
        }
    }

    internal class InterceptableDerivedType() : InterceptableBaseType
    {
        public override void MyMethod(string caller, string param, string? secondLine = null, params string[]? additionalLines)
        {
            base.MyMethod(caller, param, secondLine, additionalLines);
        }
    }

    internal class InterceptableType() : InterceptableDerivedType
    {
        public static int? StaticMethod(int value)
        {
            return value + 1;
        }

        public override void MyMethod(string caller, string param, string? secondLine = null, params string[]? additionalLines)
        {
            base.MyMethod(caller, param, secondLine, additionalLines);
        }
    }

    internal class InterceptableGenericType<T1>()
    {
        public static string? StaticMethod<T2>(T1 value1, T2 value2)
        {
            return $"{value1}-{value2}";
        }

        public static T2? ReturnTestGeneric<T2>(T1 value2) where T2 : SecondNamespace.IReturnTest
        {
            string? message = value2?.ToString();
            SecondNamespace.IReturnTest test = new SecondNamespace.ReturnTest(message is null ? "N/A" : message);
            return (T2?)test;
        }


        public void NonStaticMethod<T2>(T1 value1, T2 value2)
        {
            Console.WriteLine($"{value1}-{value2}");
        }

        public void NonStaticMethod<T2, T3>(T2 value1, T3 value2)
        {
            Console.WriteLine($"{value1}-{value2}");
        }
    }
}
