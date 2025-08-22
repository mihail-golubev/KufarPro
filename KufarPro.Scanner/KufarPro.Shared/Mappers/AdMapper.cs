using KufarPro.Shared.Models.Ads;
using KufarPro.Shared.Models.DTOs;

namespace KufarPro.Shared.Mappers
{
    public static class AdMapper
    {
        public static NewAd MapToNewAd(Ad ad)
        {
            var newAd = new NewAd
            {
                Id = ad.Id,
                Url = ad.Url,
                Subject = ad.Subject,
                Author = ad.Author,
                ListTime = ad.ListTime,
                Images = ad.Images,
                City = ad.City,
                Region = ad.Region,
                Currency = ad.Currency,
                Price = ad.Price,
                AdType = ad.Type,
                IsPhoneHidden = ad.IsPhoneHidden
            };

            newAd.Parameters = ad.Parameters.Select(x =>
            {
                var value = string.IsNullOrEmpty(x.ValueText) ? x.ValueRaw : x.ValueText;
                return $"{x.Label}: {value}";
            });

            return newAd;
        }
    }
}
