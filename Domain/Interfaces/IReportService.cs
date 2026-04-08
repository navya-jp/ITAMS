using ITAMS.Models;

namespace ITAMS.Domain.Interfaces;

public interface IReportService
{
    Task<AssetInventoryReport> GetAssetInventoryAsync(AssetReportFilter filter, int userId);
    Task<List<ExpiryReportItem>> GetWarrantyExpiryReportAsync(int daysAhead, int userId);
    Task<List<ExpiryReportItem>> GetLicenseExpiryReportAsync(int daysAhead, int userId);
    Task<List<ExpiryReportItem>> GetContractExpiryReportAsync(int daysAhead, int userId);
    Task<List<MaintenanceReportItem>> GetMaintenanceSummaryAsync(MaintenanceFilter filter, int userId);
    Task<List<ComplianceReportItem>> GetComplianceStatusReportAsync(ComplianceFilter filter, int userId);
    Task<List<TransferReportItem>> GetTransferHistoryAsync(TransferFilter filter, int userId);
    Task<UserActivityReport> GetUserActivityReportAsync(UserActivityFilter filter, int userId);
    Task<List<AlertReportItem>> GetAlertSummaryAsync(int userId);
    Task<DashboardKpiDto> GetDashboardKpisAsync(int userId);
    Task<byte[]> ExportToExcelAsync(string reportType, Dictionary<string, string>? filter, int userId);
}
