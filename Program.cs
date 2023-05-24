using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Cors.Infrastructure;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{builder.Environment.ApplicationName} v1"));
}
app.Use((context, next) =>
{
    var origin = context.Request.Headers.Origin.ToString();
    app.Logger.LogInformation($"new request origin: {origin}, pass: {corsPolicy.IsOriginAllowed(origin)}");
    return next(context);
});
app.UseCors("MyCustomPolicy");


app.UseHttpsRedirection();
app.MapRefelectedEndpoints(Assembly.GetExecutingAssembly());
app.Run();

