using SharedClassLibrary.Domain;
using System.Collections.Generic;

namespace WebMarketplace.Models
{
    public class MyMarketProfileViewModel
    {
        public Seller Seller { get; set; }
        public IEnumerable<Product> Products { get; set; }
    }
}
