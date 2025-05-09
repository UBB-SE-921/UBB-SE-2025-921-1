// <copyright file="DummyCardEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.DataModels
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents the DummyCard table structure for EF Core mapping.
    /// OBS: This is the exact same table as Maria had it, it has no foreign keys to any buyers, nothing :)).
    /// Change if needed.
    /// </summary>
    [Table("DummyCards")]
    public class DummyCardEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DummyCardEntity"/> class.
        /// Required by EF Core for the migrations.
        /// </summary>
        public DummyCardEntity()
        {
            // Initialize properties to default values if necessary, especially non-nullable strings
            this.CardholderName = string.Empty;
            this.CardNumber = string.Empty;
            this.CVC = string.Empty;
            this.Country = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyCardEntity"/> class.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="cardholderName">The name of the cardholder.</param>
        /// <param name="cardNumber">The number of the card.</param>
        /// <param name="cvc">The CVC of the card.</param>
        /// <param name="month">The month of the card.</param>
        /// <param name="year">The year of the card.</param>
        /// <param name="country">The country of the card.</param>
        /// <param name="balance">The balance of the card.</param>
        public DummyCardEntity(int buyerId, string cardholderName, string cardNumber, string cvc, int month, int year, string country, double balance)
        {
            this.ID = 0; // (default int value) this should be automatically generated by the DB
            this.BuyerId = buyerId;
            this.CardholderName = cardholderName;
            this.CardNumber = cardNumber;
            this.CVC = cvc;
            this.Month = month;
            this.Year = year;
            this.Country = country;
            this.Balance = balance;
        }

        /// <summary>
        /// Gets or sets the ID of the card.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the buyer.
        /// </summary>
        public int BuyerId { get; set; }

        /// <summary>
        /// Gets or sets the name of the cardholder.
        /// </summary>
        public string CardholderName { get; set; }

        /// <summary>
        /// Gets or sets the number of the card.
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets the CVC of the card.
        /// </summary>
        public string CVC { get; set; }

        /// <summary>
        /// Gets or sets the month of the card.
        /// </summary>
        public int Month { get; set; } // Maria had this as varchar(2) -> changed to int since it is not used in the application and it is more manageable

        /// <summary>
        /// Gets or sets the year of the card.
        /// </summary>
        public int Year { get; set; } // Maria had this as varchar(2) -> changed to int since it is not used in the application and it is more manageable

        /// <summary>
        /// Gets or sets the country of the card.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the balance of the card.
        /// </summary>
        public double Balance { get; set; }
    }
}