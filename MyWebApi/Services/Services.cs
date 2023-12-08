

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace servicesToUse
{
    public class PasswordManager
    {
        public static string HashPassword(string password, string salt)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }

        public static string Salt()
        {
            return BCrypt.Net.BCrypt.GenerateSalt();
        }

        public static bool VerifyPassword(string receivedPassword, string storedHashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(receivedPassword, storedHashedPassword);
        }
    }


    public class JWTToken
    {

        public static string refreshToken(string lasToken)
        {
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var jsonToken = handler.ReadToken(lasToken) as JwtSecurityToken;

                if (jsonToken != null)
                {
                    // Access claims from the decoded token
                    var userId = jsonToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                    var username = jsonToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
                    var role = jsonToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role")?.Value;
                    Console.WriteLine("ola"+userId);
                    Console.WriteLine("ola"+username);
                    Console.WriteLine("ola"+role);

                    if (userId != null || username != null || role != null)
                    {
                        Console.WriteLine("entrou no if");
                        return generateToken(userId, username, role);
                    }
                    else
                    {
                        Console.WriteLine("entrou no else");
                        return "No data";
                    }
                    // Use userId, username, or other claims as neede
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("entrou no catch");
                return ex.Message;
            }
        return "2Something is wrong";

        }

        public static string generateToken(string userId, string username, string roleId)
        {
            Console.WriteLine(roleId);
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, roleId.ToString()),
            // Add additional claims as needed
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("areallyhardpasswordtocrackaaaaaaaaaaaaaaaa"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "Backend",
                audience: "PostOnFront",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), // Token expiration time
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

}