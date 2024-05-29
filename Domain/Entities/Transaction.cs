namespace Domain.Entities;

public class Transaction : IEntity
{
    public Guid Id { get; init; }

    private DateTime _purchaseDate;

    public DateTime PurchaseDate
    {
        get => _purchaseDate;
        set
        {
            if (value == default)
                throw new ArgumentException("Invalid purchase date", nameof(value));
            _purchaseDate = value;
        }
    }

    public DateTime ExpirationDate => _purchaseDate.AddDays(Subscription.DurationInDays);

    public User User { get; set; } = null!;
    public Subscription Subscription { get; set; } = null!;

    public Transaction(DateTime purchaseDate)
    {
        PurchaseDate = purchaseDate;
    }
}