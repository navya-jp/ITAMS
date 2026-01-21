using Microsoft.EntityFrameworkCore;
using ITAMS.Domain.Entities;

namespace ITAMS.Data;

public class ITAMSDbContext : DbContext
{
    public ITAMSDbContext(DbContextOptions<ITAMSDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<UserProject> UserProjects { get; set; }
    public DbSet<UserProjectPermission> UserProjectPermissions { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AuditEntry> AuditEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
                
            entity.Property(e => e.PasswordHash)
                .IsRequired();
                
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Role entity configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
        });

        // Permission entity configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Module)
                .IsRequired()
                .HasMaxLength(50);
        });

        // RolePermission entity configuration
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
            
            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Project entity configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);
        });

        // Location entity configuration
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Region)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.State)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Locations)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserProject entity configuration
        modelBuilder.Entity<UserProject>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ProjectId }).IsUnique();
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserProjects)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Project)
                .WithMany(p => p.UserProjects)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserProjectPermission entity configuration
        modelBuilder.Entity<UserProjectPermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserProjectId, e.PermissionId }).IsUnique();
            
            entity.HasOne(e => e.UserProject)
                .WithMany(up => up.UserProjectPermissions)
                .HasForeignKey(e => e.UserProjectId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.UserProjectPermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Asset entity configuration
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AssetTag).IsUnique();
            
            entity.Property(e => e.AssetTag)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.Property(e => e.AssetType)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Make)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Model)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.ProcurementCost)
                .HasPrecision(18, 2);
                
            entity.Property(e => e.UsageCategory)
                .IsRequired()
                .HasConversion<int>();
                
            entity.Property(e => e.Criticality)
                .IsRequired()
                .HasConversion<int>();
                
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();
                
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Assets)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Location)
                .WithMany(l => l.Assets)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.AssignedUser)
                .WithMany(u => u.AssignedAssets)
                .HasForeignKey(e => e.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // AuditEntry entity configuration
        modelBuilder.Entity<AuditEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.HasOne(e => e.User)
                .WithMany(u => u.AuditEntries)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed default data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Use static date for seed data
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        // Create default roles
        var superAdminRole = new Role { Id = 1, Name = "Super Admin", Description = "Full system access", IsSystemRole = true, CreatedAt = seedDate };
        var adminRole = new Role { Id = 2, Name = "Admin", Description = "Project administration access", IsSystemRole = true, CreatedAt = seedDate };
        var itStaffRole = new Role { Id = 3, Name = "IT Staff", Description = "Asset management access", IsSystemRole = true, CreatedAt = seedDate };
        var readOnlyRole = new Role { Id = 4, Name = "Read Only User", Description = "View-only access", IsSystemRole = true, CreatedAt = seedDate };
        var auditorRole = new Role { Id = 5, Name = "Auditor", Description = "Audit and reporting access", IsSystemRole = true, CreatedAt = seedDate };

        modelBuilder.Entity<Role>().HasData(superAdminRole, adminRole, itStaffRole, readOnlyRole, auditorRole);

        // Create default permissions
        var permissions = new[]
        {
            // User Management
            new Permission { Id = 1, Name = "View Users", Code = "USERS_VIEW", Module = "Users", Description = "View user list and details" },
            new Permission { Id = 2, Name = "Create Users", Code = "USERS_CREATE", Module = "Users", Description = "Create new users" },
            new Permission { Id = 3, Name = "Edit Users", Code = "USERS_EDIT", Module = "Users", Description = "Edit user details" },
            new Permission { Id = 4, Name = "Delete Users", Code = "USERS_DELETE", Module = "Users", Description = "Delete users" },
            new Permission { Id = 5, Name = "Manage User Permissions", Code = "USERS_PERMISSIONS", Module = "Users", Description = "Assign permissions to users" },
            
            // Project Management
            new Permission { Id = 6, Name = "View Projects", Code = "PROJECTS_VIEW", Module = "Projects", Description = "View project list and details" },
            new Permission { Id = 7, Name = "Create Projects", Code = "PROJECTS_CREATE", Module = "Projects", Description = "Create new projects" },
            new Permission { Id = 8, Name = "Edit Projects", Code = "PROJECTS_EDIT", Module = "Projects", Description = "Edit project details" },
            new Permission { Id = 9, Name = "Delete Projects", Code = "PROJECTS_DELETE", Module = "Projects", Description = "Delete projects" },
            
            // Location Management
            new Permission { Id = 10, Name = "View Locations", Code = "LOCATIONS_VIEW", Module = "Locations", Description = "View location list and details" },
            new Permission { Id = 11, Name = "Create Locations", Code = "LOCATIONS_CREATE", Module = "Locations", Description = "Create new locations" },
            new Permission { Id = 12, Name = "Edit Locations", Code = "LOCATIONS_EDIT", Module = "Locations", Description = "Edit location details" },
            new Permission { Id = 13, Name = "Delete Locations", Code = "LOCATIONS_DELETE", Module = "Locations", Description = "Delete locations" },
            
            // Asset Management
            new Permission { Id = 14, Name = "View Assets", Code = "ASSETS_VIEW", Module = "Assets", Description = "View asset list and details" },
            new Permission { Id = 15, Name = "Create Assets", Code = "ASSETS_CREATE", Module = "Assets", Description = "Create new assets" },
            new Permission { Id = 16, Name = "Edit Assets", Code = "ASSETS_EDIT", Module = "Assets", Description = "Edit asset details" },
            new Permission { Id = 17, Name = "Delete Assets", Code = "ASSETS_DELETE", Module = "Assets", Description = "Delete assets" },
            new Permission { Id = 18, Name = "Transfer Assets", Code = "ASSETS_TRANSFER", Module = "Assets", Description = "Transfer assets between locations" },
            
            // Reports
            new Permission { Id = 19, Name = "View Reports", Code = "REPORTS_VIEW", Module = "Reports", Description = "View system reports" },
            new Permission { Id = 20, Name = "Export Reports", Code = "REPORTS_EXPORT", Module = "Reports", Description = "Export reports to files" },
            
            // Settings
            new Permission { Id = 21, Name = "System Settings", Code = "SETTINGS_SYSTEM", Module = "Settings", Description = "Manage system settings" },
            new Permission { Id = 22, Name = "Role Management", Code = "SETTINGS_ROLES", Module = "Settings", Description = "Manage roles and permissions" },
            
            // Audit
            new Permission { Id = 23, Name = "View Audit Trail", Code = "AUDIT_VIEW", Module = "Audit", Description = "View audit trail and logs" }
        };

        modelBuilder.Entity<Permission>().HasData(permissions);

        // Assign all permissions to Super Admin role
        var superAdminPermissions = permissions.Select((p, index) => new RolePermission
        {
            Id = index + 1,
            RoleId = 1, // Super Admin
            PermissionId = p.Id,
            IsGranted = true,
            CreatedAt = seedDate
        }).ToArray();

        modelBuilder.Entity<RolePermission>().HasData(superAdminPermissions);

        // Create default Super Admin user
        var superAdmin = new User
        {
            Id = 1,
            Username = "superadmin",
            Email = "superadmin@itams.com",
            FirstName = "Super",
            LastName = "Admin",
            RoleId = 1, // Super Admin role
            IsActive = true,
            CreatedAt = seedDate,
            MustChangePassword = true,
            // Default password: "Admin@123" - should be changed on first login
            // Using a static hash for seed data consistency
            PasswordHash = "$2a$11$8K1p/a0dL2LkqvMA87LzO.Mh5vllpKmdxX6NYb2UV8mWLxYX4H1Ka" // Admin@123
        };

        modelBuilder.Entity<User>().HasData(superAdmin);
    }
}