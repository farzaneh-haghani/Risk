namespace AXA;

public static class Program
{
    public static void Main()
    {
        var manyPolicies = Enumerable.Range(0, 10000)
            .Select((i) => new Policy<CarCrashRisk>(new CarCrashRisk()))
            .ToList(); //search IList instead List
        // var myCar = new CarCrashRisk();
        // var myPolicy = new Policy<CarCrashRisk>(myCar);
        var axa = new AxaDivision<CarCrashRisk>(manyPolicies);
        var to = DateTime.Now;
        var from = new DateTime(2024, 06, 14);
        // foreach (var policy in manyPolicies)
        // {
        //     foreach (var kvp in policy.Premiums(from, to))
        //     {
        //         Console.WriteLine($"Date:{kvp.Key} Amount:{kvp.Value}");
        //     }
        // }

        var forecaster = new WeatherForecastCompany();
        forecaster.OnWeatherEvent = axa.AdjustLossesForecast;

        forecaster.ForecastWeather();
        Console.WriteLine($"{axa.IncomeForPeriod(from, to)},{axa.LossesForPeriod(from, to)}");

        forecaster.ForecastWeather();
        Console.WriteLine($"{axa.IncomeForPeriod(from, to)},{axa.LossesForPeriod(from, to)}");

        forecaster.ForecastWeather();
        Console.WriteLine($"{axa.IncomeForPeriod(from, to)},{axa.LossesForPeriod(from, to)}");

        // axa.AdjustLossesForecast(WeatherEvent.Rain);
        // Console.WriteLine($"{axa.IncomeForPeriod(from, to)},{axa.LossesForPeriod(from, to)}");
        // axa.AdjustLossesForecast(WeatherEvent.GoodWeather);
        // Console.WriteLine($"{axa.IncomeForPeriod(from, to)},{axa.LossesForPeriod(from, to)}");
        // axa.AdjustLossesForecast(WeatherEvent.Wind);
        // Console.WriteLine($"{axa.IncomeForPeriod(from, to)},{axa.LossesForPeriod(from, to)}");

        var d = new NonGenericClass();
        var t = d.GenericMethod<string>("string");
        var u = d.GenericMethod<int>(2);
        var v = d.GenericMethod<AxaDivision<CarCrashRisk>>(axa);
    }

    public class WeatherForecastCompany
    {
        // WeatherEventNotifier is a type that holds a method
        // (like const fn = (y,z) => {...return x;})
        // but WeatherEventNotifier accepts only function that return void,
        // and receives WeatherEvent.
        public delegate void WeatherEventNotifier(WeatherEvent weatherEvent);
        public WeatherEventNotifier? OnWeatherEvent;
        
        // Homework, add a City to WeatherEvent

        // alternative in .NET (Action does not return a value)
        public Action<WeatherEvent> OnWeatherEvent2;
        // Func returns a value
        public Func<WeatherEvent, int> OnWeatherEvent3;

        public void ForecastWeather()
        {
            //OnWeatherEvent3 = OnWeatherEvent; // error
            var r = new Random().Next(3);
            switch (r)
            {
                case 0:
                {
                    OnWeatherEvent?.Invoke(WeatherEvent.Rain);
                    //if (OnWeatherEvent!=null) OnWeatherEvent(WeatherEvent.GoodWeather);
                    break;
                }
                case 1:
                {
                    OnWeatherEvent?.Invoke(WeatherEvent.GoodWeather);
                    break;
                }
                case 2:
                {
                    OnWeatherEvent?.Invoke(WeatherEvent.Wind);
                    break;
                }
            }
        }
    }
}