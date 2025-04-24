using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;

namespace MarketPlace924.Repository
{
    /// <summary>
    /// Provides data access functionality for tracking orders and their checkpoints.
    /// </summary>
    public class TrackedOrderProxyRepository : ITrackedOrderRepository
    {
        public Task<int> AddOrderCheckpointAsync(OrderCheckpoint checkpoint)
        {
            throw new NotImplementedException();
        }

        public Task<int> AddTrackedOrderAsync(TrackedOrder order)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteOrderCheckpointAsync(int checkpointID)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteTrackedOrderAsync(int trackOrderID)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrderCheckpoint>> GetAllOrderCheckpointsAsync(int trackedOrderID)
        {
            throw new NotImplementedException();
        }

        public Task<List<TrackedOrder>> GetAllTrackedOrdersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<OrderCheckpoint> GetOrderCheckpointByIdAsync(int checkpointID)
        {
            throw new NotImplementedException();
        }

        public Task<TrackedOrder> GetTrackedOrderByIdAsync(int trackOrderID)
        {
            throw new NotImplementedException();
        }

        public Task UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, OrderStatus currentStatus)
        {
            throw new NotImplementedException();
        }
    }
}