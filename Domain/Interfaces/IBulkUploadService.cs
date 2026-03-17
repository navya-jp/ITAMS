using ITAMS.Models;

namespace ITAMS.Services
{
    public interface IBulkUploadService
    {
        Task<BulkUploadResult> ProcessAssetExcelAsync(Stream fileStream, int userId, string usageCategory = "ITNonTMS");
        Task<BulkUploadResult> ProcessLicensingExcelAsync(Stream fileStream, int userId, string usageCategory = "TMS");
        Task<BulkUploadResult> ProcessServiceExcelAsync(Stream fileStream, int userId, string usageCategory = "TMS");
        byte[] GenerateSampleTemplate();
        byte[] GenerateLicensingTemplate();
        byte[] GenerateServiceTemplate();
    }
}
