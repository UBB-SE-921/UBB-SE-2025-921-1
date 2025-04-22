using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;

namespace Marketplace924.Repository
{
    public class PDFProxyRepository : IPDFRepository
    {
        public Task<int> InsertPdfAsync(byte[] fileBytes)
        {
            throw new NotImplementedException();
        }
    }
}
