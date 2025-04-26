namespace Server.DataModels
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents the BuyerLinkage table structure for EF Core mapping.
    /// Uses a composite key of RequestingBuyerId and ReceivingBuyerId.
    /// </summary>
    [Table("BuyerLinkages")] // Explicitly set table name
    public class BuyerLinkageEntity
    {
        // Composite Key Part 1
        public int RequestingBuyerId { get; set; }

        // Navigation property for the requesting buyer (optional, configure in DbContext)
        // public Buyer RequestingBuyer { get; set; }

        // Composite Key Part 2
        public int ReceivingBuyerId { get; set; }

        // Navigation property for the receiving buyer (optional, configure in DbContext)
        // public Buyer ReceivingBuyer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the linkage request has been approved.
        /// This maps directly to the database column used for status persistence.
        /// </summary>
        public bool IsApproved { get; set; }

        // Note: We don't include the BuyerLinkageStatus enum here directly.
        // The mapping between IsApproved and BuyerLinkageStatus will happen
        // in the repository when converting between BuyerLinkageEntity and BuyerLinkage.
    }
}
