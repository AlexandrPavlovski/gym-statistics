using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GymStatistics
{
    internal class GoogleApiClientOld
    {
        private const string clientId = "188215911639-vfmu4svuiad08sn470uqb7h65mrtlevj.apps.googleusercontent.com";
        private const string clientSecret = "05jHhmew_R7ThqpQMb2UCUi-";
        private const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string tokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
        private const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";

        private static HttpClient httpClient = new HttpClient();

        internal GoogleApiClientOld()
        {

        }

        internal async Task GetSheetDataAsync(string spreadsheetId)
        {
            string state = RandomDataBase64url(32);
            string codeVerifier = RandomDataBase64url(32);
            string codeChallenge = Base64urlEncodeNoPadding(Sha256(codeVerifier));
            const string codeChallengeMethod = "S256";

            string redirectURI = $"http://{IPAddress.Loopback}:{GetRandomUnusedPort()}/";
            Console.WriteLine($"Redirect URI: {redirectURI}");

            string authCode = await AcquireAuthCodeAsync(redirectURI, state, codeChallenge, codeChallengeMethod);

            string accessToken = await PerformCodeExchangeAsync(authCode, codeVerifier, redirectURI);

            string sheetRequestURI = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}";
            var request = new HttpRequestMessage(HttpMethod.Get, sheetRequestURI);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await httpClient.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();
        }

        private async Task<string> AcquireAuthCodeAsync(string redirectURI, string state, string codeChallenge, string codeChallengeMethod)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(redirectURI);
            Console.WriteLine("Listening...");
            listener.Start();
            var gettingContextTask = listener.GetContextAsync();

            string authorizationRequest =
                $"{authorizationEndpoint}?response_type=code&scope=openid%20profile&redirect_uri={Uri.EscapeDataString(redirectURI)}&client_id={clientId}&state={state}&code_challenge={codeChallenge}&code_challenge_method={codeChallengeMethod}";

            Process.Start(authorizationRequest);

            var context = await gettingContextTask;

            var response = context.Response;
            var buffer = Encoding.UTF8.GetBytes("<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to the app.</body></html>");
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            _ = responseOutput
                .WriteAsync(buffer, 0, buffer.Length)
                .ContinueWith((task) =>
                {
                    responseOutput.Close();
                    listener.Stop();
                    Console.WriteLine("Http listener stopped");
                });

            // Checks for errors
            if (context.Request.QueryString.Get("error") != null)
            {
                Console.WriteLine($"OAuth authorization error: {context.Request.QueryString.Get("error")}");
                return null;
            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                Console.WriteLine($"Malformed authorization response. {context.Request.QueryString}");
                return null;
            }

            // extracts the code
            var code = context.Request.QueryString.Get("code");
            var incomingState = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incomingState != state)
            {
                Console.WriteLine($"Received request with invalid state ({incomingState})");
                return null;
            }
            Console.WriteLine($"Authorization code: {code}");

            return code;
        }

        private async Task<string> PerformCodeExchangeAsync(string code, string code_verifier, string redirectURI)
        {
            Console.WriteLine("Exchanging code for tokens...");

            string tokenRequestURI = "https://www.googleapis.com/oauth2/v4/token";
            string tokenRequestQuery = $"code={code}&redirect_uri={Uri.EscapeDataString(redirectURI)}&client_id={clientId}&code_verifier={code_verifier}&client_secret={clientSecret}&scope=&grant_type=authorization_code";

            var request = new HttpRequestMessage(HttpMethod.Post, $"{tokenRequestURI}?{tokenRequestQuery}");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseText);

            var tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);
            string accessToken = tokenEndpointDecoded["access_token"];

            return accessToken;
        }

        private int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        private string RandomDataBase64url(uint length)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return Base64urlEncodeNoPadding(bytes);
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        private string Base64urlEncodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }

        /// <summary>
        /// Returns the SHA256 hash of the input string.
        /// </summary>
        private byte[] Sha256(string inputStirng)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
            SHA256Managed sha256 = new SHA256Managed();
            return sha256.ComputeHash(bytes);
        }
    }
}
