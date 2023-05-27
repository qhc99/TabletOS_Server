using FluentValidation;

public class PersonValidator : AbstractValidator<Person>
{
    public PersonValidator()
    {
        RuleFor(p => p.FirstName).NotEmpty().MaximumLength(30);
        RuleFor(p => p.LastName).NotEmpty().MaximumLength(30);
        RuleFor(p => p.Email).EmailAddress().Length(6,100);
    }
}