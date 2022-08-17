using System.Xml.Serialization;
using RSSFeeder.External.Models;

namespace RSSFeeder;

public class XmlHelper
{
    public static FeedSettings GetFeedsSettingsFromFile(string path)
    {
        var xmlString = File.ReadAllText(path);
        using TextReader sr = new StringReader(xmlString);
        var serializer = new XmlSerializer(typeof(FeedSettings));
        return (FeedSettings)serializer.Deserialize(sr);
    }

    public static void WriteData(string? path, FeedSettings settings)
    {
        File.WriteAllText(path, "");
        var formatter = new XmlSerializer(settings.GetType());
        using var fs = new FileStream(path, FileMode.OpenOrCreate);
        formatter.Serialize(fs, settings);
    }
}