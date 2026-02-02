using Microsoft.EntityFrameworkCore;
using NodeService.Models;

namespace NodeService.Repository;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Node> Nodes => Set<Node>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        modelBuilder.Entity<Node>(entity =>
        {
            entity.HasKey(n => n.Id);

            entity.Property(n => n.Name)
                .IsRequired()
                .HasMaxLength(256);
            
            entity
                .HasOne(n => n.Parent)
                .WithMany(n => n.Children)
                .HasForeignKey(n => n.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public User? GetUser(string username)
    {
        return Users.AsNoTracking()
            .FirstOrDefault(u => u.UserName == username);
    }
    
    public bool CreateUser(string username, string password, Roles role)
    {
        var exists = Users.Any(u => u.UserName == username);
        if (exists)
            return false;

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role
        };

        Users.Add(user);
        SaveChanges();
        return true;
    }
}