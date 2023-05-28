using Microsoft.EntityFrameworkCore;

public class DBEndpoints : IEndpointsMapper
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var postIcecream = async (IcecreamDb db, Icecream icecream) =>
            {
                await db.Icecreams.AddAsync(icecream);
                await db.SaveChangesAsync();
                return Results.Created($"/icecreams/{icecream.Id}", icecream);
            };
        var getIcecream = async (IcecreamDb db) => await db.Icecreams.ToListAsync();
        var updateIcecream = async (IcecreamDb db, Icecream updateicecream, int id) =>
            {
                var icecream = await db.Icecreams.FindAsync(id);
                if (icecream is null)
                {
                    return Results.NotFound();
                }
                icecream.Name = updateicecream.Name;
                icecream.Description = updateicecream.Description;
                await db.SaveChangesAsync();
                return Results.Ok();
            };
        var deleteIcecream = async (IcecreamDb db, int id) =>
            {
                var icecream = await db.Icecreams.FindAsync(id);
                if (icecream is null)
                {
                    return Results.NotFound();
                }
                db.Icecreams.Remove(icecream);
                await db.SaveChangesAsync();
                return Results.Ok();
            };

        app.MapPost("/icecreams", postIcecream);
        app.MapGet("/icecreams", getIcecream);
        app.MapPut("/icecreams/{id}", updateIcecream);
        app.MapDelete("/icecreams/{id}", deleteIcecream);
    }
}