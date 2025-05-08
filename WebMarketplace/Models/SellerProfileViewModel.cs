using SharedClassLibrary.Domain;

namespace WebMarketplace.Models
{
    public class SellerProfileViewModel
    {
        public Seller Seller { get; set; }
        public List<Product> Products { get; set; }
    }
}
