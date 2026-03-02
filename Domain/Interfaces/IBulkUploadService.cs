using ITAMS.Models;

namespace ITAMS.Services
{
    public interface IBulkUploadService
    {
        Task<BulkUploadResult> ProcessAssetExcelAsync(Stream fileStream, int userId);
        byte[] GenerateSampleTemplate();
    }
}
