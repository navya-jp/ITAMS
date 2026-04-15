using ITAMS.Data;
using ITAMS.Domain.Interfaces;
using ITAMS.Models;
using ITAMS.Utilities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ITAMS.Services;

public class ReportService : IReportService
{
    private readonly ITAMSDbContext _context;
    private readonly IAccessControlService _access;
    private readonly ILogger<ReportService> _logger;

    public ReportService(ITAMSDbContext context, IAccessControlService access, ILogger<ReportService> logger)
    {
        _context = context;
        _access = access;
        _logger = logger;
    }

    // ── Dashboard KPIs ────────────────────────────────────────────────────────

    public async Task<DashboardKpiDto> GetDashboardKpisAsync(int userId)
    {
        var today = DateTimeHelper.Now.Date;
        var in30 = today.AddDays(30);

        var totalHW = await _context.Assets.CountAsync();
        var totalLic = await _context.LicensingAssets.CountAsync();
        var totalSvc = await _context.ServiceAssets.CountAsync();
        var decomm = await _context.Assets.CountAsync(a => a.IsDecommissioned);
        var warExp = await _context.Assets.CountAsync(a => a.WarrantyEndDate != null && a.WarrantyEndDate >= today && a.WarrantyEndDate <= in30);
        var licExp = await _context.LicensingAssets.CountAsync(l => l.ValidityEndDate >= today && l.ValidityEndDate <= in30);
        var svcExp = await _context.ServiceAssets.CountAsync(s => s.ContractEndDate >= today && s.ContractEndDate <= in30);
        var openMaint = await _context.MaintenanceRequests.CountAsync(m => m.Status == "Open");
        var unacked = await _context.SystemAlerts.CountAsync(a => !a.IsAcknowledged && !a.IsResolved);
        var activeUsr = await _context.Users.CountAsync(u => u.IsActive);

        var byType = await _context.Assets
            .GroupBy(a => a.AssetTypeName)
            .Select(g => new ChartItemDto { Label = g.Key == null || g.Key == "" ? "Unknown" : g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();

        var byStatus = await _context.AssetStatuses
            .Select(s => new ChartItemDto { Label = s.StatusName, Count = s.Assets.Count() })
            .ToListAsync();

        var byLocation = await _context.Locations
            .Select(l => new ChartItemDto { 
                Label = l.Name.Replace(" Toll Plaza", "").Replace(" Toll plaza", "").Trim(), 
                Count = l.Assets.Count() 
            })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();

        var byProject = await _context.Projects
            .Select(p => new ChartItemDto { Label = p.Code, Count = p.Assets.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var cutoff12 = today.AddMonths(-12);
        var trend = await _context.Assets
            .Where(a => a.ProcurementDate != null && a.ProcurementDate >= cutoff12)
            .GroupBy(a => new { a.ProcurementDate!.Value.Year, a.ProcurementDate.Value.Month })
            .Select(g => new MonthlyTrendDto
            {
                Year = g.Key.Year, Month = g.Key.Month,
                Label = g.Key.Year + "-" + g.Key.Month.ToString("D2"),
                Count = g.Count()
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        return new DashboardKpiDto
        {
            TotalHardwareAssets = totalHW - decomm,
            TotalLicenses = totalLic,
            TotalServiceContracts = totalSvc,
            AssetsInUse = byStatus.FirstOrDefault(s => s.Label == "In Use")?.Count ?? 0,
            AssetsInRepair = byStatus.Where(s => s.Label.Contains("Repair")).Sum(s => s.Count),
            AssetsDecommissioned = decomm,
            WarrantyExpiringIn30Days = warExp,
            LicensesExpiringIn30Days = licExp,
            ContractsExpiringIn30Days = svcExp,
            OpenMaintenanceRequests = openMaint,
            FailedComplianceChecks = 0,
            UnacknowledgedAlerts = unacked,
            ActiveUsers = activeUsr,
            AssetsByType = byType,
            AssetsByStatus = byStatus,
            AssetsByLocation = byLocation,
            AssetsByProject = byProject,
            MonthlyProcurementTrend = trend
        };
    }
        // ── Asset Inventory ───────────────────────────────────────────────────────

    public async Task<AssetInventoryReport> GetAssetInventoryAsync(AssetReportFilter filter, int userId)
    {
        var query = _context.Assets
            .Include(a => a.Project).Include(a => a.Location)
            .Include(a => a.AssetStatus).Include(a => a.AssetType)
            .Include(a => a.Vendor).Include(a => a.AssignedUser)
            .Include(a => a.PatchStatus).Include(a => a.USBBlockingStatus)
            .AsQueryable();

        query = await _access.ApplyProjectFilter(query, userId);

        if (filter.ProjectId.HasValue) query = query.Where(a => a.ProjectId == filter.ProjectId);
        else if (filter.LocationType == "office") query = query.Where(a => a.ProjectId == 8);
        else if (filter.LocationType == "site") query = query.Where(a => a.ProjectId != 8);
        if (filter.LocationId.HasValue) query = query.Where(a => a.LocationId == filter.LocationId);
        if (!string.IsNullOrEmpty(filter.Status)) query = query.Where(a => a.AssetStatus != null && a.AssetStatus.StatusName == filter.Status);
        if (filter.IsDecommissioned.HasValue) query = query.Where(a => a.IsDecommissioned == filter.IsDecommissioned);
        else query = query.Where(a => !a.IsDecommissioned);
        if (filter.ProcuredFrom.HasValue) query = query.Where(a => a.ProcurementDate >= filter.ProcuredFrom);
        if (filter.ProcuredTo.HasValue) query = query.Where(a => a.ProcurementDate <= filter.ProcuredTo);

        var total = await query.CountAsync();
        var items = await query.OrderBy(a => a.AssetId)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new AssetInventoryReport
        {
            Total = total,
            Page = filter.PageNumber,
            PageSize = filter.PageSize,
            Items = items.Select(a => new AssetInventoryItem
            {
                AssetId = a.AssetId,
                AssetTag = a.AssetTag,
                Project = a.Project?.Name,
                Location = a.Location?.Name,
                AssetType = a.AssetType?.TypeName ?? a.AssetTypeName,
                Make = a.Make,
                Model = a.Model,
                SerialNumber = a.SerialNumber,
                Status = a.AssetStatus?.StatusName,
                AssignedUser = a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : a.AssignedUserText,
                Vendor = a.Vendor?.VendorName,
                ProcurementDate = a.ProcurementDate,
                ProcurementCost = a.ProcurementCost,
                WarrantyEndDate = a.WarrantyEndDate,
                PatchStatus = a.PatchStatus?.Name,
                USBStatus = a.USBBlockingStatus?.Name,
                UsageCategory = a.UsageCategory.ToString(),
                IsDecommissioned = a.IsDecommissioned,
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }

    // ── Expiry Reports ────────────────────────────────────────────────────────

    public async Task<List<ExpiryReportItem>> GetWarrantyExpiryReportAsync(int daysAhead, int userId)
    {
        var today = DateTimeHelper.Now.Date;
        var cutoff = today.AddDays(daysAhead);
        var items = await _context.Assets
            .Include(a => a.Project).Include(a => a.Location)
            .Where(a => a.WarrantyEndDate != null && a.WarrantyEndDate >= today && a.WarrantyEndDate <= cutoff && !a.IsDecommissioned)
            .OrderBy(a => a.WarrantyEndDate)
            .ToListAsync();

        return items.Select(a =>
        {
            var days = (int)(a.WarrantyEndDate!.Value - today).TotalDays;
            return new ExpiryReportItem
            {
                Id = a.Id, Identifier = a.AssetTag, Name = $"{a.Make} {a.Model}",
                Project = a.Project?.Name, Location = a.Location?.Name,
                ExpiryDate = a.WarrantyEndDate, DaysRemaining = days,
                Severity = days <= 7 ? "Critical" : days <= 30 ? "High" : "Medium"
            };
        }).ToList();
    }

    public async Task<List<ExpiryReportItem>> GetLicenseExpiryReportAsync(int daysAhead, int userId)
    {
        var today = DateTimeHelper.Now.Date;
        var cutoff = today.AddDays(daysAhead);
        var items = await _context.LicensingAssets
            .Where(l => l.ValidityEndDate >= today && l.ValidityEndDate <= cutoff)
            .OrderBy(l => l.ValidityEndDate).ToListAsync();

        return items.Select(l =>
        {
            var days = (int)(l.ValidityEndDate - today).TotalDays;
            return new ExpiryReportItem
            {
                Id = l.Id, Identifier = l.AssetTag, Name = l.LicenseName,
                ExpiryDate = l.ValidityEndDate, DaysRemaining = days,
                Severity = days <= 7 ? "Critical" : days <= 30 ? "High" : "Medium",
                Extra = $"{l.LicenseType} | {l.Vendor}"
            };
        }).ToList();
    }

    public async Task<List<ExpiryReportItem>> GetContractExpiryReportAsync(int daysAhead, int userId)
    {
        var today = DateTimeHelper.Now.Date;
        var cutoff = today.AddDays(daysAhead);
        var items = await _context.ServiceAssets
            .Include(s => s.Location)
            .Where(s => s.ContractEndDate >= today && s.ContractEndDate <= cutoff)
            .OrderBy(s => s.ContractEndDate).ToListAsync();

        return items.Select(s =>
        {
            var days = (int)(s.ContractEndDate - today).TotalDays;
            return new ExpiryReportItem
            {
                Id = s.Id, Identifier = s.AssetId, Name = s.ServiceName,
                Location = s.Location?.Name,
                ExpiryDate = s.ContractEndDate, DaysRemaining = days,
                Severity = days <= 7 ? "Critical" : days <= 30 ? "High" : "Medium",
                Extra = s.VendorName
            };
        }).ToList();
    }

    // ── Maintenance ───────────────────────────────────────────────────────────

    public async Task<List<MaintenanceReportItem>> GetMaintenanceSummaryAsync(MaintenanceFilter filter, int userId)
    {
        var query = _context.MaintenanceRequests
            .Include(m => m.Asset).ThenInclude(a => a.Project)
            .Include(m => m.Asset).ThenInclude(a => a.Location)
            .Include(m => m.Asset).ThenInclude(a => a.AssetType)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.Status)) query = query.Where(m => m.Status == filter.Status);
        if (filter.AssetId.HasValue) query = query.Where(m => m.AssetId == filter.AssetId);
        if (filter.From.HasValue) query = query.Where(m => m.CreatedAt >= filter.From);
        if (filter.To.HasValue) query = query.Where(m => m.CreatedAt <= filter.To);

        var items = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        var today = DateTimeHelper.Now.Date;

        return items.Select(m => new MaintenanceReportItem
        {
            Id = m.Id,
            AssetTag = m.Asset?.AssetTag ?? "",
            AssetType = m.Asset?.AssetType?.TypeName,
            Project = m.Asset?.Project?.Name,
            Location = m.Asset?.Location?.Name,
            RequestType = m.RequestType,
            Description = m.Description,
            Status = m.Status,
            ScheduledDate = m.ScheduledDate,
            CompletedDate = m.CompletedDate,
            Cost = m.Cost,
            VendorName = m.VendorName,
            CreatedByName = m.CreatedByName,
            DaysOpen = m.Status == "Open" ? (int)(today - m.CreatedAt.Date).TotalDays : 0,
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    // ── Compliance ────────────────────────────────────────────────────────────

    public async Task<List<ComplianceReportItem>> GetComplianceStatusReportAsync(ComplianceFilter filter, int userId)
    {
        var query = _context.ComplianceChecks
            .Include(c => c.Asset).ThenInclude(a => a.Project)
            .Include(c => c.Asset).ThenInclude(a => a.Location)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.Result)) query = query.Where(c => c.Result == filter.Result);

        var items = await query.OrderByDescending(c => c.CheckedAt).ToListAsync();

        return items.Select(c => new ComplianceReportItem
        {
            AssetTag = c.Asset?.AssetTag ?? "",
            Project = c.Asset?.Project?.Name,
            Location = c.Asset?.Location?.Name,
            CheckType = c.CheckType,
            Result = c.Result,
            Status = c.Status,
            CheckedAt = c.CheckedAt,
            Details = c.Details
        }).ToList();
    }

    // ── Transfers ─────────────────────────────────────────────────────────────

    public async Task<List<TransferReportItem>> GetTransferHistoryAsync(TransferFilter filter, int userId)
    {
        var query = _context.AssetTransferRequests
            .Include(t => t.Asset)
            .Include(t => t.FromLocation).Include(t => t.ToLocation)
            .Include(t => t.FromUser).Include(t => t.ToUser)
            .AsQueryable();

        if (filter.From.HasValue) query = query.Where(t => t.TransferDate >= filter.From);
        if (filter.To.HasValue) query = query.Where(t => t.TransferDate <= filter.To);

        var items = await query.OrderByDescending(t => t.TransferDate).ToListAsync();

        return items.Select(t => new TransferReportItem
        {
            Id = t.Id,
            TransferDate = t.TransferDate,
            AssetTag = t.Asset?.AssetTag ?? "",
            Make = t.Asset?.Make ?? "",
            Model = t.Asset?.Model ?? "",
            FromLocation = t.FromLocation?.Name,
            ToLocation = t.ToLocation?.Name,
            FromUser = t.FromUser != null ? $"{t.FromUser.FirstName} {t.FromUser.LastName}" : null,
            ToUser = t.ToUser != null ? $"{t.ToUser.FirstName} {t.ToUser.LastName}" : null,
            Reason = t.Reason,
            Status = t.Status,
            RequestedBy = t.RequestedByName
        }).ToList();
    }

    // ── User Activity ─────────────────────────────────────────────────────────

    public async Task<UserActivityReport> GetUserActivityReportAsync(UserActivityFilter filter, int userId)
    {
        var query = _context.LoginAudits
            .Include(la => la.User).ThenInclude(u => u.Role)
            .Include(la => la.SessionStatus)
            .Where(la => la.LoginTime >= filter.From && la.LoginTime <= filter.To)
            .AsQueryable();

        if (filter.UserId.HasValue) query = query.Where(la => la.UserId == filter.UserId);

        var items = await query.OrderByDescending(la => la.LoginTime).ToListAsync();

        var activityItems = items.Select(la => new UserActivityItem
        {
            Username = la.User?.Username ?? "",
            Role = la.User?.Role?.Name,
            LoginTime = la.LoginTime,
            LogoutTime = la.LogoutTime,
            SessionMinutes = la.LogoutTime.HasValue ? (int)(la.LogoutTime.Value - la.LoginTime).TotalMinutes : null,
            IpAddress = la.IpAddress,
            BrowserType = la.BrowserType,
            OperatingSystem = la.OperatingSystem,
            SessionStatus = la.SessionStatus?.Name
        }).ToList();

        var withDuration = activityItems.Where(a => a.SessionMinutes.HasValue).ToList();

        return new UserActivityReport
        {
            TotalSessions = activityItems.Count,
            AverageSessionMinutes = withDuration.Any() ? withDuration.Average(a => a.SessionMinutes!.Value) : 0,
            UniqueUsers = activityItems.Select(a => a.Username).Distinct().Count(),
            PeakLoginHour = activityItems.GroupBy(a => a.LoginTime.Hour).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? 0,
            Items = activityItems
        };
    }

    // ── Alert Summary ─────────────────────────────────────────────────────────

    public async Task<List<AlertReportItem>> GetAlertSummaryAsync(int userId)
    {
        var items = await _context.SystemAlerts
            .Where(a => !a.IsResolved)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return items.Select(a => new AlertReportItem
        {
            Id = a.Id,
            AlertType = a.AlertType,
            Severity = a.Severity,
            Title = a.Title,
            EntityIdentifier = a.EntityIdentifier,
            EscalationLevel = a.EscalationLevel,
            IsAcknowledged = a.IsAcknowledged,
            CreatedAt = a.CreatedAt,
            AcknowledgedAt = a.AcknowledgedAt
        }).ToList();
    }

    // ── Excel Export ──────────────────────────────────────────────────────────

    public async Task<byte[]> ExportToExcelAsync(string reportType, Dictionary<string, string>? filter, int userId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add(reportType);

        var headerColor = Color.FromArgb(59, 31, 107);

        switch (reportType.ToLower())
        {
            case "asset-inventory":
                var invFilter = new AssetReportFilter { PageSize = 10000 }; // export all, no pagination
                if (filter != null)
                {
                    if (filter.TryGetValue("locationType", out var lt) && !string.IsNullOrEmpty(lt))
                        invFilter.LocationType = lt;
                    if (filter.TryGetValue("projectId", out var pid) && int.TryParse(pid, out var pidVal))
                        invFilter.ProjectId = pidVal;
                }
                var inv = await GetAssetInventoryAsync(invFilter, userId);
                WriteHeaders(ws, new[] { "Asset ID", "Asset Tag", "Project", "Location", "Type", "Make", "Model", "Serial", "Status", "Assigned User", "Warranty End", "Procurement Cost" }, headerColor);
                int r = 2;
                foreach (var item in inv.Items)
                {
                    ws.Cells[r, 1].Value = item.AssetId; ws.Cells[r, 2].Value = item.AssetTag;
                    ws.Cells[r, 3].Value = item.Project; ws.Cells[r, 4].Value = item.Location;
                    ws.Cells[r, 5].Value = item.AssetType; ws.Cells[r, 6].Value = item.Make;
                    ws.Cells[r, 7].Value = item.Model; ws.Cells[r, 8].Value = item.SerialNumber;
                    ws.Cells[r, 9].Value = item.Status; ws.Cells[r, 10].Value = item.AssignedUser;
                    ws.Cells[r, 11].Value = item.WarrantyEndDate?.ToString("dd-MMM-yyyy");
                    ws.Cells[r, 12].Value = item.ProcurementCost;
                    r++;
                }
                break;

            case "warranty-expiry":
                var days = filter != null && filter.TryGetValue("daysAhead", out var d) ? int.Parse(d) : 30;
                var warItems = await GetWarrantyExpiryReportAsync(days, userId);
                WriteHeaders(ws, new[] { "Asset Tag", "Name", "Project", "Location", "Expiry Date", "Days Remaining", "Severity" }, headerColor);
                r = 2;
                foreach (var item in warItems)
                {
                    ws.Cells[r, 1].Value = item.Identifier; ws.Cells[r, 2].Value = item.Name;
                    ws.Cells[r, 3].Value = item.Project; ws.Cells[r, 4].Value = item.Location;
                    ws.Cells[r, 5].Value = item.ExpiryDate?.ToString("dd-MMM-yyyy");
                    ws.Cells[r, 6].Value = item.DaysRemaining; ws.Cells[r, 7].Value = item.Severity;
                    r++;
                }
                break;

            case "user-activity":
            {
                var uaFilter = new UserActivityFilter();
                if (filter != null)
                {
                    if (filter.TryGetValue("from", out var fromStr) && DateTime.TryParse(fromStr, out var fromDate))
                        uaFilter.From = fromDate;
                    if (filter.TryGetValue("to", out var toStr) && DateTime.TryParse(toStr, out var toDate))
                        uaFilter.To = toDate.AddDays(1); // include full end day
                }
                var ua = await GetUserActivityReportAsync(uaFilter, userId);
                WriteHeaders(ws, new[] { "Username", "Role", "Login", "Logout", "Duration", "IP", "Status" }, headerColor);
                int ru = 2;
                foreach (var item in ua.Items) {
                    ws.Cells[ru,1].Value=item.Username; ws.Cells[ru,2].Value=item.Role;
                    ws.Cells[ru,3].Value=item.LoginTime.ToString("dd-MMM-yyyy HH:mm");
                    ws.Cells[ru,4].Value=item.LogoutTime?.ToString("dd-MMM-yyyy HH:mm") ?? "—";
                    ws.Cells[ru,5].Value=item.SessionMinutes.HasValue ? $"{item.SessionMinutes}m" : "—";
                    ws.Cells[ru,6].Value=item.IpAddress;
                    ws.Cells[ru,7].Value=item.SessionStatus; ru++;
                }
                break;
            }
            case "alerts":
            {
                var alertItems = await GetAlertSummaryAsync(userId);
                WriteHeaders(ws, new[] { "Severity", "Type", "Title", "Entity", "Level", "Created", "Acknowledged" }, headerColor);
                int ra = 2;
                foreach (var item in alertItems) {
                    ws.Cells[ra,1].Value=item.Severity; ws.Cells[ra,2].Value=item.AlertType;
                    ws.Cells[ra,3].Value=item.Title; ws.Cells[ra,4].Value=item.EntityIdentifier;
                    ws.Cells[ra,5].Value=item.EscalationLevel;
                    ws.Cells[ra,6].Value=item.CreatedAt.ToString("dd-MMM-yyyy HH:mm");
                    ws.Cells[ra,7].Value=item.IsAcknowledged?"Yes":"No"; ra++;
                }
                break;
            }
            case "maintenance":
            {
                var mi = await GetMaintenanceSummaryAsync(new MaintenanceFilter(), userId);
                WriteHeaders(ws, new[] { "Asset Tag","Project","Location","Type","Description","Status","Days Open","Cost","Created" }, headerColor);
                int rm = 2;
                foreach (var item in mi) {
                    ws.Cells[rm,1].Value=item.AssetTag; ws.Cells[rm,2].Value=item.Project;
                    ws.Cells[rm,3].Value=item.Location; ws.Cells[rm,4].Value=item.RequestType;
                    ws.Cells[rm,5].Value=item.Description; ws.Cells[rm,6].Value=item.Status;
                    ws.Cells[rm,7].Value=item.DaysOpen; ws.Cells[rm,8].Value=item.Cost;
                    ws.Cells[rm,9].Value=item.CreatedAt.ToString("dd-MMM-yyyy"); rm++;
                }
                break;
            }
            case "compliance":
            {
                var ci = await GetComplianceStatusReportAsync(new ComplianceFilter(), userId);
                WriteHeaders(ws, new[] { "Asset Tag","Project","Location","Check Type","Result","Status","Checked At" }, headerColor);
                int rc = 2;
                foreach (var item in ci) {
                    ws.Cells[rc,1].Value=item.AssetTag; ws.Cells[rc,2].Value=item.Project;
                    ws.Cells[rc,3].Value=item.Location; ws.Cells[rc,4].Value=item.CheckType;
                    ws.Cells[rc,5].Value=item.Result; ws.Cells[rc,6].Value=item.Status;
                    ws.Cells[rc,7].Value=item.CheckedAt.ToString("dd-MMM-yyyy"); rc++;
                }
                break;
            }
            case "transfers":
            {
                var ti = await GetTransferHistoryAsync(new TransferFilter(), userId);
                WriteHeaders(ws, new[] { "Date","Asset Tag","From Location","To Location","From User","To User","Reason","By" }, headerColor);
                int rt = 2;
                foreach (var item in ti) {
                    ws.Cells[rt,1].Value=item.TransferDate.ToString("dd-MMM-yyyy");
                    ws.Cells[rt,2].Value=item.AssetTag; ws.Cells[rt,3].Value=item.FromLocation;
                    ws.Cells[rt,4].Value=item.ToLocation; ws.Cells[rt,5].Value=item.FromUser;
                    ws.Cells[rt,6].Value=item.ToUser; ws.Cells[rt,7].Value=item.Reason;
                    ws.Cells[rt,8].Value=item.RequestedBy; rt++;
                }
                break;
            }
            case "license":
            {
                var ld = filter!=null && filter.TryGetValue("daysAhead",out var ldv) ? int.Parse(ldv) : 30;
                var li = await GetLicenseExpiryReportAsync(ld, userId);
                WriteHeaders(ws, new[] { "Identifier","Name","Expiry Date","Days Remaining","Severity","Extra" }, headerColor);
                int rl = 2;
                foreach (var item in li) {
                    ws.Cells[rl,1].Value=item.Identifier; ws.Cells[rl,2].Value=item.Name;
                    ws.Cells[rl,3].Value=item.ExpiryDate?.ToString("dd-MMM-yyyy");
                    ws.Cells[rl,4].Value=item.DaysRemaining; ws.Cells[rl,5].Value=item.Severity;
                    ws.Cells[rl,6].Value=item.Extra; rl++;
                }
                break;
            }
            case "contract":
            {
                var cd = filter!=null && filter.TryGetValue("daysAhead",out var cdv) ? int.Parse(cdv) : 30;
                var cti = await GetContractExpiryReportAsync(cd, userId);
                WriteHeaders(ws, new[] { "Identifier","Service Name","Location","Expiry Date","Days Remaining","Severity","Vendor" }, headerColor);
                int rct = 2;
                foreach (var item in cti) {
                    ws.Cells[rct,1].Value=item.Identifier; ws.Cells[rct,2].Value=item.Name;
                    ws.Cells[rct,3].Value=item.Location;
                    ws.Cells[rct,4].Value=item.ExpiryDate?.ToString("dd-MMM-yyyy");
                    ws.Cells[rct,5].Value=item.DaysRemaining; ws.Cells[rct,6].Value=item.Severity;
                    ws.Cells[rct,7].Value=item.Extra; rct++;
                }
                break;
            }
            default:
                ws.Cells[1, 1].Value = "No data for report type: " + reportType;
                break;
        }

        ws.Cells.AutoFitColumns(10, 50);

        // Metadata sheet
        var meta = package.Workbook.Worksheets.Add("Info");
        meta.Cells[1, 1].Value = "Report Type"; meta.Cells[1, 2].Value = reportType;
        meta.Cells[2, 1].Value = "Generated At"; meta.Cells[2, 2].Value = DateTimeHelper.Now.ToString("dd-MMM-yyyy HH:mm IST");

        return package.GetAsByteArray();
    }

    private static void WriteHeaders(ExcelWorksheet ws, string[] headers, Color bgColor)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(bgColor);
            ws.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
            ws.Cells[1, i + 1].Style.Font.Bold = true;
        }
    }
}
