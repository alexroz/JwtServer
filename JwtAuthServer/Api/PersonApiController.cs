using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using JwtAuthServer.Attributes;
using JwtAuthServer.Data;
using JwtAuthServer.Models;

namespace JwtAuthServer.Api
{
    [RoutePrefix("api/people")]
    public class PersonApiController : ApiController
    {
        [ClaimsAuthorization(ClaimType = "role", ClaimValue = "user")]
        [Route("Get")]
        public object Get()
        {
            return new[]
            {
                new {Name = "Richard"},
                new {Name = "Craig"},
                new {Name = "Kevin"},
                new {Name = "Pete"}
            };
        }

        [ClaimsAuthorization(ClaimType = "role", ClaimValue = "user")]
        [Route("GetClaims")]
        public object GetClaims()
        {
            var identity = User.Identity as ClaimsIdentity;

            var claims = from c in identity.Claims
                         select new
                         {
                             subject = c.Subject.Name,
                             type = c.Type,
                             value = c.Value
                         };

            return claims;
        }

        [AllowAnonymous]
        [Route("Register")]
        public async Task<object> RegisterUser()
        {
            using (var repo = new AuthRepository())
            {
                await repo.RegisterUser(new UserModel
                {
                    UserName = "richard@penrose.me.uk",
                    Password = "Test123!",
                    ConfirmPassword = "Test123!"
                });

                return new { message = "success"  };
            }
        }
    }
}