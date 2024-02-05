using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gpsave
{
    public readonly struct KnownGame
    {
        public string ProductId { get; init; }

        public string ProductName { get; init; }

        public string PackageName { get; init; }

        public KnownGame(string productId, string productName, string packageName)
        {
            ProductId = productId;
            ProductName = productName;
            PackageName = packageName;
        }
    }


    public sealed class KnownGameCatalog
    {
        static readonly string gamePassPC_CatalogURL = "https://catalog.gamepass.com/sigls/v2?id=609d944c-d395-4c0a-9ea4-e9f39b52c1ad&language=en-us&market=US";

        static readonly string gamePass_ProductInfoURL = "https://displaycatalog.mp.microsoft.com/v7.0/products?bigIds={0}&market=US&languages=en-us";


        public List<KnownGame> Data = new List<KnownGame>();


        public KnownGameCatalog() { }


        public static KnownGameCatalog Load(string dataFile)
        {
            KnownGameCatalog r = null;
            using (FileStream stream = new FileStream(dataFile, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    r = (KnownGameCatalog)serializer.Deserialize(reader, typeof(KnownGameCatalog));
                }
            }
            return r;
        }


        public void Save(string path)
        {
            string filepath = Path.Combine(path, "knowngames.json");

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            using (FileStream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, this);
                }
            }
        }


        public async static Task<string[]> GetCatalog()
        {
            HttpClient httpClient = new HttpClient();

            string responseContent = await httpClient.GetStringAsync(gamePassPC_CatalogURL);
            JObject json = JObject.Parse("{\"data\": " + responseContent + "}");
            JArray dataRoot = (JArray)json["data"];

            string[] products = new string[dataRoot.Count - 1];
            for (int i = 1; i < dataRoot.Count; ++i)
            {
                products[i - 1] = (string)dataRoot[i]["id"];
            }

            return products;
        }


        public async static Task<KnownGame> GetGame(string gameId)
        {
            HttpClient httpClient = new HttpClient();

            string responseContent = await httpClient.GetStringAsync(string.Format(gamePass_ProductInfoURL, gameId));
            JObject json = JObject.Parse(responseContent);

            string productName = (string)json["Products"][0]["LocalizedProperties"][0]["ProductTitle"];
            string packageName = (string)json["Products"][0]["Properties"]["PackageFamilyName"];

            return new KnownGame(gameId, productName, packageName);
        }
    }
}
