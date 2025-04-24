using System.Threading.Tasks;

namespace MarketPlace924.Service
{
    public interface IPDFService
    {
        Task<int> InsertPdfAsync(byte[] fileBytes);
    }
}