using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using JwtAuthServer.Data;
using JwtAuthServer.Providers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace JwtAuthServer.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            if (context.Clients.Any())
            {
                return;
            }
            context.Clients.AddRange(BuildClientsList());
            context.SaveChanges();

            
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            userManager.Create(new ApplicationUser()
            {
                UserName = "richard@penrose.me.uk"
            }, "Test123!");
        }

        private static List<Client> BuildClientsList()
        {
            var clientsList = new List<Client>
            {
                new Client
                {
                    Id = "ngAuthApp",
                    Secret = AuthHelper.GetHash("abc@123"),
                    Name = "AngularJS front-end Application",
                    ApplicationType = ApplicationTypes.JavaScript,
                    Active = true,
                    RefreshTokenLifeTime = 7200,
                    AllowedOrigin = "*"
                },
                new Client
                {
                    Id = "consoleApp",
                    Secret = AuthHelper.GetHash("123@abc"),
                    Name = "Console Application",
                    ApplicationType = ApplicationTypes.NativeConfidential,
                    Active = true,
                    RefreshTokenLifeTime = 14400,
                    AllowedOrigin = "*"
                }
            };

            return clientsList;
        }
    }
}