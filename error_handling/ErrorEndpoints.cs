using Microsoft.AspNetCore.Mvc;

public class ErrorEndpoitns : IEndpointsMapper
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        // test error handling package
        app.MapGet("/not-implemented-exception", () =>
        {
            throw new NotImplementedException
         ("This is an exception thrown from a Minimal API.");
        }).
            Produces<ProblemDetails>(StatusCodes.Status501NotImplemented).
            WithName("NotImplementedExceptions");
    }
}