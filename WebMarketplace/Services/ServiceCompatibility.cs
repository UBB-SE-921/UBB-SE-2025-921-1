using System.Collections.Generic;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service.Web;

namespace WebMarketplace.Services
{
    // Compatibility interfaces that forward to the new ones in SharedClassLibrary

    /// <summary>
    /// Compatibility layer for WebMarketplace.Services.IProductService
    /// </summary>
    public interface IProductService
    {
        Task<Product> GetProductByIdAsync(int productId);
        Task<string> GetSellerNameAsync(int sellerId);
    }

    /// <summary>
    /// Compatibility layer for WebMarketplace.Services.IWaitlistService
    /// </summary>
    public interface IWaitlistService
    {
        Task AddUserToWaitlist(int userId, int productId);
        Task RemoveUserFromWaitlist(int userId, int productId);
        Task<bool> IsUserInWaitlist(int userId, int productId);
        Task<int> GetUserWaitlistPosition(int userId, int productId);
    }

    /// <summary>
    /// Compatibility layer for WebMarketplace.Services.INotificationService
    /// </summary>
    public interface INotificationService
    {
        Task<int> GetUnreadNotificationsCountAsync(int userId);
        Task<List<NotificationModel>> GetUserNotificationsAsync(int userId);
    }

    /// <summary>
    /// Compatibility layer for WebMarketplace.Services.NotificationModel
    /// </summary>
    public class NotificationModel
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public System.DateTime Date { get; set; }
        public bool IsRead { get; set; }

        // Conversion from SharedClassLibrary model
        public static implicit operator NotificationModel(SharedClassLibrary.Service.Web.NotificationModel model)
        {
            return new NotificationModel
            {
                Id = model.Id,
                Message = model.Message,
                Date = model.Date,
                IsRead = model.IsRead
            };
        }

        // Conversion to SharedClassLibrary model
        public static implicit operator SharedClassLibrary.Service.Web.NotificationModel(NotificationModel model)
        {
            return new SharedClassLibrary.Service.Web.NotificationModel
            {
                Id = model.Id,
                Message = model.Message,
                Date = model.Date,
                IsRead = model.IsRead
            };
        }
    }

    // Adapter classes that implement the old interfaces but use the new services

    /// <summary>
    /// Adapter for WebMarketplace.Services.IProductService
    /// </summary>
    public class ProductServiceAdapter : IProductService
    {
        private readonly SharedClassLibrary.Service.Web.IWebProductService _service;

        public ProductServiceAdapter(SharedClassLibrary.Service.Web.IWebProductService service)
        {
            _service = service;
        }

        public Task<Product> GetProductByIdAsync(int productId) => _service.GetProductByIdAsync(productId);
        public Task<string> GetSellerNameAsync(int sellerId) => _service.GetSellerNameAsync(sellerId);
    }

    /// <summary>
    /// Adapter for WebMarketplace.Services.IWaitlistService
    /// </summary>
    public class WaitlistServiceAdapter : IWaitlistService
    {
        private readonly SharedClassLibrary.Service.Web.IWaitlistService _service;

        public WaitlistServiceAdapter(SharedClassLibrary.Service.Web.IWaitlistService service)
        {
            _service = service;
        }

        public Task AddUserToWaitlist(int userId, int productId) => _service.AddUserToWaitlist(userId, productId);
        public Task RemoveUserFromWaitlist(int userId, int productId) => _service.RemoveUserFromWaitlist(userId, productId);
        public Task<bool> IsUserInWaitlist(int userId, int productId) => _service.IsUserInWaitlist(userId, productId);
        public Task<int> GetUserWaitlistPosition(int userId, int productId) => _service.GetUserWaitlistPosition(userId, productId);
    }

    /// <summary>
    /// Adapter for WebMarketplace.Services.INotificationService
    /// </summary>
    public class NotificationServiceAdapter : INotificationService
    {
        private readonly SharedClassLibrary.Service.Web.INotificationService _service;

        public NotificationServiceAdapter(SharedClassLibrary.Service.Web.INotificationService service)
        {
            _service = service;
        }

        public Task<int> GetUnreadNotificationsCountAsync(int userId) => _service.GetUnreadNotificationsCountAsync(userId);

        public async Task<List<NotificationModel>> GetUserNotificationsAsync(int userId)
        {
            var notifications = await _service.GetUserNotificationsAsync(userId);
            return notifications.ConvertAll(n => (NotificationModel)n);
        }
    }
}