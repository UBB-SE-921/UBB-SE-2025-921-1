// <copyright file="DatabaseConnection.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.DBConnection
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Server.Helper;

    /// <summary>
    /// Represents a database connection.
    /// </summary>
    public class DatabaseConnection : IDisposable
    {
        private readonly SqlConnection dbConnection;
        private readonly bool ownsConnection; // Flag to indicate if this instance owns the connection lifecycle

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnection"/> class using the default connection string from AppConfig.
        /// </summary>
        public DatabaseConnection()
        {
            this.dbConnection = new SqlConnection(AppConfig.GetConnectionString("MyLocalDb"));
            this.ownsConnection = true; // This instance creates and owns the connection
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnection"/> class using the provided connection string.
        /// Primarily used for testing purposes.
        /// </summary>
        /// <param name="connectionString">The database connection string to use.</param>
        public DatabaseConnection(string connectionString)
        {
            this.dbConnection = new SqlConnection(connectionString);
            this.ownsConnection = true; // This instance creates and owns the connection
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnection"/> class using an existing SqlConnection.
        /// The lifecycle of the provided connection is managed externally.
        /// </summary>
        /// <param name="existingConnection">An existing SqlConnection instance.</param>
        internal DatabaseConnection(SqlConnection existingConnection) // Consider making internal or adjusting access as needed
        {
            this.dbConnection = existingConnection ?? throw new ArgumentNullException(nameof(existingConnection));
            this.ownsConnection = false; // This instance uses an existing connection, does not own it
        }

        /// <summary>
        /// Gets the database connection.
        /// </summary>
        /// <returns>The database connection.</returns>
        public SqlConnection GetConnection()
        {
            return this.dbConnection;
        }

        /// <summary>
        /// Opens the database connection synchronously.
        /// </summary>
        public void OpenConnectionSync()
        {
            if (this.dbConnection.State == System.Data.ConnectionState.Closed)
            {
                this.dbConnection.Open();
            }
        }

        /// <summary>
        /// Opens the database connection asynchronously.
        /// </summary>
        /// <returns>The database connection.</returns>
        public async Task OpenConnection()
        {
            // Only open if owned and closed
            if (this.ownsConnection && this.dbConnection.State == System.Data.ConnectionState.Closed)
            {
                await this.dbConnection.OpenAsync();
            }
        }

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        public void CloseConnection()
        {
            // Only close if owned and open
            if (this.ownsConnection && this.dbConnection.State == System.Data.ConnectionState.Open)
            {
                this.dbConnection.Close();
            }
        }

        /// <summary>
        /// Executes a procedure.
        /// </summary>
        public void ExecuteProcedure()
        {
            throw new NotImplementedException("Executing a procedure was not implemented when the application was developed.");
        }

        /// <summary>
        /// Disposes the database connection if this instance owns it.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources.
                if (this.ownsConnection)
                {
                    this.dbConnection?.Dispose();
                }
            }
            // Free unmanaged resources (unmanaged objects) and override a finalizer below.
            // Set large fields to null.
        }
    }
}