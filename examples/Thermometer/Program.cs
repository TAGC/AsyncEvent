using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncEvent;

namespace Thermometer
{
    public static class Program
    {
        public static async Task Main()
        {
            using (var thermometer = new Thermometer())
            {
                var subscribers = new[]
                {
                    CreateSubscriber(0, celcius => celcius, "°C"),
                    CreateSubscriber(1, celcius => (celcius * 9 / 5) + 32, "°F"),
                    CreateSubscriber(2, celcius => celcius + 273.15, "°K"),
                };

                foreach (var subscriber in subscribers)
                {
                    thermometer.TemperatureChanged += subscriber;
                }

                Console.WriteLine("Press any key to stop the program.");
                await Task.Delay(2000);

                thermometer.StartMonitoring();
                Console.ReadKey();
            }
        }

        private static AsyncEventHandler<TemperatureChangedEventArgs> CreateSubscriber(
            int subscriberId,
            Func<double, double> convertTemperature,
            string units)
        {
            async Task Subscriber(object sender, TemperatureChangedEventArgs eventArgs)
            {
                await Task.Delay(500);

                var temperature = convertTemperature(eventArgs.Celcius);
                Console.WriteLine($"[{DateTime.Now}] [{subscriberId}] Responding to new temperature: {temperature}{units}");
            }

            return Subscriber;
        }
    }

    public class Thermometer : IDisposable
    {
        private readonly Random _random;
        private readonly CancellationTokenSource _cts;

        private Task _monitorTask;

        public Thermometer()
        {
            _random = new Random();
            _cts = new CancellationTokenSource();
        }

        public AsyncEventHandler<TemperatureChangedEventArgs> TemperatureChanged;

        public void StartMonitoring()
        {
            _monitorTask = MonitorAndPublishTemperature();
        }

        private async Task MonitorAndPublishTemperature()
        {
            var currentTemperature = 20.0;
            double GenerateNewTemperature() => currentTemperature + (_random.NextDouble() - 0.5) * 2;

            while (!_cts.Token.IsCancellationRequested)
            {
                currentTemperature = GenerateNewTemperature();
                var temperatureChanged = TemperatureChanged;

                if (temperatureChanged != null)
                {
                    Console.WriteLine($"[{DateTime.Now}] Publishing new temperature: {currentTemperature}°C");
                    await temperatureChanged.InvokeAsync(this, new TemperatureChangedEventArgs(currentTemperature));
                    Console.WriteLine($"[{DateTime.Now}] Finished publishing temperature\n");
                }

                await Task.Delay(_random.Next(500, 1000), _cts.Token);
            }
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
    }

    public class TemperatureChangedEventArgs
    {
        public TemperatureChangedEventArgs(double celcius)
        {
            Celcius = celcius;
        }

        public double Celcius { get; }
    }
}