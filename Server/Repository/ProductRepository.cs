using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Shared;

// WILL NOT IMPLEMENT DUMMY PRODUCT REPOSITORY - SHOULD BE REPLACED WITH ACTUAL PRODUCT REPOSITORY -Alex
namespace Server.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        [ExcludeFromCodeCoverage]
        public ProductRepository(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public ProductRepository(string connectionString, IDatabaseProvider databaseProvider)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (databaseProvider == null)
            {
                throw new ArgumentNullException(nameof(databaseProvider));
            }

            this.connectionString = connectionString;
            this.databaseProvider = databaseProvider;
        }

        public async Task AddProductAsync(string name, double price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "AddProduct";
                    databaseCommand.CommandType = CommandType.StoredProcedure;

                    AddParameter(databaseCommand, "@Name", name);
                    AddParameter(databaseCommand, "@Price", price);
                    AddParameter(databaseCommand, "@SellerID", sellerId);
                    AddParameter(databaseCommand, "@ProductType", productType);
                    AddParameter(databaseCommand, "@StartDate", startDate);
                    AddParameter(databaseCommand, "@EndDate", endDate);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateProductAsync(int id, string name, double price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "UpdateProduct";
                    databaseCommand.CommandType = CommandType.StoredProcedure;

                    AddParameter(databaseCommand, "@ID", id);
                    AddParameter(databaseCommand, "@Name", name);
                    AddParameter(databaseCommand, "@Price", price);
                    AddParameter(databaseCommand, "@SellerID", sellerId);
                    AddParameter(databaseCommand, "@ProductType", productType);
                    AddParameter(databaseCommand, "@StartDate", startDate);
                    AddParameter(databaseCommand, "@EndDate", endDate);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteProduct(int id)
        {
            using (IDbConnection databaseConnection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand databaseCommand = databaseConnection.CreateCommand())
                {
                    databaseCommand.CommandText = "DeleteProduct";
                    databaseCommand.CommandType = CommandType.StoredProcedure;

                    AddParameter(databaseCommand, "@ID", id);

                    await databaseConnection.OpenAsync();
                    await databaseCommand.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "GetSellerById";
                    command.CommandType = CommandType.StoredProcedure;

                    if (sellerId.HasValue)
                    {
                        AddParameter(command, "@SellerID", sellerId.Value);
                    }
                    else
                    {
                        AddParameter(command, "@SellerID", DBNull.Value);
                    }

                    await connection.OpenAsync();

                    object result = await command.ExecuteScalarAsync();
                    return result?.ToString();
                }
            }
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "GetProductByID";
                    command.CommandType = CommandType.StoredProcedure;

                    AddParameter(command, "@productID", productId);

                    await connection.OpenAsync();

                    using (IDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Product
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ID")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Price = (double)reader.GetDouble(reader.GetOrdinal("Price")),
                                SellerId = reader.IsDBNull(reader.GetOrdinal("SellerID")) ? 0 : (int)reader.GetInt32(reader.GetOrdinal("SellerID")),
                                ProductType = reader.GetString(reader.GetOrdinal("ProductType")),
                                StartDate = reader.IsDBNull(reader.GetOrdinal("StartDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("EndDate"))
                            };
                        }

                        return null;
                    }
                }
            }
        }

        private void AddParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;

            if (value == null)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = value;
            }

            command.Parameters.Add(parameter);
        }
    }
}