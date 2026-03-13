using DCData.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCData.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("users");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
        entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
        entity.Property(e => e.Nickname).HasMaxLength(50).IsRequired();
        entity.Property(e => e.Level).IsRequired();
        entity.Property(e => e.Gold).IsRequired();
        entity.Property(e => e.Stamina).IsRequired();
        entity.Property(e => e.CurrentDungeonId);
        entity.Property(e => e.CurrentFloor);
        entity.Property(e => e.LastLoginAt).HasColumnType("datetime(6)");
        entity.Property(e => e.CreatedAt).HasColumnType("datetime(6)");
        entity.Property(e => e.UpdatedAt).HasColumnType("datetime(6)");
    }
}
