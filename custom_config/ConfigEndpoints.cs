using Microsoft.Extensions.Options;

public class ConfigEndpoints : IEndpointsMapper
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        

        // config validation endpoint
        app.MapGet("/read/options", (IOptions<ConfigWithValidation> optionsValidation) =>
        {
            return Results.Ok(new
            {
                Validation = optionsValidation.Value
            });
        })
        .WithName("ReadOptions");
    }
}