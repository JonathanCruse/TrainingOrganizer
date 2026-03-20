using MediatR;
using TrainingOrganizer.Api.Extensions;
using TrainingOrganizer.Membership.Application.Commands;

namespace TrainingOrganizer.Api.Endpoints;

public static class ImportEndpoints
{
    public static void MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/import").WithTags("Import").RequireAuthorization("Admin");

        group.MapPost("/easyverein", ImportFromEasyVerein);
    }

    private static async Task<IResult> ImportFromEasyVerein(
        ImportFromEasyVereinRequest request, ISender sender)
    {
        var command = new ImportMembersFromEasyVereinCommand(request.AdminGroupName);
        var result = await sender.Send(command);
        return result.ToApiResult();
    }
}

public sealed record ImportFromEasyVereinRequest(string AdminGroupName);
