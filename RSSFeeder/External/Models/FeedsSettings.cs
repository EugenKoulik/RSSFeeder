using System.Xml.Serialization;

namespace RSSFeeder.External.Models;

[XmlRoot("Settings")]
public class FeedSettings
{
    [XmlElement("UpdateFrequency")]
    public int UpdateFrequency { get; set; }
    [XmlElement("NewsFeedLink")]
    public string? NewsFeedLink { get; set; }
}