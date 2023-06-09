using System.Globalization;
using System.Net.Mime;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

public class DemoEndpoints : IEndpointsMapper
{

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/{i:int}", (int i) => $"i:{i}");

        app.MapGet("/api/v2/", APIHandle);
        string APIHandle(int a = 2, [FromQuery(Name = "b")] int bb = 2) => $"a:{a},b:{bb}";

        // GET /navigate/v1?location=43.8427,7.8527
        string handle(Location location) => $"Location: {location.Latitude}, {location.Longitude}";
        app.MapGet("/navigate/v1", handle);


        // POST /navigate/v2?lat=43.8427&lon=7.8527
        app.MapPost("/navigate/v2", (Location2 location) => $"Location: {location.Latitude}, {location.Longitude}");

        app.MapGet("/xml_obj", () => Results.Extensions.Xml(new object[] { "a", 1, 2.2, new object[] { "a", 1, 2.2 } }));
        app.MapGet("/xml_int_arr", () => Results.Extensions.Xml(new[] { 1, 2, 3 }));

        app.MapGet("/sampleresponse", () =>
        {
            return Results.Ok(new ResponseData("My Response"));
        })
        .Produces<ResponseData>(StatusCodes.Status200OK)
        .WithTags("Sample")
        .WithName("SampleResponseOperation"); // operation ids to Open API
        app.MapGet("/sampleresponseskipped", () =>
        {
            return Results.Ok(new ResponseData("My Response Skipped"));
        }).ExcludeFromDescription();
        app.MapGet("/{id}", (int id) => Results.Ok(id));
        app.MapPost("/", (ResponseData data) => Results.Ok(data))
           .Accepts<ResponseData>(MediaTypeNames.Application.Json);
    }
}

record ResponseData(string str);

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class Location
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public static bool TryParse(string? value, IFormatProvider? provider, out Location? location)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (values.Length == 2 && 
                double.TryParse(values[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var latitude) &&
                double.TryParse(values[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var longitude))
            {
                location = new Location
                {
                    Latitude = latitude,
                    Longitude = longitude
                };
                return true;
            }
        }
        location = null;
        return false;
    }
}

public class Location2
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }


    public static ValueTask<Location2?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        if (double.TryParse(context.Request.Query["lat"], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var latitude) &&
            double.TryParse(context.Request.Query["lon"], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var longitude))
        {
            var location = new Location2
            { Latitude = latitude, Longitude = longitude };
            return ValueTask.FromResult<Location2?>(location);
        }
        return ValueTask.FromResult<Location2?>(null);
    }
}
