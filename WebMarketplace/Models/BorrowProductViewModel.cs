using System;

namespace WebMarketplace.Models
{
    /// <summary>
    /// View model for the borrow product view
    /// </summary>
    public class BorrowProductViewModel
    {
        /// <summary>
        /// Gets or sets the product ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product name
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the product price
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Gets or sets the seller ID
        /// </summary>
        public int SellerId { get; set; }

        /// <summary>
        /// Gets or sets the seller name
        /// </summary>
        public string SellerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the product type
        /// </summary>
        public string ProductType { get; set; } = string.Empty;

        private DateTime? _startDate;
        /// <summary>
        /// Gets or sets the start date (availability date)
        /// </summary>
        public DateTimeOffset? StartDate 
        { 
            get => _startDate; 
            set
            {
                if (value is DateTimeOffset dateTimeOffset)
                {
                    _startDate = dateTimeOffset.DateTime;
                }
                else
                {
                    _startDate = null;
                }
            }
        }

        private DateTime? _endDate;
        /// <summary>
        /// Gets or sets the end date (unavailable until)
        /// </summary>
        public DateTimeOffset? EndDate 
        { 
            get => _endDate; 
            set
            {
                if (value is DateTimeOffset dateTimeOffset)
                {
                    _endDate = dateTimeOffset.DateTime;
                }
                else
                {
                    _endDate = null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the product is available
        /// </summary>
        public bool IsAvailable => !EndDate.HasValue || EndDate.Value == DateTime.MinValue;

        /// <summary>
        /// Gets or sets a value indicating whether the current user is on the waitlist
        /// </summary>
        public bool IsOnWaitlist { get; set; }

        /// <summary>
        /// Gets or sets the user's position in the waitlist
        /// </summary>
        public int WaitlistPosition { get; set; }

        /// <summary>
        /// Gets or sets the unread notifications count
        /// </summary>
        public int UnreadNotificationsCount { get; set; }

        /// <summary>
        /// Gets the formatted availability message based on dates
        /// </summary>
        public string AvailabilityMessage
        {
            get
            {
                if (IsAvailable)
                {
                    if (!StartDate.HasValue || StartDate.Value == DateTime.MinValue)
                    {
                        return "Availability: Now";
                    }
                    else
                    {
                        return $"Available after: {StartDate.Value:yyyy-MM-dd}";
                    }
                }
                else
                {
                    return $"Unavailable until: {EndDate.Value:yyyy-MM-dd}";
                }
            }
        }
    }
} 