using System;

namespace SharedClassLibrary.Domain
{
    public class Order
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int BuyerID { get; set; }
        public string ProductType { get; set; } // will be populated from Products table based on ProductID
        public string PaymentMethod { get; set; } // constraint {'card', 'wallet', 'cash'}
        public int OrderSummaryID { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public int OrderHistoryID { get; set; }
    }
}
