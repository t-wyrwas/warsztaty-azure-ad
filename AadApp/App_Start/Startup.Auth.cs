using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.IdentityModel.Claims;
using Microsoft.Owin.Security.Notifications;

namespace AadApp
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        public static readonly string Authority = aadInstance + tenantId;

        // This is the resource ID of the AAD Graph API.  We'll need this to request a token to call the Graph API.
        string graphResourceId = "https://graph.windows.net";

        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure: default sign in, cookie middleware, OIDC protocol middleware

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = Authority,
                    PostLogoutRedirectUri = postLogoutRedirectUri,
                    AuthenticationMode = AuthenticationMode.Passive,
                    AuthenticationType = tenantId,
                    //Scope = "openid profile email",
                    
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        // If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
                        AuthorizationCodeReceived = async (context) =>
                        {
                            await AuthCodeReceivedCallback(context, graphResourceId);
                        },

                        SecurityTokenValidated = (context) =>
                        {
                            var identity = context.AuthenticationTicket.Identity;
                            var userNameClaim = identity.FindFirst(ClaimTypes.Name);
                            var userName = userNameClaim.Value;

                            // todo: zrob cos w oparciu o dane z tokena, np. sprawdz czy uzytkownik o danym userName/email ma pozwolenie na dostęp do aplikacji

                            return Task.FromResult(0);
                        }
                        
                    }
                });
        }

        private static async Task<AuthenticationResult>  AuthCodeReceivedCallback(AuthorizationCodeReceivedNotification context, string graphResourceId)
        {
            var code = context.Code;
            ClientCredential credential = new ClientCredential(clientId, appKey);
            string signedInUserID = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
            AuthenticationContext authContext = new AuthenticationContext(Authority);
            return await authContext.AcquireTokenByAuthorizationCodeAsync(
            code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, graphResourceId);
        }
    }
}