using web_do_an1.Models;

namespace web_do_an1.Services
{
    public static class PasswordUtility
    {
        public static string Hash(UserAccount user, string password)
        {
            return password;
        }

        public static bool Verify(UserAccount user, string password, out bool shouldUpgrade)
        {
            shouldUpgrade = false;
            return user.Password == password;
        }

        public static int UsePlainTextDemoPasswords(web_do_an1.Data.EnglishCenterDbContext db)
        {
            var changed = 0;

            foreach (var user in db.Users)
            {
                var password = PlainTextDemoPassword(user);
                if (!string.IsNullOrEmpty(password) && user.Password != password)
                {
                    user.Password = password;
                    changed++;
                }
            }

            if (changed > 0)
            {
                db.SaveChanges();
            }

            return changed;
        }

        private static string? PlainTextDemoPassword(UserAccount user)
        {
            if (user.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return "admin123";
            }

            if (user.Password.StartsWith("AQAAAA")
                || user.UserName.Equals("nvdt", StringComparison.OrdinalIgnoreCase)
                || IsDemoCode(user.UserName, "gv")
                || IsDemoCode(user.UserName, "hv"))
            {
                return "123456";
            }

            return null;
        }

        private static bool IsDemoCode(string userName, string prefix)
        {
            return userName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && userName.Length > prefix.Length
                && userName[prefix.Length..].All(char.IsDigit);
        }
    }
}
