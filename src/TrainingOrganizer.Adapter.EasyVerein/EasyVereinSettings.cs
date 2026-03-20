namespace TrainingOrganizer.Adapter.EasyVerein;

public sealed class EasyVereinSettings
{
    public const string SectionName = "EasyVerein";

    public required string ApiToken { get; init; }
    public string BaseUrl { get; init; } = "https://easyverein.com/api/v2.0";
}
