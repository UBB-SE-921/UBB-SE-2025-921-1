// <copyright file="OrderNotificationEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.DataModels
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents the OrderNotification table structure for EF Core mapping.
    /// </summary>
    [Table("OrderNotifications")]
    public class OrderNotificationEntity
    {
        public int NotificationID { get; set; }

        public int RecipientID { get; set; }

        public string Category { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public bool IsRead { get; set; }

        public int? ContractID { get; set; }

        public bool? IsAccepted { get; set; }

        public int? ProductID { get; set; }

        public int? OrderID { get; set; }

        public string? ShippingState { get; set; }

        public DateTimeOffset? DeliveryDate { get; set; }

        public DateTimeOffset? ExpirationDate { get; set; }

    }
}