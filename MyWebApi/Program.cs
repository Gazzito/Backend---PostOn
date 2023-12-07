using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using servicesToUse;
var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL connection
var connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=omfgnoob24413;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));


// Add services to the container.
// ...
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
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
        // Check if the user with the same email already exists
        var existingEmail = await dbContext.Users
            .Include(u => u.Login)
            .FirstOrDefaultAsync(u => u.Login.Username == user.Login.Username);

        if (existingEmail != null)
        {
            // User with the same email already exists
            return Results.BadRequest("User with the same email already exists.");
        }

         var existingUsername = await dbContext.Users
            .Include(u => u)
            .FirstOrDefaultAsync(u => u.Email == user.Email);

        if (existingUsername != null)
        {
            // User with the same email already exists
            return Results.BadRequest("User with the same username already exists.");
        }
         
        // Generate a random salt
        string salt = PasswordManager.Salt();

        // Combine the password and salt, then hash the result
        string hashedPassword = PasswordManager.HashPassword(user.Login.PasswordHash, salt);

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

app.MapPost("/login", async (User user, ApplicationDbContext dbContext) =>
{
    // Assuming a new Login is provided along with the User data
    if (user.Login != null)
    {
        // Ensure UserId is 0 to allow for identity column generation
         
        // Generate a random salt
        string salt = PasswordManager.Salt();

        // Combine the password and salt, then hash the result
        string hashedPassword = PasswordManager.HashPassword(user.Login.PasswordHash, salt);

        // Store both the hashed password and the salt in the database
        user.Login.PasswordHash = hashedPassword;
        user.Login.Salt = salt;

        // Add the User and associated Login
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return Results.Created($"/users/{user.UserId}", user);
    }

    return Results.BadRequest("Login information is required.");
}).WithName("login");


app.MapGet("/logins", async (ApplicationDbContext dbContext) =>
{
    var logins = await dbContext.Logins
        .Include(l => l.User) // Include the related User entity
        .ToListAsync();

    return logins;
})
.WithName("GetLogins");

app.Run();
