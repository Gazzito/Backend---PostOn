using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using servicesToUse;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

// Add authentication services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "Backend",
            ValidAudience = "PostOnFront",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("areallyhardpasswordtocrack"))
        };
    });

// Configure PostgreSQL connection
var connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=omfgnoob24413;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container.
// ...
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod();
                      });
});
var app = builder.Build();

app.MapHub<FriendsHub>("/friendsHub");

app.UseCors(MyAllowSpecificOrigins);
// Use authentication middleware
app.UseAuthentication();

app.UseAuthorization();

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
    Console.WriteLine(user);
    // Assuming a new Login is provided along with the User data
    if (user.Login != null)
    {
        // Check if the user with the same email already exists
        var existingUsername = await dbContext.Users
            .Include(u => u.Login)
            .FirstOrDefaultAsync(u => u.Login.Username == user.Login.Username);

        if (existingUsername != null)
        {
            // User with the same email already exists
            return Results.BadRequest("User with the same username already exists.");
        }

        var existingEmail = await dbContext.Users
           .FirstOrDefaultAsync(u => u.Email == user.Email);

        if (existingEmail != null)
        {
            // User with the same email already exists
            return Results.BadRequest("User with the same email already exists.");
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

app.MapPost("/login", async (Login login, ApplicationDbContext dbContext) =>
{
    if (login.Username != "")
    {
        var existingLogin = await dbContext.Logins
            .Include(l => l.User) // Ensure the User navigation property is loaded
            .FirstOrDefaultAsync(u => u.Username == login.Username);

        if (existingLogin != null && PasswordManager.VerifyPassword(login.PasswordHash, existingLogin.PasswordHash))
        {
            // Update the corresponding User entity
            existingLogin.User.IsOnline = true;

            // Save changes to the database
            await dbContext.SaveChangesAsync();

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, existingLogin.UserId.ToString()),
            new Claim(ClaimTypes.Name, login.Username),
            // Add additional claims as needed
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("areallyhardpasswordtocrack"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "Backend",
                audience: "PostOnFront",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), // Token expiration time
                signingCredentials: creds
            );

            return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
        else
        {
            return Results.BadRequest("Wrong information!");
        }
    }

    return Results.BadRequest("Login information is required.");
}).WithName("login");


app.MapGet("/status", async (ApplicationDbContext dbContext) =>
{
    var users = await dbContext.Users
        .Include(l => l.Login) // Include the related User entity
        .ToListAsync();

    return users;
})
.WithName("VerifyStatus");

app.Run();
