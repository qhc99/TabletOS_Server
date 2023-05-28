using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

var corsPolicy = new CorsPolicyBuilder("http://127.0.0.1:5500")
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

// logging
// Infrastructure logging
// builder.Services.AddW3CLogging(logging =>
// {
//     logging.LoggingFields = W3CLoggingFields.All;
// });
// source generator logging
builder.Services.AddScoped<LogGenerator>();

// json console logging
builder.Logging.AddJsonConsole(options =>
 options.JsonWriterOptions = new JsonWriterOptions()
 {
     Indented = true
 });

// validation register
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationRulesToSwagger();

// DTO
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// EF Core
builder.Services.AddDbContext<IcecreamDb>(options => options.UseInMemoryDatabase("icecreams"));

// auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization();


//=================================================
// middleware
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{builder.Environment.ApplicationName} v1"));
    // check cors
    app.Use((context, next) =>
    {
        var origin = context.Request.Headers.Origin.ToString();
        app.Logger.LogDebug("new request origin: {Origin}, pass: {Pass}", origin, corsPolicy.IsOriginAllowed(origin));
        return next(context);
    });
}

// use error handling middleware
app.UseProblemDetails();

// cors
app.UseCors("MyCustomPolicy");

// logging middleware
// app.UseW3CLogging();
app.UseHttpsRedirection();

// auth
app.UseAuthentication();
app.UseAuthorization();

// add endpoints by reflection
app.MapRefelectedEndpoints();

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

app.Run();



