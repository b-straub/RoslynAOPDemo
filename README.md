# RoslynAOPDemo
A demonstration project for the new interceptors feature

[Interceptors](https://github.com/dotnet/roslyn/blob/main/docs/features/interceptors.md)

## Prerequisites

- Visual Studio >= Version 17.7.0 Preview 3.0 (other IDEs might work as well)
- .NET SDK >= 8.0.100-preview.6.23330.14

### Demo

The demonstration project has currently one source generator (LogAspectSG) adding a simple logging aspect to every method listed with full qualified name inside 'LogEntries.txt'.

Check the sample 'LogAspectDemo' for a simple demonstration of the interception.

The source generator is for demonstration only as well, no error handling is provided and there will be cases not handled yet.