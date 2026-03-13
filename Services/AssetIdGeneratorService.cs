using ITAMS.Data;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Services;

public interface IAssetIdGeneratorService
{
    Task<string> GenerateHardwareAssetIdAsync();
    Task<string> GenerateSoftwareAssetIdAsync();
}

public class AssetIdGeneratorService : IAssetIdGeneratorService
{
    private readonly ITAMSDbContext _context;
    private readonly ILogger<AssetIdGeneratorService> _logger;

    public AssetIdGeneratorService(ITAMSDbContext context, ILogger<AssetIdGeneratorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> GenerateHardwareAssetIdAsync()
    {
        try
        {
            // Get the highest existing hardware asset ID
            var lastAsset = await _context.Assets
                .Where(a => a.AssetId.StartsWith("ASTH"))
                .OrderByDescending(a => a.AssetId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastAsset != null)
            {
                // Extract the number from the last asset ID (e.g., "ASTH00001" -> 1)
                if (int.TryParse(lastAsset.AssetId.Substring(4), out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            string newAssetId = $"ASTH{nextNumber:D5}";
            _logger.LogInformation("Generated hardware asset ID: {AssetId}", newAssetId);
            return newAssetId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating hardware asset ID");
            throw;
        }
    }

    public async Task<string> GenerateSoftwareAssetIdAsync()
    {
        try
        {
            // Get the highest existing software asset ID
            var lastAsset = await _context.SoftwareAssets
                .Where(a => a.AssetId.StartsWith("ASTS"))
                .OrderByDescending(a => a.AssetId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastAsset != null)
            {
                // Extract the number from the last asset ID (e.g., "ASTS00001" -> 1)
                if (int.TryParse(lastAsset.AssetId.Substring(4), out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            string newAssetId = $"ASTS{nextNumber:D5}";
            _logger.LogInformation("Generated software asset ID: {AssetId}", newAssetId);
            return newAssetId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating software asset ID");
            throw;
        }
    }
}
