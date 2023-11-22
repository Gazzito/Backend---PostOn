using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL connection
var connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=omfgnoob24413;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add services to the container.
// ...

var app = builder.Build();


app.UseHttpsRedirection();

app.MapGet("/users", async (ApplicationDbContext dbContext) =>
{
    var users = await dbContext.Users
        .ToListAsync();

    return users;
})
.WithName("GetUsers")
.WithOpenApi();

app.Run();
