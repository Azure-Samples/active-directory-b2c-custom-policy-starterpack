using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace AADB2C.GraphService
{
    public class B2CGraphClient
    {
        private AuthenticationContext authContext;
        private ClientCredential credential;
        static private AuthenticationResult AccessToken;

        public readonly string aadInstance = "https://login.microsoftonline.com/";
        public readonly string aadGraphResourceId = "https://graph.windows.net/";
        public readonly string aadGraphEndpoint = "https://graph.windows.net/";
        public readonly string aadGraphVersion = "api-version=1.6";

        public string Tenant { get; }
        public string ClientId { get; }
        public string ClientSecret { get; }

        public B2CGraphClient(string tenant, string clientId, string clientSecret)
        {
            this.Tenant = tenant;
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;

            // The AuthenticationContext is ADAL's primary class, in which you indicate the direcotry to use.
            this.authContext = new AuthenticationContext("https://login.microsoftonline.com/" + this.Tenant);

            // The ClientCredential is where you pass in your client_id and client_secret, which are 
            // provided to Azure AD in order to receive an access_token using the app's identity.
            this.credential = new ClientCredential(this.ClientId, this.ClientSecret);
        }

        /// <summary>
        /// Create consumer user accounts
        /// When creating user accounts in a B2C tenant, you can send an HTTP POST request to the /users endpoint
        /// </summary>
        public async Task CreateUser(string signInName, string password, string displayName, string givenName, string surname, bool generateRandomPassword)
        {
            if (string.IsNullOrEmpty(signInName))
                throw new Exception("Email address is NULL or empty, you must provide valid email address");

            if (string.IsNullOrEmpty(displayName) || displayName.Length < 1)
                throw new Exception("Dispay name is NULL or empty, you must provide valid dislay name");

            // Use random password for just-in-time migration flow
            if (generateRandomPassword)
            {
                password = GeneratePassword();
            }

            try
            {
                // Create Graph json string from object
                GraphUserModel graphUserModel = new GraphUserModel(signInName,password,displayName,givenName,surname);

                // Send the json to Graph API end point
                await SendGraphRequest("/users/", null, graphUserModel.ToString(), HttpMethod.Post);

                Console.WriteLine($"Azure AD user account '{signInName}' created");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ObjectConflict"))
                {
                    // TBD: Add you error Handling here
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"User with same emaill address '{signInName}' already exists in Azure AD");
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Search Azure AD user by signInNames property
        /// </summary>
        public async Task<string> SearcUserBySignInNames(string signInNames)
        {
            return await SendGraphRequest("/users/",
                            $"$filter=signInNames/any(x:x/value eq '{signInNames}')",
                            null, HttpMethod.Get);
        }

        /// <summary>
        /// Search Azure AD user by displayName property
        /// </summary>
        public async Task<string> SearchUserByDisplayName(string displayName)
        {
            return await SendGraphRequest("/users/",
                            $"$filter=displayName eq '{displayName}'",
                            null, HttpMethod.Get);
        }

        /// <summary>
        /// Update consumer user account's password
        /// </summary>
        /// <returns></returns>
        public async Task UpdateUserPassword(string signInName, string password)
        {
            string JSON = await this.SearcUserBySignInNames(signInName);

            GraphUsersModel users = GraphUsersModel.Parse(JSON);

            // If user exists
            if (users != null && users.value != null && users.value.Count==1)
            {
                // Generate JSON containing the password and password policy
                GraphUserSetPasswordModel graphPasswordModel = new GraphUserSetPasswordModel(password);
                string json = JsonConvert.SerializeObject(graphPasswordModel);

                // Send the request to Graph API
                await SendGraphRequest("/users/" + users.value[0].objectId, null, json, new HttpMethod("PATCH"));
            }
        }

        /// <summary>
        /// Delete user anccounts from Azure AD by SignInName (email address)
        /// </summary>
        public async Task DeleteAADUserBySignInNames(string signInName)
        {
            // First step, get the user account ID
            string JSON = await this.SearcUserBySignInNames(signInName);

            GraphUsersModel users = GraphUsersModel.Parse(JSON);

            // If the user account Id return successfully, iterate through all accounts
            if (users != null && users.value != null && users.value.Count > 0)
            {
                foreach (var item in users.value)
                {
                    // Send delete request to Graph API
                    await SendGraphRequest("/users/" + item.objectId, null, null, HttpMethod.Delete);
                }
            }
        }


        /// <summary>
        /// Handle Graph user API, support following HTTP methods: GET, POST and PATCH
        /// </summary>
        private async Task<string> SendGraphRequest(string api, string query, string data, HttpMethod method)
        {
            // Get the access toke to Graph API
            string acceeToken = await AcquireAccessToken();

            // Set the Graph url. Including: Graph-endpoint/tenat/users?api-version&query
            string url = $"{this.aadGraphEndpoint}{this.Tenant}{api}?{this.aadGraphVersion}";

            if (!string.IsNullOrEmpty(query))
            {
                url += "&" + query;
            }

            //Trace.WriteLine($"Graph API call: {url}");
            try
            {
                using (HttpClient http = new HttpClient())
                using (HttpRequestMessage request = new HttpRequestMessage(method, url))
                {
                    // Set the authorization header
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", acceeToken);

                    // For POST and PATCH set the request content 
                    if (!string.IsNullOrEmpty(data))
                    {
                        //Trace.WriteLine($"Graph API data: {data}");
                        request.Content = new StringContent(data, Encoding.UTF8, "application/json");
                    }

                    // Send the request to Graph API endpoint
                    using (HttpResponseMessage response = await http.SendAsync(request))
                    {
                        string error = await response.Content.ReadAsStringAsync();

                        // Check the result for error
                        if (!response.IsSuccessStatusCode)
                        {
                            // Throw server busy error message
                            if (response.StatusCode == (HttpStatusCode)429)
                            {
                                // TBD: Add you error handling here
                            }

                            throw new Exception(error);
                        }

                        // Return the response body, usually in JSON format
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception)
            {
                // TBD: Add you error handling here
                throw;
            }
        }

        private async Task<string> AcquireAccessToken()
        {
            // If the access token is null or about to be invalid, acquire new one
            if (B2CGraphClient.AccessToken == null ||
                (B2CGraphClient.AccessToken.ExpiresOn.UtcDateTime > DateTime.UtcNow.AddMinutes(-10)))
            {
                try
                {
                    B2CGraphClient.AccessToken = await authContext.AcquireTokenAsync(this.aadGraphResourceId, credential);
                }
                catch (Exception ex)
                {
                    // TBD: Add you error handling here
                    throw;
                }
            }

            return B2CGraphClient.AccessToken.AccessToken;
        }

        /// <summary>
        /// Generate temporary password
        /// </summary>
        private static string GeneratePassword()
        {
            const string A = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string a = "abcdefghijklmnopqrstuvwxyz";
            const string num = "1234567890";
            const string spe = "!@#$!&";

            string rv = GenerateLetters(4, A) + GenerateLetters(4, a) + GenerateLetters(4, num) + GenerateLetters(1, spe);
            return rv;
        }

        /// <summary>
        /// Generate random letters from string of letters
        /// </summary>
        private static string GenerateLetters(int length, string baseString)
        {
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(baseString[rnd.Next(baseString.Length)]);
            }
            return res.ToString();
        }
    }
}
