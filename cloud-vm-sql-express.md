# Cloud VM with SQL Server Express

## Setup Virtual Machine in Cloud

### Option A: Azure VM (Recommended)
1. **Create Azure Account** (free $200 credits)
2. **Create Windows VM**:
   - Size: B1s (1 vCPU, 1 GB RAM) - ~$15/month
   - OS: Windows Server 2022
   - Allow RDP (port 3389)

3. **Install SQL Server Express on VM**:
   ```powershell
   # RDP into the VM
   # Download and install SQL Server Express
   # Run setup-sql-express-server.ps1
   ```

4. **Configure VM Networking**:
   - Open port 1433 in Azure Network Security Group
   - Configure Windows Firewall on VM

5. **Team Connection**:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR-VM-IP\\SQLEXPRESS;Database=ITAMSSharedDB;User Id=sa;Password=YourStrongPassword;TrustServerCertificate=true;Encrypt=false;"
     }
   }
   ```

### Option B: AWS EC2 with SQL Express
1. **Create AWS Account** (free tier available)
2. **Launch Windows EC2 instance**:
   - Instance type: t3.micro (free tier eligible)
   - Windows Server 2022
   - Security group: Allow RDP (3389) and SQL (1433)

3. **Install SQL Server Express**
4. **Configure for remote access**

### Option C: Google Cloud VM
1. **Create Google Cloud account** ($300 free credits)
2. **Create Windows VM instance**
3. **Install and configure SQL Server Express**

## Benefits:
✅ Uses SQL Server Express as required
✅ Always available (24/7)
✅ Professional hosting
✅ Team independent access
✅ Automatic VM backups

## Costs:
- Azure B1s: ~$15/month
- AWS t3.micro: Free tier (1 year), then ~$10/month
- Google Cloud: ~$12/month

## Setup Time: 30-45 minutes