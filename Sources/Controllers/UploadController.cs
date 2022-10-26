using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Web;
using YamlRestApiTester.Commun;
using YamlRestApiTester.Model;
using YamlHttpClient;
using YamlHttpClient.Settings;
using YamlHttpClient.Utils;

namespace YamlRestApiTester.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UploadController : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> Go([FromForm] IFormCollection form)
        {
            if (!form.TryGetValue("session_id", out var sessionId)) throw new Exception("Pas de session? WTF!");

            var repertoireTravail = GetTemporaryDirectory(new Guid(sessionId)) + "\\";
            var configFile = repertoireTravail + "config_api.yml";

            var cas = new List<CasEssai>();

            foreach (var file in form.Files)
            {
                if (file.FileName.EndsWith("config_api.yml", StringComparison.InvariantCultureIgnoreCase))
                {
                    using var destinationStream = new FileStream(configFile, FileMode.Create, FileAccess.Write, FileShare.None, 0, true);
                    await file.CopyToAsync(destinationStream);
                }
                else if (file.FileName.EndsWith(".yml", StringComparison.InvariantCultureIgnoreCase))
                {
                    var fichierCas = repertoireTravail + file.FileName.Replace('/', '_').Replace('\\', '_');
                    /*using var destinationStream = new FileStream(fichierCas, FileMode.Create, FileAccess.Write, FileShare.None, 0, false);
                    {
                        await file.CopyToAsync(destinationStream);
                        var casEssai = OutilsYaml.DeserializerString<CasEssai>(System.IO.File.ReadAllText(fichierCas));

                        cas.Add(casEssai);
                    }*/

                    string contents;
                    using (var sr = new StreamReader(file.OpenReadStream()))
                    {
                        contents = sr.ReadToEnd();
                    }
                    var casEssai = OutilsYaml.DeserializerStringSupportAlias<CasEssai>(contents);

                    cas.Add(casEssai);
                }
            }

            // Source config
            //var file = @"myYamlConfig.yml";

            foreach (var essai in cas)
            {
                var settings = new YamlHttpClientConfigBuilder()
                             .LoadFromFile(configFile, "api");

                settings.Content = essai.Content ?? new ContentSettings();

                var uriBuilder = new UriBuilder(settings.Url);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                if (essai.ParamsUrl is { })
                    foreach (var item in essai.ParamsUrl)
                    {
                        query.Add(item.Key, item.Value);
                    }

                uriBuilder.Query = query.ToString();

                settings.Url = uriBuilder.ToString();

                // Core builder, load settings from Yaml source
                YamlHttpClientFactory httpClient = new(settings);

                // Here the magic - Build Http message - Dynamically
                // from config with your object as data source, see yaml config below
                var request = httpClient.BuildRequestMessage(new { });

                // Inspect content if needed
                //var readContent = await request.Content?.ReadAsStringAsync()!;

                // Send it
                var response = await httpClient.SendAsync(request);

                // Do something with response
                var returnData = await response.Content.ReadAsStringAsync();

                //Comparer avec le retour attendu (conversion json/json)

                // Check some stuff from config
                await httpClient.CheckResponseAsync(response);
            }


            return Ok();
        }

        private static string GetTemporaryDirectory(Guid sessionId)
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), sessionId.ToString());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
