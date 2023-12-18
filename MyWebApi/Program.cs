using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using servicesToUse;
var builder = WebApplication.CreateBuilder(args);


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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("areallyhardpasswordtocrackaaaaaaaaaaaaaaaa"))
        };
    });
builder.Services.AddAuthorization();
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
    options.AddDefaultPolicy(
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                      });
});


builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Enable detailed error messages
});
var app = builder.Build();


app.MapHub<FriendsHub>("/friendsHub");

app.UseCors();
// Use authentication middleware
app.UseAuthentication();

app.UseAuthorization();

app.MapPost("/token", async (HttpContext context) =>
{
    var authorizationHeader = context.Request.Headers["Authorization"].ToString();
    var token = authorizationHeader.Replace("Bearer ", "");
    return servicesToUse.JWTToken.refreshToken(token);
}).RequireAuthorization()
.WithName("GetToken");

app.MapGet("/userDetails", async (string userId, ApplicationDbContext dbContext) =>
{
    var userDetails = await dbContext.Users
        .Where(u => u.UserId == int.Parse(userId))
        .Select(u => u) // Include the related User entity
        .ToListAsync();

    return userDetails;
}).RequireAuthorization()
.WithName("GetUserDetails");

app.MapGet("/friendPosts", async (string userId, ApplicationDbContext dbContext) =>
{
    var userDetails = await dbContext.Users
        .Where(u => u.UserId == int.Parse(userId))
        .Select(u => u) // Include the related User entity
        .ToListAsync();

    return userDetails;
}).RequireAuthorization()
.WithName("GetFriendPosts");

app.MapPost("/requestFriendship", async (int userRequestingId, int userReceivingId, ApplicationDbContext dbContext) =>
{
    // Check if a friendship request already exists between these users
    var existingFriendship = await dbContext.Friendships
        .FirstOrDefaultAsync(f => (f.CreatedBy == userRequestingId && f.FriendId == userReceivingId)
                               || (f.CreatedBy == userReceivingId && f.FriendId == userRequestingId));

    if (existingFriendship != null)
    {
        return Results.BadRequest("A friendship request already exists between these users.");
    }

    var newFriendship = new Friendship
    {
        CreatedBy = userRequestingId,
        FriendId = userReceivingId,
        CreatedOn = DateTime.UtcNow,
        UpdatedBy = userRequestingId,
        UpdatedOn = DateTime.UtcNow,
        State = FriendState.Pending
    };

    dbContext.Friendships.Add(newFriendship);
    await dbContext.SaveChangesAsync();

    return Results.Ok(new { Message = "Friendship request sent successfully." });
})
.RequireAuthorization()
.WithName("RequestFriendship");

app.MapGet("/friends", async (string userId, ApplicationDbContext dbContext) =>
{
    var parsedUserId = int.Parse(userId); // Convert userId to int once and reuse

    var friendIdsCreatedBy = await dbContext.Friendships
        .Where(f => f.CreatedBy == parsedUserId && f.FriendId != parsedUserId
                    && f.State == FriendState.Accepted) // Check for Accepted state
        .Select(f => f.FriendId)
        .ToListAsync();

    var friendIdsFriendId = await dbContext.Friendships
        .Where(f => f.FriendId == parsedUserId && f.CreatedBy != parsedUserId
                    && f.State == FriendState.Accepted) // Check for Accepted state
        .Select(f => f.CreatedBy)
        .ToListAsync();

    var friendIds = friendIdsCreatedBy.Union(friendIdsFriendId);

    var friends = await dbContext.Users
        .Where(u => friendIds.Contains(u.UserId))
        .OrderByDescending(u => u.IsOnline) // This will sort the users, putting online users first
        .Select(user => new
        {
            userId = user.UserId,
            firstName = user.FirstName,
            lastName = user.LastName,
            email = user.Email,
            profilePic = user.ProfilePic,
            isOnline = user.IsOnline,
            lastSeen = user.LastSeeOn
        })
        .ToListAsync();

    return Results.Ok(friends);
})
.WithName("GetFriends").RequireAuthorization();





app.MapGet("/users", async (ApplicationDbContext dbContext, string search) =>
{

       search = search.ToLower();
        var users = await dbContext.Users
       .Where(u => u.FirstName.ToLower().StartsWith(search) || u.LastName.ToLower().StartsWith(search))
       .OrderBy(u => u.FirstName) // or any other ordering logic
       .Take(10) // Limit the result to 6 users
       .ToListAsync();

        return users;

})
.WithName("GetUsers").RequireAuthorization();

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

app.MapPost("/login", async (Login login, ApplicationDbContext dbContext, IHubContext<FriendsHub> friendsHubContext) =>
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


            // Fetch the friends of the logged-in user
            var friendships = await dbContext.Friendships
    .Where(f => (f.CreatedBy == existingLogin.UserId && f.FriendId != existingLogin.UserId) || (f.FriendId == existingLogin.UserId && f.CreatedBy != existingLogin.UserId))
    .ToListAsync();


            foreach (var friendship in friendships)
            {
                int friendUserId;

                if (friendship.CreatedBy == existingLogin.UserId)
                {
                    // The friend is the one who received the friend request
                    friendUserId = friendship.FriendId;
                    await friendsHubContext.Clients.User(friendUserId.ToString()).SendAsync("FriendOnline", existingLogin.UserId);
                    Console.WriteLine(friendUserId + "CreatedBy");
                }
                else
                {
                    // The friend is the one who initiated the friend request
                    friendUserId = friendship.CreatedBy;
                    await friendsHubContext.Clients.User(friendUserId.ToString()).SendAsync("FriendOnline", existingLogin.UserId);
                    Console.WriteLine(friendUserId + "FriendId");
                }


            }
            return Results.Ok(servicesToUse.JWTToken.generateToken(existingLogin.UserId.ToString(), existingLogin.Username.ToString(), existingLogin.Role.ToString()));
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
