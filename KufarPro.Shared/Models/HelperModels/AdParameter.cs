namespace KufarPro.Shared.Models.HelperModels
{
    public class AdParameter
    {
        public string Property { get; set; } // p - property name
        public string Label { get; set; }   // pl - readable parameter label
        public string ValueText { get; set; } // vl - readable value
        public string ValueRaw { get; set; }  // v - not readable value
    }
}
