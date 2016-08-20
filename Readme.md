#Javascript Web Tokens (JWT)#

##Why JWT##
###Background###
The traditional approach to Web Api security was to use an API key which is a known secret that is exchanged between the  client and the API. There are several downsides to this approach:

1. It provides an all or nothing approach to securing the API. Either you can access it or you can't.
2. It required some sort of persistence in the API layer to keep track of valid API keys.

###What are JWT's###
Javascript Web Tokens are passed between the client and the server normally in the HTTP header each time the client accesses the API. They are issued by an authorization server (which can be part of the Api itself) and allow the API to authorise the client and determine some information about the client via the claims contained within the token.

When the token is issued, it is signed typically using SHA1 or SHA256 using a secret that only the API layer knows. This means that although the JWT can be read, it cannot be tampered with.

####What does a JWT look like####

The JWT token is a simple string of three ‘.’ separated base 64 encoded values:

    <header>.<payload>.<hash>

e.g. 

#####Example Header#####
    {
      "typ": "JWT",
      "alg": "HS256"
    }

#####Example Payload#####
    {
      "unique_name": "richard@penrose.me.uk",
      "sub": "richard@penrose.me.uk",
      "role": "user",
      "iss": "http://localhost:57916",
      "aud": "DootrixWebApi",
      "exp": 1455884103,
      "nbf": 1455884073
    }

#####Creating the Token#####
The token is signed by taking the header and payload, base 64 encoding them, concatenating with ‘.’ and then generating a hash value using the given algorithm. The resulting byte array is also base 64 encoded and concatenated to produce the complete token. When creating the token two things that are required are the audienceId and a secret.

- **AudienceId** - This refers to the resource servers that should accept the token. In  the case above this has been set to "DootrixApi". Each resource server will configure its own audience id and will not accept tokens that do not list it as one of its audience ids.
- **Secret** this is the key the is used to sign the token. It must be a base 64 string whichi is either 32, 48 or 64 bytes.
- **Client Id** - This is required when using refresh tokens.
 
An example JWT is shown below: 
    `eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1bmlxdWVfbmFtZSI6InJpY2hhcmRAcGVucm9zZS5tZS51ayIsInN1YiI6InJpY2hhcmRAcGVucm9zZS5tZS51ayIsInJvbGUiOiJ1c2VyIiwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo1NzkxNiIsImF1ZCI6IkRvb3RyaXhXZWJBcGkiLCJleHAiOjE0NTU4ODQxMDMsIm5iZiI6MTQ1NTg4NDA3M30.ZhAmpjgK_hMmD012UBr46fiuFQ8HADLxbww9MM5IboA`

####Benefits of JWT's####
1. Cross Domain API calls. Because it’s just a header rather than a cookie, you don’t have any of the cross-domain browser problems that you get with cookies. It makes implementing single-sign-on much easier because the app that issues the token doesn’t need to be in any way connected with the app that consumes it. They merely need to have access to the same shared secret encryption key.
2. No server affinity. Because the token contains all the necessary user identification, there’s no for shared server state – a call to a database or shared session store.
3. Simple to implement clients. It’s easy to consume the API from other servers, or mobile apps.
4. The reduce the number of times the user name and password are passed across the network thus making it less likely they will be intercepted. Tokens will have an expiry date time, so if they are intercepted the will only provide limited access to the API.

##The Demo App##
The solution "ApiWithTokens.sln" contains 4 projects:

