#region Using Statements

using Blackdot.Configuration;
using Blackdot.Helpers;
using Blackdot.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

#endregion

namespace Blackdot.Controllers
{
    public class HomeController : Controller
    {
        private readonly Settings _config;
        private readonly ILogger<HomeController> _logger;
        private readonly string userAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0";

        public HomeController(ILogger<HomeController> logger, Settings settings)
        {
            _logger = logger;
            _config = settings;
        }

        public IActionResult Index()
        {
            return View(new SearchViewModel { NumOfResults = 50 });
        }

        [HttpGet]
        public JsonResult GetResults(string searchTerm, int numOfResults)
        {
            if (_config.SearchEngines.Count != 2)
                return Json(new { success = false, responseText = "Please specify two search engines in app settings" });

            if (string.IsNullOrWhiteSpace(searchTerm))
                return Json(new { success = false, responseText = "Please enter a search term" });

            if (numOfResults == 0)
                return Json(new { success = false, responseText = "Please select number of results to return" });

            var searchEngine1 = ProcessSearchEngine(_config.SearchEngines[0], searchTerm, numOfResults);
            var searchEngine2 = ProcessSearchEngine(_config.SearchEngines[1], searchTerm, numOfResults);

            // remove any entries from the second result that are in the first result based on the url
            var noDuplicates = searchEngine2.Except(searchEngine1, new SearchResultComparer());

            // build a result list of both the results, one from each going down each list
            var result = Enumerable.Zip(searchEngine1, noDuplicates, (a, b) => new[] { a, b }).SelectMany(a => a);

            // return the result list based on the number of results to return
            var searchResults = result.Take(numOfResults).ToList();

            return Json(new { success = true, searchResults });
        }

        private List<SearchResultViewModel> ProcessSearchEngine(SearchEngine searchEngine, string searchTerm, int numOfResults)
        {
            // get the url
            var searchUrl = searchEngine.URL.Replace("{searchTerm}", searchTerm).Replace("{numOfResults}", numOfResults.ToString());

            // get the HTML from the search
            var result = GetSearchResult(searchUrl);

            var results = new List<SearchResultViewModel>();

            // I have used the HTML Agility Pack to traverse and select the HTML nodes
            // create a new HTML document and load the search result HTML string
            var doc = new HtmlDocument();
            doc.LoadHtml(result);

            // select all the search result elements from the HTML document
            var htmlNodes = doc.DocumentNode.SelectNodes(searchEngine.XPathResultNodes);

            // loop through all the nodes extracting the title, url and summary
            foreach (var node in htmlNodes)
            {
                var title = node.SelectSingleNode(searchEngine.XPathTitleNodes)?.InnerText ?? string.Empty;
                var url = node.SelectSingleNode(searchEngine.XPathUrlNodes)?.Attributes["href"].Value ?? string.Empty;
                var summary = node.SelectSingleNode(searchEngine.XPathSummaryNodes)?.InnerText ?? string.Empty;

                // ignore if title or summary are empty
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(summary))
                    continue;

                // add to a result collection
                results.Add(new SearchResultViewModel
                {
                    Title = title,
                    URL = url,
                    Summary = summary
                });
            }

            return results;
        }

        public string GetSearchResult(string url)
        {
            // create a HttpWebRequest and set the User Agent and the Proxy
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = userAgent;
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;

            // get the response
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                // get the response stream
                using (var streamResponse = response.GetResponseStream())
                {
                    // read the stream to the end and return it
                    using (var streamRead = new StreamReader(streamResponse))
                    {
                        return streamRead.ReadToEnd();
                    }
                }
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
