namespace Server.DataModels
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("BuyerWishlistItems")]
    public class BuyerWishlistItemsEntity
    {
        public int BuyerId { get; set; }

        public int ProductId { get; set; }
    }
}