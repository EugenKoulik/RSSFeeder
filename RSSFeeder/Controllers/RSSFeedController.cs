using System.Net;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using RSSFeeder.External.Models;
using RSSFeeder.Models;

namespace RSSFeeder.Controllers;

public class RSSFeedController : Controller
{
    private readonly IWebHostEnvironment _environment;

    private readonly int minFrequency = 5;

    public RSSFeedController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }
    public ActionResult Index()
    {
        var settings = XmlHelper.GetFeedsSettingsFromFile(Path.Combine(_environment.WebRootPath, "appSettings.xml"));
        try
        {
            GetFeed(settings.NewsFeedLink);
        }
        catch (WebException exception)
        {
            ViewBag.Message = exception.Message;
        }
        
        if (settings.UpdateFrequency < minFrequency) ViewBag.Message = $"Frequency must be more {minFrequency}";
        if (!string.IsNullOrEmpty(ViewBag.Message)) return View();
        
        ViewBag.URL = settings.NewsFeedLink;
        ViewBag.UpdateFrequency = settings.UpdateFrequency;
        ViewBag.RSSFeed = GetFeed(settings.NewsFeedLink);

        return View();
    }

    [HttpPost]
    public ActionResult Index(string rssUrl, int frequency)
    {
        if (frequency < minFrequency)
        {
            ViewBag.Message = $"Frequency must be more {minFrequency}";
            return View();
        }

        if (string.IsNullOrEmpty(rssUrl))
        {
            ViewBag.Message = $"Url is null!";
            return View();
        }
        try
        {
            ViewBag.RSSFeed = GetFeed(rssUrl);
            ViewBag.URL = rssUrl;
            ViewBag.UpdateFrequency = frequency;
        }
        catch (Exception exception)
        {
            ViewBag.Message = exception.Message;
            return View();
        }
  
        XmlHelper.WriteData(Path.Combine(_environment.WebRootPath, "appSettings.xml"), 
            new FeedSettings(){UpdateFrequency = frequency, NewsFeedLink = rssUrl});

        return View();
    }

    private static IEnumerable<RSSFeed> GetFeed(string url)
    {
        var webClient = new WebClient();
        var rssData = webClient.DownloadString(url);
        var xml = XDocument.Parse(rssData);
        
        var rssFeedData = (from x in xml.Descendants("item")
            select new RSSFeed
            {
                Title = (string)x.Element("title"),
                Link = (string)x.Element("link"),
                Description = (string)x.Element("description"),
                PubDate = (string)x.Element("pubDate")
            });
        return rssFeedData;
    }
}