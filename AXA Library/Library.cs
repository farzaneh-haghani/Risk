using Microsoft.VisualBasic;

namespace AXA;
    
public class AxaDivision<TRisk> where TRisk: IRisk
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
            foreach (var kvp in policy.Premiums(from, to))  //key value pair
            {
                sum += kvp.Value;
            }
        }

        return sum;
    }

    public double LossesForPeriod(DateTime from, DateTime to)
    {
        double sum = 0;
        foreach(var policy in _policies)
        {
            sum += policy.AverageClaimPerYear();
        }

        var years= (to - from).Days/365.0;
        return sum*years*_adjustLossProbability;
    }

    private double _adjustLossProbability = 1.0;
    
    public void AdjustLossesForecast(WeatherEvent e)
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

public class NonGenericClass {
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

public enum WeatherEvent
{
    GoodWeather,
    Rain,
    Wind
}

public class Policy<TRisk> where TRisk:IRisk
{
    private TRisk _risk;
    
    public Policy(TRisk risk)
    {
        _risk = risk;
    }
    public IDictionary<DateTime, double> Premiums(DateTime from, DateTime to)
    {
        // example from=25May2025 to=31March2027
        // DayOfMonth=15 
        // {15Jun2025,100}, {15jul2025,100}, etc....
        // loop while dt <= to
        // add dt and MonthlyPremium to dictionary
        Dictionary<DateTime,double> dictionary= new Dictionary<DateTime, double>();
        var dt = new DateTime(from.Year, from.Month, _risk.DayOfMonthPremiumIsDue());
        if (dt<from) 
            dt = dt.AddMonths(1);
        while (dt <= to)
        {
            dictionary.Add(dt,_risk.MonthlyPremium());
            dt = dt.AddMonths(1);
        }
        return dictionary;
    }

    public double AverageClaimPerYear()
    {
        double sum = 0;
        foreach (var (probability,amount) in _risk.RiskProbabilityPerYear())
        {
            sum += probability*amount;
        }
        return sum;
    }
}

public interface IRisk
{
    double MonthlyPremium();
    int DayOfMonthPremiumIsDue();
    IList<(double,double)> RiskProbabilityPerYear(); //probability of loss & amount of loss
}


public class CarCrashRisk:IRisk
{
    private readonly double _premium;
    private readonly int _dayOfMonth;
    private readonly (double,double)[] _loss;
    // private Car car; // car can contain model, age,
    // private Driver driver; // driver has Age, Experience, Risk (Hi/lo) 
    // private Address address; // some cities are more expensive
    

    public CarCrashRisk()
    {
        _premium = new Random().NextDouble()*100;
        _dayOfMonth = (int)Math.Floor(new Random().NextDouble()*28) + 1;
        _loss = [
            (new Random().NextDouble()*0.01,10000),
            (new Random().NextDouble()*0.05,1000),
            (new Random().NextDouble()*0.1,500)
        ];
        // replace _loss calculation with something that is determined by the properties of the car/driver/address
    }
    public double MonthlyPremium()
    {
        return _premium;
    }

    public int DayOfMonthPremiumIsDue()
    {
        return _dayOfMonth;
    }

    public IList<(double,double)>RiskProbabilityPerYear()
    {
        
        return _loss;   
    }
}

public class HouseFireRisk:IRisk
{
    private readonly double _premium;
    private readonly int _dayOfMonth;
    private readonly (double,double)[] _loss;

    public HouseFireRisk()
    {
        _premium = new Random().NextDouble()*100;
        _dayOfMonth = (int)Math.Floor(new Random().NextDouble()*28) + 1;
        _loss = [
            (new Random().NextDouble()*0.001,100000),
            (new Random().NextDouble()*0.01,10000),
            (new Random().NextDouble()*0.5,5000)
        ];
    }
    public double MonthlyPremium()
    {
        return _premium;
    }

    public int DayOfMonthPremiumIsDue()
    {
        return _dayOfMonth;
    }
    public IList<(double, double)> RiskProbabilityPerYear()
    {
        return _loss;
    }
}
