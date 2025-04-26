// <copyright file="PDFRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.Repository
{
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Server.DBConnection;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Represents a repository for PDF operations.
    /// </summary>
    public class PDFRepository : IPDFRepository
    {
        private readonly MarketPlaceDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the PDFRepository class.
        /// </summary>
        /// <param name="dbContext">The database context instance.</param>
        public PDFRepository(MarketPlaceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Asynchronously inserts a PDF into the database.
        /// </summary>
        /// <param name="fileBytes">The byte array representing the PDF file.</param>
        /// <returns>The ID of the inserted PDF.</returns>
        public async Task<int> InsertPdfAsync(byte[] fileBytes)
        {
            var pdfToInsert = new PDF
            {
                ContractID = 0, // initialize with 0, anyway we don't use this in the database (ignored this field in the table creation, but kept it in order to prevent errors)
                PdfID = 0, // initialize with 0, will be set by the database
                File = fileBytes,
            };

            await this.dbContext.PDFs.AddAsync(pdfToInsert);
            await this.dbContext.SaveChangesAsync();

            return pdfToInsert.PdfID; // return the PDF ID that was set by the database
        }
    }
}