![](http://i.imgur.com/uxVH099.png)

The projects in this solution are as follows:

1. **JwtAuthServer** - The authorisation server with a simple resource API method
2. **ConsoleClient** - A .Net console app which accesses the authorization server and resource API.
3. **SimpleWebClient** - A simple web app which uses JQuery to access the Authorisation server and resource server.
4. **WebClient** - An angular JS app which uses the authorisation and resource server.

The key project in this solution is the JwtAuthServer. Using the OWIN pipeline this project provides a Token endpoint for authentication and builds in a Token consumer for incoming calls to the resource API.

All of the JWT plumbing is configured in the **Startup** class in this project: The method: **ConfigureOAuthWithJwtFormatTokens** is used to set this up.
 
###The Token End Point###
The token end point for authentication is configured in the **ConfigureOAuthTokenGeneration** method.

###Token Consumption###
The token consumption, i.e. using the token to authenticate requests is configured in the following method: **ConfigureOAuthTokenGeneration**

###Resource Api###
The JwtAuthServer also contains a simple resource API: **JwtAuthServer.Api.PersonApiController**. This API controller contains a method: **Get** which is protected using a custom Attribute: **ClaimsAuthorization**. This source code for this Attribute is contained within the project and it simply asserts that the specified claims exist. These claims will have been extracted from the JWT token by the OWIN middle ware.
 
###The Logic  Flow###
Before accessing a protected resourse each client application must authenticate and obtain a JWT. This JWT is then passed in the header of each request. NOTE: the JWT has an expiry date time and must be renewed once it has expired. The token can be renewed by either re authenticating or using refresh tokens - if implemented.  

###Demo Code Set Up###
1. First clone the Git repo.
2. Rebuild the solution
3. Run the Migrations:

![](http://i.imgur.com/SP5m69O.png)

**Before running the migrations** ensure that the Default project is set to "JwtAuthServer" and that the JwtAuthServer project is set as the startup.

##Inspecting  JWT Tokens##

There is a useful tool here that can be used for decoding JWT tokens:

http://jwt.io/

You will need to post in your secret to chack that the signature is valid.

##Security##
JWT's are as secure as the cookies used in standard web authentication. As long as you use HTTPS the JWT cannot be intercepted in flight. However, they could potentially be retrieved from local storage via a malicious script on the clients browser. **NOTE:** If a JWT was stolen the contents could not be changed as they are digitally signed.

In the Demo app the Token is signed using the SHA256 algorithm and a 32 byte secret key.
##Important implementation details###
1. JWT's should have a short life span.
2. We **must** use **HTTPS**!
3. We must ensure that the JWT handler does not accept the "none" algorithm  - see here: https://auth0.com/blog/2015/03/31/critical-vulnerabilities-in-json-web-token-libraries/.
4. The JWT in the sample is created using the ThinkTecture libraries. The HmacSha256, Sha256Digestand algorithms are used for the symmetric key. The key is based on a secret provided in the web.config file. This must be changed in production code.  


###Token signatures###
The sample code uses the ThinkTecture HmacSigningCredentials class which uses a SHA256 algorithm to create symmetric keys. To generate a new key, use the following code snippet:

    private static void Generate32ByteKey()
    {
        var rnd = new Random();
        var b = new byte[32];
        rnd.NextBytes(b);

        var key = Convert.ToBase64String(b);
    }

##Refresh Tokens, why?##
1. Enables JWT's to be short lived, thus the token content is always up to date e.g. claims and roles etc. It also makes the access token more secure because it is being regularly refreshed.
2. Allows us to revoke access. NOTE: there is no way to revoke access from a long life JWT.
3. No need to store or ask for username and password in mobile / web apps. Auth server can issue a very long lived refresh token - e.g. a year.

###Cons - Refresh Tokens###
They add a fair amount of complexity as we will need to persist the refresh tokens and some client identifiers in a database. An example implementation is available in the Demo App.

##Alternative to refresh tokens##
Rether than implementing refresh tokens we can set the JWT to expire every 30mins say. This will force the client to re login every 30mins, but this can be handled behind the scenes by the calling app. The downside of this is that the app will need to store a user name and password, but this is not unusual. 

##Things to consider##
1. If decide to use JWT's and possibly refresh tokens, we need to come up with a pattern for handling re login's. This can be quite complicated in an async language like JS...
2. **We need to work out the most secure way to store JWT's in the client.**

###Further Reading###

https://jwt.io/introduction/

http://mikehadlow.blogspot.co.uk/2014/04/json-web-tokens-owin-and-angularjs.html

http://bitoftech.net/2015/02/16/implement-oauth-json-web-tokens-authentication-in-asp-net-web-api-and-identity-2/

http://bitoftech.net/2014/10/27/json-web-token-asp-net-web-api-2-jwt-owin-authorization-server/

http://odetocode.com/blogs/scott/archive/2015/01/15/using-json-web-tokens-with-katana-and-webapi.aspx

http://bitoftech.net/2014/07/16/enable-oauth-refresh-tokens-angularjs-app-using-asp-net-web-api-2-owin/

https://auth0.com/blog/2015/03/31/critical-vulnerabilities-in-json-web-token-libraries/

https://www.chosenplaintext.ca/2015/03/31/jwt-algorithm-confusion.html

####JWT Storage####
https://stormpath.com/blog/where-to-store-your-jwts-cookies-vs-html5-web-storage/