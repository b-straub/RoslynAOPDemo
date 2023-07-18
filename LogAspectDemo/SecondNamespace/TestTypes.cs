
namespace LogAspectDemo.SecondNamespace
{
    internal interface IReturnTest
    {
        public string Message { get; }
    }

    internal record ReturnTest(string Message) : IReturnTest;

}
