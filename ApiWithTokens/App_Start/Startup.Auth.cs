using System;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using ApiWithTokens.Providers;
using ApiWithTokens.Models;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;

namespace ApiWithTokens
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context and user manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Configure the application for OAuth based flow
            PublicClientId = "self";
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                // In production mode set AllowInsecureHttp = false
                AllowInsecureHttp = true
            };

            // Enable the application to use bearer tokens to authenticate users
            
            //TODO
            //app.UseOAuthBearerTokens(OAuthOptions);

            app.UseOAuthAuthorizationServer(new MyOAuthOptions());
            app.UseJwtBearerAuthentication(new MyJwtOptions());

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //    consumerKey: "",
            //    consumerSecret: "");

            //app.UseFacebookAuthentication(
            //    appId: "",
            //    appSecret: "");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});
        }

    }

    public class MyOAuthOptions : OAuthAuthorizationServerOptions
    {
        public MyOAuthOptions()
        {
            TokenEndpointPath = new PathString("/Token");
            AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60);
            AccessTokenFormat = new MyJwtFormat();
            Provider = new MyOAuthProvider();
#if DEBUG
            AllowInsecureHttp = true;
#endif
        }
    }

    public class MyJwtFormat : ISecureDataFormat<AuthenticationTicket>
    {
        public string SignatureAlgorithm => "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";

        public string DigestAlgorithm => "http://www.w3.org/2001/04/xmlenc#sha256";

        public string Protect(AuthenticationTicket data)
        {
            if (data == null) throw new ArgumentNullException("data");

            var issuer = "localhost";
            var audience = "all";
            var key = Convert.FromBase64String("UHxNtYMRYwvfpO1dS5pWLKL0M2DgOj40EbN4SoBWgfc");
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(TimeSpan.FromMinutes(60).TotalMinutes);
            var signingCredentials = new SigningCredentials(
                                        //This could be: X509SecurityKey
                                        new InMemorySymmetricSecurityKey(key),
                                        SignatureAlgorithm,
                                        DigestAlgorithm);
            var token = new JwtSecurityToken(issuer, audience, data.Identity.Claims,
                                             now, expires, signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            throw new NotImplementedException();
        }
    }

    public class MyOAuthProvider : OAuthAuthorizationServerProvider
    {
        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity("otc");
            var username = context.OwinContext.Get<string>("otc:username");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", username));
            identity.AddClaim(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "user"));
            context.Validated(identity);
            return Task.FromResult(0);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            try
            {
                var username = context.Parameters["username"];
                var password = context.Parameters["password"];

                if (username == password)
                {
                    context.OwinContext.Set("otc:username", username);
                    context.Validated();
                }
                else
                {
                    context.SetError("Invalid credentials");
                    context.Rejected();
                }
            }
            catch
            {
                context.SetError("Server error");
                context.Rejected();
            }
            return Task.FromResult(0);
        }
    }

    public class MyJwtOptions : JwtBearerAuthenticationOptions
    {
        public MyJwtOptions()
        {
            var issuer = "localhost";
            var audience = "all";
            var key = Convert.FromBase64String("UHxNtYMRYwvfpO1dS5pWLKL0M2DgOj40EbN4SoBWgfc"); ;

            AllowedAudiences = new[] { audience };
            IssuerSecurityTokenProviders = new[]
            {
            new SymmetricKeyIssuerSecurityTokenProvider(issuer, key)
        };
        }
    }
}
