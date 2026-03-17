namespace TrainingOrganizer.Application.Membership.Services;

public interface IKeycloakAdminClient
{
    /// <summary>
    /// Creates a Keycloak user or returns the existing user's ID if the email is already registered.
    /// The user is created with email verified and a required UPDATE_PASSWORD action.
    /// </summary>
    Task<string> CreateOrGetUserAsync(string email, string firstName, string lastName, CancellationToken ct = default);

    /// <summary>
    /// Assigns a realm-level role to a Keycloak user.
    /// </summary>
    Task AssignRealmRoleAsync(string userId, string roleName, CancellationToken ct = default);
}
