using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RealtYeahBackend.Helpers
{
    public class AuthOptions
    {
        public const string ISSUER = "webapi1";
        public const string AUDIENCE = "MyAuthClient";
        public const int LIFETIME = 1;
        
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes("erhtjkrelwqewrrІ"));
        }
    }
}
