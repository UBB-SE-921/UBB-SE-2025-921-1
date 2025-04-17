﻿namespace Marketplace924.Domain
{
    public class PredefinedContract : IPredefinedContract
    {
        public int ContractID { get; set; }
        public required string ContractContent { get; set; }
        public int ID { get; set; }
    }

    /// <summary>
    /// Enum for the predefined contract types.
    /// </summary>
    public enum PredefinedContractType
    {
        BuyingContract = 1,
        SellingContract = 2,
        BorrowingContract = 3
    }
}
