using System;
using System.Threading.Tasks;
using AsyncEvent;

namespace Thermometer
{
    public static class Program
    {
        private delegate double ConvertTemperature(double celcius);

        public static async Task Main()
        {
            using (var thermometer = new Thermometer())
            {
                var subscribers = new[]
                {
                    CreateSubscriber(0, celcius => celcius, "°C"),
                    CreateSubscriber(1, celcius => ((celcius * 9) / 5) + 32, "°F"),
                    CreateSubscriber(2, celcius => celcius + 273.15, "°K")
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
            ConvertTemperature convert,
            string units)
        {
            async Task Subscriber(object sender, TemperatureChangedEventArgs eventArgs)
            {
                await Task.Delay(500);

                var temperature = convert(eventArgs.Celcius);
                Console.WriteLine(
                    $"[{DateTime.Now}] [{subscriberId}] Responding to new temperature: {temperature:F1}{units}");
            }

            return Subscriber;
        }
    }
}