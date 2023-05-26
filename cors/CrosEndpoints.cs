using Microsoft.AspNetCore.Cors;

public class CrosEndpoints : IEndpointsMapper
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/cors", () =>
        {
            return Results.Ok(new { CorsResultJson = true });
        });

        // With Extension
        app.MapGet("/api/cors/extension", () =>
        {
            return Results.Ok(new { CorsResultJson = true });
        })
        .RequireCors("MyCustomPolicy");

        // With Annotation
        app.MapGet("/api/cors/annotation", [EnableCors("MyCustomPolicy")] () =>
        {
            return Results.Ok(new { CorsResultJson = true });
        });
    }
}