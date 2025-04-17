using System.Threading.Tasks;

namespace Marketplace924.Service
{
    public interface IPDFService
    {
        Task<int> InsertPdfAsync(byte[] fileBytes);
    }
}