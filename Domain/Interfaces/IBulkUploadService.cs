using ITAMS.Models;

namespace ITAMS.Services
{
    public interface IBulkUploadService
    {
        Task<BulkUploadResult> ProcessAssetExcelAsync(Stream fileStream, int userId);
        Task<BulkUploadResult> ProcessLicensingExcelAsync(Stream fileStream, int userId);
        Task<BulkUploadResult> ProcessServiceExcelAsync(Stream fileStream, int userId);
        byte[] GenerateSampleTemplate();
        byte[] GenerateLicensingTemplate();
        byte[] GenerateServiceTemplate();
    }
}
