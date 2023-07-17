using LogAspectTest;

var it = new InterceptableType();
it.MyMethod(nameof(InterceptableType), "World", additionalLines: new[] {"Line1", "Line2"});

var idt = new InterceptableDerivedType();
idt.MyMethod(nameof(InterceptableDerivedType), "World", "SecondLine");

var ibt = new InterceptableBaseType();
ibt.MyMethod(nameof(InterceptableBaseType), "World");

var value = InterceptableType.StaticMethod(5);
Console.WriteLine(value);
