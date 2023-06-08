# Instances
A .NET Standard `Process` wrapper with an elegant API, for both asyncronous and syncronous use, providing both Events and support for Tasks with cancellation support
 
![.NET Core](https://github.com/rosenbjerg/Instances/workflows/CI/badge.svg)
[![codecov.io](https://codecov.io/github/rosenbjerg/agentdeploy/coverage.svg?branch=main)](https://app.codecov.io/gh/rosenbjerg/Instances)
[![GitHub](https://img.shields.io/github/license/rosenbjerg/Instances)](https://github.com/rosenbjerg/Instances/blob/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/v/instances)](https://www.nuget.org/packages/instances/)
[![Nuget](https://img.shields.io/nuget/dt/instances)](https://www.nuget.org/packages/instances/)
![Dependent repos (via libraries.io)](https://img.shields.io/librariesio/dependent-repos/nuget/instances)


# Usage
There are three ways to use this library, requiring at least 1, 2, or 3 lines of code to use.

### Shortest form, supporting only few options
```c#
var result = await Instance.FinishAsync("dotnet", "build -c Release", cancellationToken);
Console.WriteLine(result.ExitCode);
// or
var result = Instance.Finish("dotnet", "build -c Release");
```

### Short form, supporting more options
```c#
using var instance = Instance.Start("dotnet", "build -c Release");
var result = await instance.WaitForExitAsync(cancellationToken);
// or
using var instance = Instance.Start("dotnet", "build -c Release");
var result = instance.WaitForExit();
```

### Full form, supporting all options
```c#
var processArgument = new ProcessArguments("dotnet", "build -c Release");
processArgument.Exited += (_, exitResult) => Console.WriteLine(exitResult.ExitCode);
processArgument.OutputDataReceived += (_, data) => Console.WriteLine(data);
processArgument.ErrorDataReceived += (_, data) => Console.WriteLine(data);

using var instance = processArgument.Start();

var result = await instance.WaitForExitAsync(cancellationToken);
// or 
var result = instance.WaitForExit();
```


## Features
```c#
using var instance = Instance.Start("dotnet", "build -c Release");

// send input to process' standard input
instance.SendInput("Hello World");

// stop the process
instance.Kill();

// access process output
foreach (var line in instance.OutputData)
    Console.WriteLine(line);
// and error data easily while the process is running
foreach (var line in instance.ErrorData)
    Console.WriteLine(line);

// or wait for the process to exit (with support for cancellation token)
var result = await instance.WaitForExitAsync(cancellationToken);
Console.WriteLine(result.ExitCode);
Console.WriteLine(result.OutputData.Count);
```