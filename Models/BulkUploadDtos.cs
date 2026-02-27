namespace ITAMS.Models;

public class BulkUploadResult
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<BulkUploadError> Errors { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class BulkUploadError
{
    public int RowNumber { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class AssetExcelRow
{
    public int RowNumber { get; set; }
    public string Asset_Tag { get; set; } = string.Empty;
    public string? Serial_Number { get; set; }
    public string? Region { get; set; }
    public string? Plaza_Name { get; set; }
    public string? Location { get; set; }
    public string? Department { get; set; }
    public string Asset_Type { get; set; } = string.Empty;
    public string? Sub_Type { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? Asset_Classification { get; set; }
    public string? OS_Type { get; set; }
    public string? OS_Version { get; set; }
    public string? DB_Type { get; set; }
    public string? DB_Version { get; set; }
    public string? IP_Address { get; set; }
    public string? Assigned_User_Name { get; set; }
    public string? User_Role { get; set; }
    public string? Procured_By { get; set; }
    public string? Commissioning_Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Criticality { get; set; }
    public string Placing { get; set; } = string.Empty;
    public string? Patch_Status { get; set; }
    public string? USB_Blocking_Status { get; set; }
    public string? Remarks { get; set; }
}
