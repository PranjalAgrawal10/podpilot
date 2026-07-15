using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>EF configuration for enterprise audit events.</summary>
public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("AuditEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActorEmail).HasMaxLength(256);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EntityType).HasMaxLength(100);
        builder.Property(x => x.EntityId).HasMaxLength(100);
        builder.Property(x => x.Summary).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.MetadataJson).HasColumnType("longtext");
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.HasIndex(x => new { x.OrganizationId, x.OccurredAt });
        builder.HasIndex(x => x.EventType);
    }
}

/// <summary>EF configuration for identity providers.</summary>
public class IdentityProviderConfiguration : IEntityTypeConfiguration<IdentityProvider>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<IdentityProvider> builder)
    {
        builder.ToTable("IdentityProviders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ClientId).HasMaxLength(500);
        builder.Property(x => x.EncryptedClientSecret).HasColumnType("longtext");
        builder.Property(x => x.Issuer).HasMaxLength(500);
        builder.Property(x => x.AuthorizationEndpoint).HasMaxLength(500);
        builder.Property(x => x.TokenEndpoint).HasMaxLength(500);
        builder.Property(x => x.JwksUri).HasMaxLength(500);
        builder.Property(x => x.SamlEntityId).HasMaxLength(500);
        builder.Property(x => x.SamlSsoUrl).HasMaxLength(500);
        builder.Property(x => x.EncryptedSamlCertificate).HasColumnType("longtext");
        builder.Property(x => x.Scopes).IsRequired().HasMaxLength(500);
        builder.Property(x => x.CallbackPath).HasMaxLength(200);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique();
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF configuration for SCIM mappings.</summary>
public class ScimMappingConfiguration : IEntityTypeConfiguration<ScimMapping>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ScimMapping> builder)
    {
        builder.ToTable("ScimMappings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalGroupId).IsRequired().HasMaxLength(300);
        builder.Property(x => x.ExternalGroupName).HasMaxLength(300);
        builder.Property(x => x.OrganizationRole).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.OrganizationId, x.ExternalGroupId }).IsUnique();
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.IdentityProvider)
            .WithMany()
            .HasForeignKey(x => x.IdentityProviderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>EF configuration for secret references.</summary>
public class SecretReferenceConfiguration : IEntityTypeConfiguration<SecretReference>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SecretReference> builder)
    {
        builder.ToTable("SecretReferences");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.BackendLocator).IsRequired().HasMaxLength(500);
        builder.Property(x => x.EncryptedValue).HasColumnType("longtext");
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique();
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF configuration for compliance events.</summary>
public class ComplianceEventConfiguration : IEntityTypeConfiguration<ComplianceEvent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ComplianceEvent> builder)
    {
        builder.ToTable("ComplianceEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Details).HasMaxLength(2000);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.OrganizationId, x.OccurredAt });
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF configuration for session history.</summary>
public class SessionHistoryConfiguration : IEntityTypeConfiguration<SessionHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SessionHistory> builder)
    {
        builder.ToTable("SessionHistory");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SessionId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.DeviceFingerprint).HasMaxLength(200);
        builder.Property(x => x.CountryCode).HasMaxLength(8);
        builder.Property(x => x.FailureReason).HasMaxLength(200);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.OrganizationId, x.UserId, x.IsActive });
        builder.HasIndex(x => x.SessionId);
    }
}

/// <summary>EF configuration for trusted devices.</summary>
public class TrustedDeviceConfiguration : IEntityTypeConfiguration<TrustedDevice>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TrustedDevice> builder)
    {
        builder.ToTable("TrustedDevices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DeviceName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.FingerprintHash).IsRequired().HasMaxLength(200);
        builder.Property(x => x.LastIpAddress).HasMaxLength(64);
        builder.Property(x => x.LastUserAgent).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.UserId, x.FingerprintHash });
    }
}

/// <summary>EF configuration for organization security policies.</summary>
public class OrganizationSecurityPolicyConfiguration : IEntityTypeConfiguration<OrganizationSecurityPolicy>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrganizationSecurityPolicy> builder)
    {
        builder.ToTable("OrganizationSecurityPolicies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.IpAllowListJson).IsRequired().HasColumnType("longtext");
        builder.Property(x => x.GeoAllowListJson).IsRequired().HasColumnType("longtext");
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => x.OrganizationId).IsUnique();
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF configuration for organization governance policies.</summary>
public class OrganizationGovernancePolicyConfiguration : IEntityTypeConfiguration<OrganizationGovernancePolicy>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrganizationGovernancePolicy> builder)
    {
        builder.ToTable("OrganizationPolicies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AllowedProvidersJson).IsRequired().HasColumnType("longtext");
        builder.Property(x => x.AllowedModelsJson).IsRequired().HasColumnType("longtext");
        builder.Property(x => x.AllowedPluginsJson).IsRequired().HasColumnType("longtext");
        builder.Property(x => x.AllowedMcpServersJson).IsRequired().HasColumnType("longtext");
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => x.OrganizationId).IsUnique();
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF configuration for organization compliance settings.</summary>
public class OrganizationComplianceSettingsConfiguration : IEntityTypeConfiguration<OrganizationComplianceSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrganizationComplianceSettings> builder)
    {
        builder.ToTable("OrganizationComplianceSettings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => x.OrganizationId).IsUnique();
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF configuration for user MFA enrollments.</summary>
public class UserMfaEnrollmentConfiguration : IEntityTypeConfiguration<UserMfaEnrollment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserMfaEnrollment> builder)
    {
        builder.ToTable("UserMfaEnrollments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EncryptedTotpSecret).HasColumnType("longtext");
        builder.Property(x => x.EncryptedRecoveryCodesJson).HasColumnType("longtext");
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}
