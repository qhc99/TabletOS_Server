public class GeneratorEndpoints : IEndpointsMapper
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        // logger generator endpoint
        app.MapPost("/start-log", (PostData data, LogGenerator
        logGenerator) =>
        {
            logGenerator.StartEndpointSignal("start-log", data);
            logGenerator.LogLevelFilteredAtRuntime(LogLevel.Trace,
            "start-log", data);
        })
        .WithName("StartLog");

        // w3c logging endpoint
        app.MapGet(
            "/first-w3c-log",
            (IWebHostEnvironment webHostEnvironment) =>
            {
                return Results.Ok(new { PathToWrite = webHostEnvironment.ContentRootPath });
            }).
                WithName("GetW3CLog");

        // console logging
        app.MapGet("/first-log", (ILogger<CategoryFiltered> loggerCategory, ILogger<MyCategoryAlert> loggerAlertCategory)
        =>
        {
            loggerCategory.LogInformation("I'm information {MyName}", "My Name Information");
            loggerCategory.LogDebug("I'm debug {MyName}", "My Name Debug");
            loggerCategory.LogInformation("I'm debug {Data}", new PayloadData("CategoryRoot", "Debug"));
            loggerAlertCategory.LogInformation("I'm information {MyName}", "Alert Information");
            loggerAlertCategory.LogDebug("I'm debug {MyName}", "Alert Debug");
            var p = new PayloadData("AlertCategory", "Debug");
            loggerAlertCategory.LogDebug("I'm debug {Data}", p);
            return Results.Ok();
        })
        .WithName("GetFirstLog");

        app.MapGet("/provider-log", (ILogger<CategoryFiltered> loggerCategory) =>
        {
            loggerCategory.LogInformation("I'm information");
            loggerCategory.LogDebug("I'm debug");
            loggerCategory.LogWarning("I'm warning");
            loggerCategory.LogError("I'm error");
            loggerCategory.LogError(new Exception("My brutal error"), "I'm error");
            return Results.Ok();
        })
        .WithName("GetProviderLog");
    }
}