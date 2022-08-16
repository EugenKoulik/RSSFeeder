using System.Net;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using RSSFeeder.External.Models;
using RSSFeeder.Models;

namespace RSSFeeder.Controllers;

public class RSSFeedController : Controller
{
    private readonly IWebHostEnvironment _environment;

    public RSSFeedController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }
    public ActionResult Index()
    {
        var settings = XmlHelper.GetListOfFeedsSettings(Path.Combine(_environment.WebRootPath, "appSettings.xml"));
        try
        {
            GetFeed(settings.NewsFeedLink);
        }
        catch (WebException exception)
        {
            ViewBag.ErrorMessage = exception.Message;
        }

        if (!string.IsNullOrEmpty(ViewBag.ErrorMessage)) return View();
        
        ViewBag.URL = settings.NewsFeedLink;
        ViewBag.UpdateFrequency = settings.UpdateFrequency;
        ViewBag.RSSFeed = GetFeed(settings.NewsFeedLink);

        return View();
    }

    protected void Timer1_Tick(object sender, EventArgs e)
    {
        Console.WriteLine("Жопа");
    }
    
    [HttpPost]
    public ActionResult Index(string rssUrl, int frequency)
    {
        try
        {
            ViewBag.RSSFeed = GetFeed(rssUrl);
            ViewBag.URL = rssUrl;
            ViewBag.UpdateFrequency = frequency;
        }
        catch (WebException exception)
        {
            ViewBag.ErrorMessage = exception.Message;
        }
        
        if (string.IsNullOrEmpty(ViewBag.ErrorMessage))
        {
            XmlHelper.WriteData(Path.Combine(_environment.WebRootPath, "appSettings.xml"), 
                new FeedSettings(){UpdateFrequency = frequency, NewsFeedLink = rssUrl});
        }
        
        return View();
    }

    [HttpPost]
    public ContentResult AjaxMethod(string rssUrl) => Content(DateTime.Now.ToString("f") + rssUrl);

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