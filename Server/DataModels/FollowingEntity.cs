namespace Server.DataModels
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Followings")]
    public class FollowingEntity
    {
        public int BuyerId { get; set; }

        public int SellerId { get; set; }
    }
}