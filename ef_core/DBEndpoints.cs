using Microsoft.EntityFrameworkCore;

public class DBEndpoints : IEndpointsMapper
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/icecreams",
            async (IcecreamDb db, Icecream icecream) =>
            {
                await db.Icecreams.AddAsync(icecream);
                await db.SaveChangesAsync();
                return Results.Created($"/icecreams/{icecream.Id}", icecream);
            });
        app.MapGet(
            "/icecreams", 
            async (IcecreamDb db) => await db.Icecreams.ToListAsync());
    }
}