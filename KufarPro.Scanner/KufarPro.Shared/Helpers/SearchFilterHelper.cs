using KufarPro.Shared.Messaging.Models;
using KufarPro.Shared.Models.Search;

namespace KufarPro.Shared.Helpers
{
    public static class SearchFilterHelper
    {
        private static readonly object _lock = new();

        public static void AddOrUpdate(this List<SearchFilter> searchFilters, NewFilter newFilter)
        {
            lock (_lock)
            {
                var existingFilter = searchFilters.FirstOrDefault(x => x.UrlQuery == newFilter.UrlQuery);

                if (existingFilter != null)
                {
                    existingFilter.ChatIds = newFilter.ChatIds;
                }
                else
                {
                    searchFilters.Add(new SearchFilter
                    {
                        UrlQuery = newFilter.UrlQuery,
                        ChatIds = new List<long>(newFilter.ChatIds)
                    });
                }
            }
        }
    }
}
