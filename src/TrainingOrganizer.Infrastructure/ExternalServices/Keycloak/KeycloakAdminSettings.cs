namespace TrainingOrganizer.Infrastructure.ExternalServices.Keycloak;

public sealed class KeycloakAdminSettings
{
    public const string SectionName = "KeycloakAdmin";

    public required string BaseUrl { get; init; }
    public required string Realm { get; init; }
    public required string AdminUsername { get; init; }
    public required string AdminPassword { get; init; }
}
