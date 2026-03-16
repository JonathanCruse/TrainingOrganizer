using TrainingOrganizer.Application.Common.Models;

namespace TrainingOrganizer.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToApiResult(this Result result)
    {
        return result.IsSuccess
            ? Results.Ok()
            : Results.Problem(statusCode: 422, title: result.Error.Code, detail: result.Error.Message);
    }

    public static IResult ToApiResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(statusCode: 422, title: result.Error.Code, detail: result.Error.Message);
    }

    public static IResult ToCreatedResult<T>(this Result<T> result, string uri)
    {
        return result.IsSuccess
            ? Results.Created(uri, result.Value)
            : Results.Problem(statusCode: 422, title: result.Error.Code, detail: result.Error.Message);
    }
}
