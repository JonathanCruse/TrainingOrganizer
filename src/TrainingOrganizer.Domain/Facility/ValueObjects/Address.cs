using TrainingOrganizer.Domain.Common;

namespace TrainingOrganizer.Domain.Facility.ValueObjects;

public sealed record Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public Address(string street, string city, string postalCode, string country)
    {
        Street = Guard.AgainstNullOrWhiteSpace(street, nameof(street));
        City = Guard.AgainstNullOrWhiteSpace(city, nameof(city));
        PostalCode = Guard.AgainstNullOrWhiteSpace(postalCode, nameof(postalCode));
        Country = Guard.AgainstNullOrWhiteSpace(country, nameof(country));
    }

    public override string ToString() => $"{Street}, {PostalCode} {City}, {Country}";
}
