using Microsoft.EntityFrameworkCore;

public class Icecream
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}

class IcecreamDb : DbContext
{
    public IcecreamDb(DbContextOptions options) : base(options)
    { }
    public DbSet<Icecream> Icecreams { get; set; } = null!;
}