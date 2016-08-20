using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleClient
{
    public class Tokens
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    internal class Program
    {
        private const string BASE_URL = "http://localhost:57916";
        static void Main()
        {
            //RunAsync().Wait();
            //Login
            Console.Write("Login ");
            var accessToken = Login();
            //Console.Write("Refresh Token ");
            //accessToken = AuthenticateWithRefresh(accessToken.RefreshToken);
            Console.Write("Protected Method ");
            CallProtectedGet("/api/people/get", accessToken.AccessToken);

            Console.Read();
        }

        private static Tokens Login()
        {

            //var result2 = CallPostWithFormEncoded("api/people/Register", new[] {
            //        new KeyValuePair<string, string>("grant_type", "password"),
            //        new KeyValuePair<string, string>("username", "richard@penrose.me.uk"),
            //        new KeyValuePair<string, string>("password", "Test123!"),
            //        new KeyValuePair<string, string>("client_id", "ngAuthApp"),
            //        new KeyValuePair<string, string>("client_secret", "Test123!")});


            var result = CallPostWithFormEncoded("/Token", new[] {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", "richard@penrose.me.uk"),
                    new KeyValuePair<string, string>("password", "Test123!"),
                    new KeyValuePair<string, string>("client_id", "ngAuthApp"),
                    new KeyValuePair<string, string>("client_secret", "Test123!")});

            return CreateToken(result);
        }
        private static Tokens AuthenticateWithRefresh(string refreshToken)
        {
            var result = CallPostWithFormEncoded("/Token", new[] {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refreshToken),
                    new KeyValuePair<string, string>("client_id", "ngAuthApp")});

            return CreateToken(result);
        }

        private static string CallPostWithFormEncoded(string apiUrl, KeyValuePair<string, string>[] args)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BASE_URL);
                var content = new FormUrlEncodedContent(args);
                var result = client.PostAsync(apiUrl, content).Result;
                var resultContent = result.Content.ReadAsStringAsync().Result;
                Console.WriteLine("POST: " + apiUrl);
                Console.WriteLine("=======================================");
                Console.WriteLine(resultContent);
                Console.WriteLine();

                return resultContent;
            }
        }

        private static void CallProtectedGet(string apiUrl, string token)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(BASE_URL)
            };

            client.SetBearerToken(token);
            var response = client.GetStringAsync(apiUrl).Result;

            Console.WriteLine("GET: " + apiUrl);
            Console.WriteLine("=======================================");
            Console.WriteLine(JArray.Parse(response));
            Console.WriteLine();
        }

        private static Tokens CreateToken(string result)
        {
            dynamic obj = JsonConvert.DeserializeObject<dynamic>(result);

            return new Tokens()
            {
                AccessToken = obj.access_token,
                RefreshToken = obj.refresh_token
            };
        }

        private static void Generate32ByteKey()
        {
            var rnd = new Random();
            var b = new byte[32];
            rnd.NextBytes(b);

            var key = Convert.ToBase64String(b);
        }
    }
}