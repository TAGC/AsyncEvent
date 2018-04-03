using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncEvent;

namespace Thermometer
{
    public class Thermometer : IDisposable
    {
        public AsyncEventHandler<TemperatureChangedEventArgs> TemperatureChanged;

        private readonly CancellationTokenSource _cts;
        private readonly Random _random;

        private Task _monitorTask;

        public Thermometer()
        {
            _random = new Random();
            _cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            try
            {
                _cts.Cancel();
                _monitorTask?.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
            }

            _cts.Dispose();
            _monitorTask?.Dispose();
        }

        public void StartMonitoring()
        {
            _monitorTask = MonitorAndPublishTemperature();
        }

        private async Task MonitorAndPublishTemperature()
        {
            var currentTemperature = 20.0;
            double GenerateNewTemperature() => currentTemperature + ((_random.NextDouble() - 0.5) * 2);

            while (!_cts.Token.IsCancellationRequested)
            {
                currentTemperature = GenerateNewTemperature();
                var temperatureChanged = TemperatureChanged;

                if (temperatureChanged != null)
                {
                    Console.WriteLine($"[{DateTime.Now}] Publishing new temperature: {currentTemperature:F1}°C");
                    await temperatureChanged.InvokeAsync(this, new TemperatureChangedEventArgs(currentTemperature));
                    Console.WriteLine($"[{DateTime.Now}] Finished publishing temperature\n");
                }

                await Task.Delay(_random.Next(1000, 3000), _cts.Token);
            }
        }
    }
}