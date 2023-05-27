using FluentValidation;

public class ValidationEndpoints : IEndpointsMapper
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/people", async (Person person, IValidator<Person> validator) =>
        {
            var validationResult =
            await validator.ValidateAsync(person);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.ToDictionary();
                return Results.ValidationProblem(errors);
            }
            else
            {
                return Results.Ok();
            }
        });
    }
}