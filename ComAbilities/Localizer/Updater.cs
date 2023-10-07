namespace Localizer
{
    using System;
    using System.Security.Policy;
    using System.Text;
    using Exiled.API.Features;
    using Exiled.API.Interfaces;
    using UnityEngine;
    using Utf8Json;
    using UnityEngine.Networking;
    using System.Collections;
    using MEC;

    public class Updater : MonoBehaviour
    {
        private const string Url = "https://api.github.com/repos/Ruemena/ComAbilities/releases/latest";

        private Version CurrentVersion;
        private string DllName;

        public int State { get; private set; } = 0;
        public bool IsSafeToStop => State != 1;
        public HttpClient Client { get; set; }



        public Updater(Version currentVersion, string url, string dllName, HttpClient client) {
            CurrentVersion = currentVersion;
            DllName = dllName;
            Client = client;
        }


        private IEnumerator<float> SearchForUpdates()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(Url))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                yield return Timing.WaitUntilDone(request.SendWebRequest());

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string content = request.downloadHandler.text;

                    ExpectedResponse? expectedResponse = JsonSerializer.Deserialize<ExpectedResponse>(content);

                    if (expectedResponse?.assets != null)
                    {

                    }
                } else
                {
                    Log.Warn("[UPDATER] Unable to check for updates.");
                }

            }
        }

        public async void Start(IPlugin<IConfig> plugin)
        {
            Log.Debug("Hi");
            using HttpClient client = Client;
            Log.Debug("Obtained client");
            HttpResponseMessage response = await client.GetAsync(Url).ConfigureAwait(false);
            Log.Debug("Got response");
            if (!response.IsSuccessStatusCode) Log.Warn("Unable to search for updates");

            string content = await response.Content.ReadAsStringAsync();
            Log.Debug(content);
            ExpectedResponse? expectedResponse = JsonSerializer.Deserialize<ExpectedResponse>(content);
            if (expectedResponse != null)
            {
                Log.Debug("Again");
                ExpectedResponse.Asset[]? assets = expectedResponse?.assets;
                if (assets != null)
                {
                    Log.Debug(assets.First().url);
                }
            }
        }
    }

    public class ExpectedResponse
    {
        public class Asset {
            public string? url;
            public string? name;
        }

        public Asset[]? assets;
    }
}