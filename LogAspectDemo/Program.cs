using LogAspectDemo;

try
{
    var it = new InterceptableType();
    it.MyMethod(nameof(InterceptableType), "World", additionalLines: new[] { "Line1", "Line2" });

    var idt = new InterceptableDerivedType();
    idt.MyMethod(nameof(InterceptableDerivedType), "World", "SecondLine");

    var ibt = new InterceptableBaseType();
    ibt.MyMethod(nameof(InterceptableBaseType), "World");

    var value = InterceptableType.StaticMethod(5);
    Console.WriteLine(value);

    var igt = new InterceptableGenericType<int>();
    igt.NonStaticMethod<string>(5, "Test");
    igt.NonStaticMethod<int, string>(5, "Test");

    Console.WriteLine(InterceptableGenericType<double>.StaticMethod(5, "Test"));
    Console.WriteLine(InterceptableGenericType<int>.ReturnTestGeneric<LogAspectDemo.SecondNamespace.ReturnTest>(5));

    Grandparent<int>.Parent<bool>.Original(1, false, "a");

    GenericExample.M(5);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

