using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL connection
var connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=omfgnoob24413;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add services to the container.
// ...
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var app = builder.Build();


app.MapGet("/users", async (ApplicationDbContext dbContext) =>
{
    var users = await dbContext.Users
        .Include(l => l.Login) // Include the related User entity
        .ToListAsync();

    return users;
})
.WithName("GetUsers");

app.MapPost("/register", async (User user, ApplicationDbContext dbContext) =>
{
    // Assuming a new Login is provided along with the User data
    if (user.Login != null)
    {
        // Ensure UserId is 0 to allow for identity column generation
       
        // Assuming user is an instance of the User class

        // Generate a random salt
        string salt = BCrypt.Net.BCrypt.GenerateSalt();

        // Combine the password and salt, then hash the result
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Login.PasswordHash, salt);

        // Store both the hashed password and the salt in the database
        user.Login.PasswordHash = hashedPassword;
        user.Login.Salt = salt;

        // Add the User and associated Login
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return Results.Created($"/users/{user.UserId}", user);
    }

    return Results.BadRequest("Login information is required.");
}).WithName("RegisterUsers");

app.MapGet("/logins", async (ApplicationDbContext dbContext) =>
{
    var logins = await dbContext.Logins
        .Include(l => l.User) // Include the related User entity
        .ToListAsync();

    return logins;
})
.WithName("GetLogins");

app.Run();
