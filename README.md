## AsyncEvent

[![NuGet](https://img.shields.io/nuget/v/AsyncEvent.svg)](https://www.nuget.org/packages/AsyncEvent)
[![Build status](https://ci.appveyor.com/api/projects/status/cpqt8jse4hgfabsx?svg=true)](https://ci.appveyor.com/project/TAGC/asyncevent)

AsyncEvent is a small library that defines a delegate representing asynchronous event handlers, as well as extension methods
for this delegate that can be used to fire events asynchronously.

To use this library, define an event based on `AsyncEventHandler` or `AsyncEventHandler<TEventArgs>` and call `InvokeAsync` to fire it. `InvokeAsync` returns a task that represents the work done by all registered subscribers to the event, and will only complete when all subscribers have finished handling the event. Additionally, any exceptions that occur while the event is being handled will be propagated back to the context in which the event was fired.

One thing worth pointing out is that, with the current implementation, all registered delegates are run concurrently in a non-deterministic order when an event is fired. By comparison, synchronous event handlers run delegates sequentially in a deterministic order.

### Installation

This package is available on NuGet and can be installed from there. For example, using the dotnet CLI:

```
dotnet add package AsyncEvent
```

### Usage

Define a class with asynchronous event handlers like this:

```cs
using AsyncEvent;

public class Thermometer
{
    public event AsyncEventHandler<TemperatureChangedEventArgs> TemperatureChanged;

    private Task OnTemperatureChanged(double newTemperature) =>
        TemperatureChanged?.InvokeAsync(this, new TemperatureChangedEventArgs { Temperature = newTemperature });
}

public class TemperatureChangedEventArgs : EventArgs
{
    public double Temperature { get; set; }
}
```

Then create a callback to asynchronously handle these events:

```cs
public class TemperatureRecorder
{
    private readonly ITemperatureDatabase _database;

    public TemperatureRecorder(Thermometer thermometer, ITemperatureDatabase database)
    {
        _database = database;
        thermometer.TemperatureChanged += RecordNewTemperature;
    }

    private async Task RecordNewTemperature(object sender, TemperatureChangedEventArgs eventArgs)
    {
        var temperature = eventArgs.Temperature;
        var currentTime = DateTime.Now;

        await _database.AddRecord(currentTime, temperature);
    }
}
```

Check under `examples/` for complete and runnable example projects.