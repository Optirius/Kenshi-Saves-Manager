using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;

namespace KenshiSavesManager.Helpers
{
    public static class GoogleAuthHelper
    {
        private static readonly string[] Scopes = { DriveService.Scope.DriveAppdata, Google.Apis.Oauth2.v2.Oauth2Service.Scope.UserinfoEmail };
        

        public static async Task<UserCredential> LoginAsync()
        {
            UserCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                var clientSecrets = await GoogleClientSecrets.FromStreamAsync(stream);
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets.Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore("KenshiSavesManager.Auth.Store")
                );
            }

            return credential;
        }

        public static void Logout()
        {
            var fileDataStore = new FileDataStore("KenshiSavesManager.Auth.Store");
            fileDataStore.DeleteAsync<TokenResponse>("user").Wait();
        }
    }
}
