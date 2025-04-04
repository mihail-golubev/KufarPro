using AvKufarCarParser.Models.Database;

namespace AvKufarCarParser.EqualityComparers
{
    public class FilterParameterComparer : IEqualityComparer<FilterParameter>
    {
        public bool Equals(FilterParameter x, FilterParameter y)
        {
            return x.QueryName == y.QueryName && x.Value == y.Value;
        }

        public int GetHashCode(FilterParameter obj)
        {
            return HashCode.Combine(obj.QueryName, obj.Value);
        }
    }
}
