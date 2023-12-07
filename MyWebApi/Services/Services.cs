

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
    
}