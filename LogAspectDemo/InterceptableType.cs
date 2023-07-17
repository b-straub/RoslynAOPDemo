namespace LogAspectTest
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
                foreach(var additionalLine in additionalLines)
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
        public string? TestProperty { get; set; }

        public static int StaticMethod(int value)
        {
            return value + 1;
        }

        public override void MyMethod(string caller, string param, string? secondLine = null, params string[]? additionalLines)
        {
            base.MyMethod(caller, param, secondLine, additionalLines);
        }
    }
}
