namespace Localizer
{
    using System;
    using System.Text;
    using Exiled.API.Features;
    using Exiled.API.Interfaces;
    using Utf8Json;

    public class Updater
    {
        public Version CurrentVersion;
        public string DllName;
        public string Url;
        public HttpClient Client { get; set; }
        public Updater(Version currentVersion, string url, string dllName, HttpClient client) {
            CurrentVersion = currentVersion;
            DllName = dllName;
            Url = url;
            Client = client;
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