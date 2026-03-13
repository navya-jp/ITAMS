using ITAMS.Data;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Services;

public interface IAssetIdGeneratorService
{
    Task<string> GenerateHardwareAssetIdAsync();
    Task<string> GenerateLicensingAssetIdAsync();
    Task<string> GenerateServiceAssetIdAsync();
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

    public async Task<string> GenerateLicensingAssetIdAsync()
    {
        try
        {
            // Get the highest existing licensing asset ID
            var lastAsset = await _context.LicensingAssets
                .Where(a => a.AssetId.StartsWith("ASTL"))
                .OrderByDescending(a => a.AssetId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastAsset != null)
            {
                // Extract the number from the last asset ID (e.g., "ASTL00001" -> 1)
                if (int.TryParse(lastAsset.AssetId.Substring(4), out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            string newAssetId = $"ASTL{nextNumber:D5}";
            _logger.LogInformation("Generated licensing asset ID: {AssetId}", newAssetId);
            return newAssetId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating licensing asset ID");
            throw;
        }
    }

    public async Task<string> GenerateServiceAssetIdAsync()
    {
        try
        {
            // Get the highest existing service asset ID
            var lastAsset = await _context.ServiceAssets
                .Where(a => a.AssetId.StartsWith("ASTV"))
                .OrderByDescending(a => a.AssetId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastAsset != null)
            {
                // Extract the number from the last asset ID (e.g., "ASTV00001" -> 1)
                if (int.TryParse(lastAsset.AssetId.Substring(4), out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            string newAssetId = $"ASTV{nextNumber:D5}";
            _logger.LogInformation("Generated service asset ID: {AssetId}", newAssetId);
            return newAssetId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating service asset ID");
            throw;
        }
    }
}
