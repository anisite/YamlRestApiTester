using YamlHttpClient.Settings;

namespace YamlRestApiTester.Model;

public class CasEssai
{
    public IDictionary<string, string>? ParamsUrl { get; set; }
    public ContentSettings? Content { get; set; }
    public ContentSettings? RetourAttendu { get; set; }
}
