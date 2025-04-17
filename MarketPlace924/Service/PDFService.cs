using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marketplace924.Repository;
using Marketplace924.Shared;

namespace Marketplace924.Service
{
    public class PDFService : IPDFService
    {
        private IPDFRepository pdfRepository;
        public PDFService()
        {
            pdfRepository = new PDFRepository(Configuration.CONNECTION_STRING);
        }

        public async Task<int> InsertPdfAsync(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                throw new ArgumentException("File bytes cannot be null or empty.", nameof(fileBytes));
            }
            return await pdfRepository.InsertPdfAsync(fileBytes);
        }
    }
}
