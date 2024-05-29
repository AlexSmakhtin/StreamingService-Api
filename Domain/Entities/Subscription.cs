namespace Domain.Entities;

public class Subscription : IEntity
{
    public Guid Id { get; init; }

    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _name = value;
        }
    }

    private decimal _cost;

    public decimal Cost
    {
        get => _cost;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _cost = value;
        }
    }

    private int _durationInDays;

    public int DurationInDays
    {
        get => _durationInDays;
        set
        {
            if (value == default)
                throw new ArgumentException("Invalid duration of subscription", nameof(value));
            _durationInDays = value;
        }
    }

    public Subscription(string name, decimal cost, int durationInDays)
    {
        Name = name;
        Cost = cost;
        DurationInDays = durationInDays;
    }
}