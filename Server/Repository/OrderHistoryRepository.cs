using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;

namespace Server.Repository
{
    /// <summary>
    /// Provides database operations for order history management.
    /// </summary>
    public class OrderHistoryRepository : IOrderHistoryRepository
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryRepository"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public OrderHistoryRepository(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryRepository"/> class with a specified database provider.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="databaseProvider">The database provider to use.</param>
        public OrderHistoryRepository(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }

        /// <inheritdoc/>
        public async Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryId)
        {
            List<Product> products = new List<Product>();

            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                await connection.OpenAsync();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "GetProductsFromOrderHistory";
                    command.CommandType = CommandType.StoredProcedure;

                    IDbDataParameter orderHistoryParameter = command.CreateParameter();
                    orderHistoryParameter.ParameterName = "@OrderHistory";
                    orderHistoryParameter.Value = orderHistoryId;
                    command.Parameters.Add(orderHistoryParameter);

                    using (IDataReader dataReader = await command.ExecuteReaderAsync())
                    {
                        while (await dataReader.ReadAsync())
                        {
                            Product product = new Product();

                            product.ProductId = dataReader.GetInt32(dataReader.GetOrdinal("productID"));
                            product.Name = dataReader.GetString(dataReader.GetOrdinal("name"));
                            product.Price = (double)dataReader.GetDouble(dataReader.GetOrdinal("price"));
                            product.ProductType = dataReader.GetString(dataReader.GetOrdinal("productType"));

                            if (dataReader["SellerID"] == DBNull.Value)
                            {
                                product.SellerId = 0;
                            }
                            else
                            {
                                product.SellerId = dataReader.GetInt32(dataReader.GetOrdinal("SellerID"));
                            }

                            if (dataReader["startDate"] == DBNull.Value)
                            {
                                product.StartDate = DateTime.MinValue;
                            }
                            else
                            {
                                product.StartDate = dataReader.GetDateTime(dataReader.GetOrdinal("startDate"));
                            }

                            if (dataReader["endDate"] == DBNull.Value)
                            {
                                product.EndDate = DateTime.MaxValue;
                            }
                            else
                            {
                                product.EndDate = dataReader.GetDateTime(dataReader.GetOrdinal("endDate"));
                            }

                            products.Add(product);
                        }
                    }
                }
            }

            return products;
        }
    }
} 