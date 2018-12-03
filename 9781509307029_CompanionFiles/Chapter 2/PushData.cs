using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace walkthrough_push_data
{
    class Program
    {
        private static string token = string.Empty;

        static void Main(string[] args)
        {
            //Get an authentication access token
            token = GetToken();

            //Create a dataset in Power BI
            CreateDataset();

            //Get a dataset to add rows into a Power BI table
            string datasetId = GetDataset();

            //Add rows to a Power BI table
            AddRows(datasetId, "Letter");
        }

        #region Get an authentication access token
        private static string GetToken()
        {
            //The client id that Azure AD created when you registered your client app.
            string clientID = "{ClientID}";

            //RedirectUri you used when you register your app.
            //For a client app, a redirect uri gives Azure AD more details on the application that it will authenticate.
            // You can use this redirect uri for your client app
            string redirectUri = "https://login.live.com/oauth20_desktop.srf";

            //Resource Uri for Power BI API
            string resourceUri = "https://analysis.windows.net/powerbi/api";

            //OAuth2 authority Uri
            string authorityUri = "https://login.windows.net/common/oauth2/authorize";

            //Get access token:
            AuthenticationContext authContext = new AuthenticationContext(authorityUri);
            string token = authContext.AcquireToken(resourceUri, clientID, new Uri(redirectUri)).AccessToken;

            Console.WriteLine(token);
            Console.ReadLine();

            return token;
        }

        #endregion

        #region Create a dataset in Power BI
        private static void CreateDataset()
        {
            string powerBIDatasetsApiUrl = "https://api.powerbi.com/v1.0/myorg/datasets";
            
            //POST web request to create a dataset
            HttpWebRequest request = System.Net.WebRequest.Create(powerBIDatasetsApiUrl) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentLength = 0;
            request.ContentType = "application/json";

            //Add token to the request header
            request.Headers.Add("Authorization", String.Format("Bearer {0}", token));

            //Create dataset JSON for POST request
            string datasetJson = "{\"name\": \"Alphabet\", \"tables\": " +
                "[{\"name\": \"Letter\", \"columns\": " +
                "[{ \"name\": \"LetterID\", \"dataType\": \"Int64\"}, " +
                "{ \"name\": \"Name\", \"dataType\": \"string\"}, " +
                "{ \"name\": \"Type\", \"dataType\": \"string\"}" +
                "]}]}";

            //POST web request
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(datasetJson);
            request.ContentLength = byteArray.Length;

            //Write JSON byte[] into a Stream
            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(byteArray, 0, byteArray.Length);

                var response = (HttpWebResponse)request.GetResponse();

                Console.WriteLine(string.Format("Dataset {0}", response.StatusCode.ToString()));

                Console.ReadLine();
            }
        }
        #endregion

        #region Get a dataset to add rows into a Power BI table
        private static string GetDataset()
        {
            string powerBIDatasetsApiUrl = "https://api.powerbi.com/v1.0/myorg/datasets";

            //POST web request to create a dataset.
            HttpWebRequest request = System.Net.WebRequest.Create(powerBIDatasetsApiUrl) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "application/json";

            //Add token to the request header
            request.Headers.Add("Authorization", String.Format("Bearer {0}", token));

            string datasetId = string.Empty;
            //Get HttpWebResponse from GET request
            using (HttpWebResponse httpResponse = request.GetResponse() as System.Net.HttpWebResponse)
            {
                //Get StreamReader that holds the response stream
                using (StreamReader reader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    string responseContent = reader.ReadToEnd();

                    //TODO: Install NuGet Newtonsoft.Json package: Install-Package Newtonsoft.Json
                    //and add using Newtonsoft.Json
                    var results = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    //Get datasetId
                    var datasets = results["value"];

                    foreach (var ds in datasets)
                    {
                        //Return the id of the first Alphabet dataset
                        if (ds["name"] == "Alphabet")
                        {
                            datasetId = ds["id"];
                            break;
                        }
                    }

                    Console.WriteLine(String.Format("Dataset ID: {0}", datasetId));
                    Console.ReadLine();

                    return datasetId;
                }
            }
        }
        #endregion

        #region Add rows to a Power BI table
        private static void AddRows(string datasetId, string tableName)
        {
            string powerBIApiAddRowsUrl = String.Format("https://api.powerbi.com/v1.0/myorg/datasets/{0}/tables/{1}/rows", datasetId, tableName);

            //POST web request to add rows
            HttpWebRequest request = System.Net.WebRequest.Create(powerBIApiAddRowsUrl) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentLength = 0;
            request.ContentType = "application/json";

            //Add token to the request header
            request.Headers.Add("Authorization", String.Format("Bearer {0}", token));

            //JSON content for Letter row
            string rowsJson = "{\"rows\":" +
                "[{\"LetterID\":1,\"Name\":\"A\",\"Type\":\"Vowel\"}," +
                "{\"LetterID\":2,\"Name\":\"B\",\"Type\":\"Consonant\"}," +
                "{\"LetterID\":3,\"Name\":\"C\",\"Type\":\"Consonant\"}]}";

            //POST web request
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(rowsJson);
            request.ContentLength = byteArray.Length;

            //Write JSON byte[] into a Stream
            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(byteArray, 0, byteArray.Length);

                var response = (HttpWebResponse)request.GetResponse();

                Console.WriteLine("Rows Added");

                Console.ReadLine();
            }
        }
        #endregion
    }
}