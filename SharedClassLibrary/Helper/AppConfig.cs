// <copyright file="AppConfig.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Helper
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// AppConfig class.
    /// Used for the configuration of the connection string which will be stored in the appsettings.json file (file which is not commited).
    /// </summary>
    public static class AppConfig
    {
        static AppConfig()
        {
            LoadConfiguration();
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public static IConfiguration? Configuration { get; private set; }

        /// <summary>
        /// Get the connection string
        /// !! Use throught the app like this: AppConfig.GetConnectionString("MyLocalDb");
        /// This will get the connection string from the appsettings.json file in the project.
        /// </summary>
        /// <param name="name">The name of the connection string to get, use MyLocalDb.</param>
        /// <returns>The connection string.</returns>
        public static string? GetConnectionString(string name)
        {
            return Configuration?[$"ConnectionStrings:{name}"];
        }

        public static string GetBaseApiUrl()
        {
            string? baseUrl = "https://localhost:7194/";

            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("Configuration 'BaseApiUrl' is required but not found or is empty.");
            }

            return baseUrl;
        }

        /// <summary>
        /// Load the configuration.
        /// </summary>
        private static void LoadConfiguration()
        {
            Debug.WriteLine("LMAOOOOO");
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            Debug.WriteLine(jsonFilePath);

            var builder = new ConfigurationBuilder()
                .AddJsonFile(jsonFilePath, optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }
    }
}
