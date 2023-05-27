using AutoMapper;

public class PersonEntity
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime BirthDate { get; set; }

    public AddressEntity? Address { get; set; }
}

public class AddressEntity
{
    public string? Street { get; set; }

    public string? City { get; set; }
}

public class PersonDto
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime BirthDate { get; set; }

    public int Age { get; set; }
    public string? City { get; set; }
}

public class PersonProfile : Profile
{
    public PersonProfile()
    {
        CreateMap<PersonEntity, PersonDto>().
            ForMember(
                dst => dst.Age,
                opt => opt.MapFrom(src => CalculateAge(src.BirthDate))).
            ForMember(
                dst => dst.City,
                opt => opt.MapFrom(src => src.Address!.City));
    }
    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (today.DayOfYear < dateOfBirth.DayOfYear)
        {
            age--;
        }
        return age;
    }
}

public class DTOEndpoints : IEndpointsMapper
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/people/{id:int}",
            (int id, IMapper mapper) =>
                {
                    var personEntity = new PersonEntity();
                    var personDto = mapper.Map<PersonDto>(personEntity);
                    return Results.Ok(personDto);
                }).
            Produces(StatusCodes.Status200OK, typeof(PersonDto));
    }
}