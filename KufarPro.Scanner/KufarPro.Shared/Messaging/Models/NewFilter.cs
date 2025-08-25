namespace KufarPro.Shared.Messaging.Models
{
    public class NewFilter
    {
        public string UrlQuery { get; set; }
        public List<long> ChatIds { get; set; } = new List<long>();
    }
}
