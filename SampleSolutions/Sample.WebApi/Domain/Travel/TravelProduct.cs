namespace Sample.WebApi.Domain.Travel;

public sealed record TravelProduct(
    string Id,
    string Name,
    string Destination,
    decimal Price,
    int Nights);
