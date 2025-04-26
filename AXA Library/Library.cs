namespace AXA_Library;

public static class Factory
{
    public static Car CreateCar(string model, int year)
    {
        return new Car(model, year);
    }

    public static House CreateHouse(int size, string city, double price)
    {
        return new House(size, city, price);
    }

    public static Driver CreateDriver(string name, int birthYear, bool noClaimBonus, int drivingLicenceYear,
        string city)
    {
        return new Driver(name, birthYear, noClaimBonus, drivingLicenceYear, city);
    }

    public static HouseOwner CreateHouseOwner(string name, int birthYear)
    {
        return new HouseOwner(name, birthYear);
    }
}

public class Car(string model, int year = 2025)
{
    public string Model { get; } = model;
    private readonly int _year = year;

    public (string, int) GetModelAndYear()
    {
        return (model, year);
    }
}

public class Driver(string name, int birthYear, bool noClaimBonus, int drivingLicenceYear, string city)
{
    private readonly string _name = name;
    public int BirthYear { get; } = birthYear;
    public bool NoClaimBonus { get; } = noClaimBonus;
    public int DrivingLicenceYear { get; } = drivingLicenceYear;
    public string City { get; } = city;
}

public class House(int size, string city, double price)
{
    public string City { get; } = city;
    public double Price { get; } = price;
}

public class HouseOwner(string name, int birthYear)
{
    private readonly string _name = name;
    private readonly int _birthYear = birthYear;
}

public enum WeatherEvent
{
    GoodWeather,
    Rain,
    Wind
}

public enum BigCity
{
    London,
    Manchester,
    Birmingham
}

public interface IRisk
{
    double MonthlyPremium();
    int DayOfMonthPremiumIsDue();
    IList<(double, double)> RiskProbabilityPerYear(); //probability of loss & amount of loss
}

public class CarCrashRisk(Driver driver, Car car) : IRisk
{
    public double MonthlyPremium()
    {
        // var car = property as Car;
        if (!driver.NoClaimBonus)
        {
            return new Random().Next(100, 130);
        }

        if (driver.DrivingLicenceYear > 2023 || driver.BirthYear > 2000
                                             || Enum.TryParse(driver.City, out BigCity _)
            // || Enum.GetNames<BigCity>().Contains(driver.City)
           )
        {
            return new Random().Next(70, 100);
        }

        return new Random().Next(40, 70);
    }

    public int DayOfMonthPremiumIsDue()
    {
        return (int)Math.Floor(new Random().NextDouble() * 28) + 1;
    }

    public IList<(double, double)> RiskProbabilityPerYear()
    {
        if (!driver.NoClaimBonus)
        {
            return
            [
                (new Random().NextDouble() * 0.1, 10000),
                (new Random().NextDouble() * 0.3, 1000),
                (new Random().NextDouble() * 0.6, 500)
            ];
        }

        if (driver.DrivingLicenceYear > 2023 || driver.BirthYear > 2000 || Enum.TryParse(driver.City, out BigCity _))
        {
            return
            [
                (new Random().NextDouble() * 0.05, 10000),
                (new Random().NextDouble() * 0.15, 1000),
                (new Random().NextDouble() * 0.3, 500)
            ];
        }

        return
        [
            (new Random().NextDouble() * 0.01, 10000),
            (new Random().NextDouble() * 0.05, 1000),
            (new Random().NextDouble() * 0.1, 500)
        ];
    }
}

public class HouseFireRisk(House house, HouseOwner houseOwner) : IRisk
{
    public double MonthlyPremium()
    {
        if (house.Price > 500000 || Enum.TryParse(house.City, out BigCity _))
        {
            return new Random().Next(50, 100);
        }

        return new Random().Next(20, 50);
    }

    public int DayOfMonthPremiumIsDue()
    {
        return (int)Math.Floor(new Random().NextDouble() * 28) + 1;
    }

    public IList<(double, double)> RiskProbabilityPerYear()
    {
        if (house.Price > 500000 || Enum.TryParse(house.City, out BigCity _))
        {
            return
            [
                (new Random().NextDouble() * 0.001, 500000),
                (new Random().NextDouble() * 0.01, 250000),
                (new Random().NextDouble() * 0.5, 125000)
            ];
        }

        return
        [
            (new Random().NextDouble() * 0.001, 200000),
            (new Random().NextDouble() * 0.01, 100000),
            (new Random().NextDouble() * 0.5, 50000)
        ];
    }
}

public class Policy<TRisk>(TRisk risk)
    where TRisk : IRisk
{
    private TRisk _risk = risk;

    public IDictionary<DateTime, double> Premiums(DateTime from, DateTime to)
    {
        // example from=25May2025 to=31March2027
        // DayOfMonth=15 
        // {15Jun2025,100}, {15jul2025,100}, etc....
        // loop while dt <= to
        // add dt and MonthlyPremium to dictionary
        Dictionary<DateTime, double> dictionary = new Dictionary<DateTime, double>();
        var dt = new DateTime(from.Year, from.Month, _risk.DayOfMonthPremiumIsDue());
        if (dt < from)
            dt = dt.AddMonths(1);
        while (dt <= to)
        {
            dictionary.Add(dt, _risk.MonthlyPremium());
            dt = dt.AddMonths(1);
        }

        return dictionary;
    }

    public double AverageClaimPerYear()
    {
        double sum = 0;
        foreach (var (probability, amount) in _risk.RiskProbabilityPerYear())
        {
            sum += probability * amount;
        }

        return sum;
    }
}

public class AxaDivision<TRisk> where TRisk : IRisk
{
    // Policy<TRisk> is not yet a real type because we don't know what TRisk is. It's a placeholder
    // but Policy<HouseFireRisk> IS a real type.
    //
    // function analogy
    // compute(int x) // x is a placeholder for an int value
    // compute(5) // 5 is a real value
    private IList<Policy<TRisk>> _policies;

    public AxaDivision(Policy<TRisk> policy)
    {
        //_policies = new List<Policy<TRisk>>(){policy};  //[{{ 1, 0.1 } , {2, 0.3}},0.2  ,  {}]
        _policies = [policy];
    }

    public AxaDivision(IList<Policy<TRisk>> list)
    {
        _policies = list;
    }

    public double IncomeForPeriod(DateTime from, DateTime to)
    {
        double sum = 0;
        foreach (var policy in _policies)
        {
            foreach (var kvp in policy.Premiums(from, to)) //key value pair
            {
                sum += kvp.Value;
            }
        }

        return sum;
    }

    public double LossesForPeriod(DateTime from, DateTime to)
    {
        double sum = 0;
        foreach (var policy in _policies)
        {
            sum += policy.AverageClaimPerYear() * _adjustLossProbability;
        }

        var years = (to - from).Days / 365.0;
        return sum * years ;
    }

    private double _adjustLossProbability = 1.0;

    public void AdjustLossesForecast(WeatherEvent e, string city)
    {
        switch (e)
        {
            case WeatherEvent.GoodWeather:
                _adjustLossProbability = 0.9;
                break;
            case WeatherEvent.Rain:
                _adjustLossProbability = 2.0;
                break;
            case WeatherEvent.Wind:
                _adjustLossProbability = 1.5;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(e), e, null);
        }
    }
}

public class NonGenericClass
{
    public T GenericMethod<T>(T arg)
    {
        return arg;
    }
}

public class GenericClass<T>
{
    private T _field;

    public T GenericMethod(T arg)
    {
        return arg;
    }

    public TU GenericMethod2<TU>(TU arg)
    {
        return arg;
    }
}