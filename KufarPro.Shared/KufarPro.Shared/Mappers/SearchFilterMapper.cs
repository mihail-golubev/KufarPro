using KufarPro.Shared.Messaging.Models;
using KufarPro.Shared.Models.Search;

namespace KufarPro.Shared.Mappers
{
    public static class SearchFilterMapper
    {
        public static NewFilter MapToNewFilter(this SearchFilter filter)
        {
            var newFilter = new NewFilter
            {
                ChatIds = filter.ChatIds,
                UrlQuery = filter.UrlQuery
            };

            return newFilter;
        }
    }
}
