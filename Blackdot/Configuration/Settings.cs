using System.Collections.Generic;

namespace Blackdot.Configuration
{
    public class Settings
    {
        public List<SearchEngine> SearchEngines { get; set; }
    }

    public class SearchEngine
    {
        public string Name { get; set; }

        public string URL { get; set; }

        public string XPathResultNodes { get; set; }

        public string XPathTitleNodes { get; set; }

        public string XPathUrlNodes { get; set; }

        public string XPathSummaryNodes { get; set; }
    }
}
