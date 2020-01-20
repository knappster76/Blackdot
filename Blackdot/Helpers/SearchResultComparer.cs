using Blackdot.Models;
using System;
using System.Collections.Generic;

namespace Blackdot.Helpers
{
    public class SearchResultComparer : IEqualityComparer<SearchResultViewModel>
    {
        public bool Equals(SearchResultViewModel x, SearchResultViewModel y)
        {
            if (string.Equals(x.URL, y.URL, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public int GetHashCode(SearchResultViewModel obj)
        {
            return obj.URL.GetHashCode();
        }
    }
}
