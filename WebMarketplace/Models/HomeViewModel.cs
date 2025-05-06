using System.Collections.Generic;

namespace WebMarketplace.Models
{
    public class HomeViewModel
    {
        public IEnumerable<BuyerFamilySyncItemViewModel> BuyerFamilySyncItems { get; set; }
        public IEnumerable<BuyerFamilySyncViewModel> BuyerFamilySyncs { get; set; }
        public IEnumerable<BuyerBadgeViewModel> BuyerBadges { get; set; }
        public IEnumerable<BuyerAddressViewModel> BuyerAddresses { get; set; }
        public IEnumerable<BuyerAddressViewModel> BuyerShippingAddresses { get; set; }
    }
}
