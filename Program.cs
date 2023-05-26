using System.Reflection;
using System.Text.Json.Serialization;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = builder.Environment.ApplicationName,
        Version = "v1",
        Contact = new()
        {
            Name = "PacktAuthor",
            Email = "authors@packtpub.com",
            Url = new Uri("https://www.packtpub.com/")
        },
        Description = "PacktPub Minimal API - Swagger",
        License = new Microsoft.OpenApi.Models.OpenApiLicense(),
        TermsOfService = new("https://www.packtpub.com/")
    });
    c.OperationFilter<CorrelationIdOperationFilter>();
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    // options.SerializerOptions.IgnoreReadOnlyProperties = true;
});

var corsPolicy = new CorsPolicyBuilder("http://127.0.0.1")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .Build();

// Custom Policy
builder.Services.AddCors(options => options.AddPolicy("MyCustomPolicy", corsPolicy));

// config file
var startupConfig = builder.Configuration.
    GetSection(nameof(MyCustomStartupObject)).
    Get<MyCustomStartupObject>();
// config file validation
builder.Services.AddOptions<ConfigWithValidation>().
    Bind(builder.Configuration.GetSection(nameof(ConfigWithValidation))).
    ValidateDataAnnotations();

// error handling package support
builder.Services.TryAddSingleton<IActionResultExecutor<ObjectResult>, ProblemDetailsResultExecutor>();

// error handling map
builder.Services.AddProblemDetails(options =>
{
    options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
});

// middleware
var app = builder.Build();

// use error handling middleware
app.UseProblemDetails();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{builder.Environment.ApplicationName} v1"));
    // check cors
    app.Use((context, next) =>
    {
        var origin = context.Request.Headers.Origin.ToString();
        app.Logger.LogInformation($"new request origin: {origin}, pass: {corsPolicy.IsOriginAllowed(origin)}");
        return next(context);
    });
}
// cors
app.UseCors("MyCustomPolicy");

// config info endpoint
app.MapGet("/read/configurations", (IConfiguration configuration) =>
        {
            var customObject = configuration.GetSection
        (nameof(MyCustomObject)).Get<MyCustomObject>();
            return Results.Ok(new
            {
                MyCustomValue = configuration.GetValue<string>("MyCustomValue"),
                ConnectionString = configuration.GetConnectionString("Default"),
                CustomObject = customObject,
                StartupObject = startupConfig
            });
        })
        .WithName("ReadConfigurations");

// config validation endpoint
app.MapGet("/read/options", (IOptions<ConfigWithValidation> optionsValidation) =>
{
    return Results.Ok(new
    {
        Validation = optionsValidation.Value
    });
})
.WithName("ReadOptions");

app.UseHttpsRedirection();
app.MapRefelectedEndpoints(Assembly.GetExecutingAssembly());

// test error handling package
app.MapGet("/not-implemented-exception", () =>
{
    throw new NotImplementedException
 ("This is an exception thrown from a Minimal API.");
}).
    Produces<ProblemDetails>(StatusCodes.Status501NotImplemented).
    WithName("NotImplementedExceptions");



app.Run();

