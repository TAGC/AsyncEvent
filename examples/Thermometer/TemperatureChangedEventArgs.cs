namespace Thermometer
{
    public class TemperatureChangedEventArgs
    {
        public TemperatureChangedEventArgs(double celcius)
        {
            Celcius = celcius;
        }

        public double Celcius { get; }
    }
}