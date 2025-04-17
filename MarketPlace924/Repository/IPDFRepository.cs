using System.Threading.Tasks;

namespace Marketplace924.Repository
{
    public interface IPDFRepository
    {
        Task<int> InsertPdfAsync(byte[] fileBytes);
    }
}