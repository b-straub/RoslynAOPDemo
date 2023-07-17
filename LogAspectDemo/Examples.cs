namespace LogAspectDemo
{
    internal class Grandparent<T1>
    {
        public class Parent<T2>
        {
            public static void Original<T3>(T1 t1, T2 t2, T3 t3) 
            {
                Console.WriteLine($"{t1}-{t2}-{t3}");
            }
        }
    }

    internal class C
    {
        public static void InterceptableMethod<T1>(T1 t) => throw new InvalidOperationException("Method failed!");
    }

    internal static class GenericExample
    {
        public static void M<T2>(T2 t)
        {
            C.InterceptableMethod(t);
        }
    }
}
