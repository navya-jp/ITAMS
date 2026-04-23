using Microsoft.AspNetCore.Mvc;
using ITAMS.Domain.Interfaces;
using ITAMS.Models;
using ITAMS.Utilities;

namespace ITAMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : BaseController
{
    private readonly IReportService _reports;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reports, ILogger<ReportsController> logger)
    {
        _reports = reports;
        _logger = logger;
    }

    [HttpGet("dashboard-kpis")]
    public async Task<IActionResult> GetDashboardKpis()
    {
        try
        {
            var userId = GetCurrentUserId() ?? 1;
            var kpis = await _reports.GetDashboardKpisAsync(userId);
            return Ok(kpis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard KPIs");
            return StatusCode(500, new { message = ex.Message, detail = ex.InnerException?.Message });
        }
    }

    [HttpGet("asset-inventory")]
    public async Task<IActionResult> GetAssetInventory([FromQuery] AssetReportFilter filter)
    {
        var userId = GetCurrentUserId() ?? 1;
        var result = await _reports.GetAssetInventoryAsync(filter, userId);
        return Ok(result);
    }

    [HttpGet("warranty-expiry")]
    public async Task<IActionResult> GetWarrantyExpiry([FromQuery] int daysAhead = 30)
    {
        var userId = GetCurrentUserId() ?? 1;
        var result = await _reports.GetWarrantyExpiryReportAsync(daysAhead, userId);
        return Ok(result);
    }

    [HttpGet("license-expiry")]
    public async Task<IActionResult> GetLicenseExpiry([FromQuery] int daysAhead = 30)
    {
        var userId = GetCurrentUserId() ?? 1;
        var result = await _reports.GetLicenseExpiryReportAsync(daysAhead, userId);
        return Ok(result);
    }

    [HttpGet("contract-expiry")]
    public async Task<IActionResult> GetContractExpiry([FromQuery] int daysAhead = 30)
    {
        var userId = GetCurrentUserId() ?? 1;
        var result = await _reports.GetContractExpiryReportAsync(daysAhead, userId);
        return Ok(result);
    }

    [HttpGet("maintenance-summary")]
    public async Task<IActionResult> GetMaintenanceSummary([FromQuery] MaintenanceFilter filter)
    {
        var userId = GetCurrentUserId() ?? 1;
        var result = await _reports.GetMaintenanceSummaryAsync(filter, userId);
        return Ok(result);
    }

    [HttpGet("compliance-status")]
    public async Task<IActionResult> GetComplianceStatus([FromQuery] ComplianceFilter filter)
    {
        var userId = GetCurrentUserId() ?? 1;
        var result = await _reports.GetComplianceStatusReportAsync(filter, userId);
        return Ok(result);
    }

    [HttpGet("asset-transfer-history")]
    public async Task<IActionResult> GetTransferHistory([FromQuery] TransferFilter filter)
    {
        var userId = GetCurrentUserId() ?? 1;
        var result = await _reports.GetTransferHistoryAsync(filter, userId);
        return Ok(result);
    }

    [HttpGet("user-activity")]
    public async Task<IActionResult> GetUserActivity([FromQuery] UserActivityFilter filter)
    {
        var userId = GetCurrentUserId() ?? 1;
        var result = await _reports.GetUserActivityReportAsync(filter, userId);
        return Ok(result);
    }

    [HttpGet("alert-summary")]
    public async Task<IActionResult> GetAlertSummary()
    {
        var userId = GetCurrentUserId() ?? 1;
        var result = await _reports.GetAlertSummaryAsync(userId);
        return Ok(result);
    }

    [HttpPost("export/excel")]
    public async Task<IActionResult> ExportExcel([FromBody] ExportRequest request)
    {
        var userId = GetCurrentUserId() ?? 1;
        var bytes = await _reports.ExportToExcelAsync(request.ReportType, request.Filter, userId);
        var fileName = $"{request.ReportType}_{DateTimeHelper.Now:yyyyMMdd_HHmm}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpPost("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromBody] ExportRequest request)
    {
        try
        {
            var userId = GetCurrentUserId() ?? 1;
            _logger.LogInformation("PDF export request: type={Type}, filter={Filter}", request.ReportType, System.Text.Json.JsonSerializer.Serialize(request.Filter));
            var bytes = await _reports.ExportToPdfAsync(request.ReportType, request.Filter, userId);
            var fileName = $"{request.ReportType}_{DateTimeHelper.Now:yyyyMMdd_HHmm}.pdf";
            return File(bytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting PDF for {ReportType}: {Message}", request.ReportType, ex.Message);
            return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message, stack = ex.StackTrace?.Split('\n').FirstOrDefault() });
        }
    }
}
