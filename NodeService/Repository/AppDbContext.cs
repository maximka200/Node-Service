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
        return Users.AsNoTracking().FirstOrDefault(u => u.UserName == username);
    }

    public bool CreateUser(string username, string password, Roles role)
    {
        if (Users.Any(u => u.UserName == username))
            return false;

        Users.Add(new User
        {
            Id = Guid.NewGuid(),
            UserName = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role
        });
        SaveChanges();
        return true;
    }

    public Node? GetNode(Guid id)
    {
        return Nodes.Include(n => n.Children).FirstOrDefault(n => n.Id == id);
    }

    public IEnumerable<Node> GetTree()
    {
        return Nodes.AsNoTracking().ToList();
    }

    public Node CreateNode(string name, Guid? parentId)
    {
        if (parentId.HasValue)
        {
            var parent = Nodes.Find(parentId.Value);
            if (parent == null)
                throw new Exception("Parent not found");
        }

        var node = new Node
        {
            Id = Guid.NewGuid(),
            Name = name,
            ParentId = parentId
        };
        Nodes.Add(node);
        SaveChanges();
        return node;
    }

    public bool UpdateNode(Guid id, string name, Guid? parentId)
    {
        var node = Nodes.Include(n => n.Children).FirstOrDefault(n => n.Id == id);
        if (node == null) return false;

        if (parentId.HasValue && IsCycle(node.Id, parentId.Value))
            return false;

        node.Name = name;
        node.ParentId = parentId;

        using var tx = Database.BeginTransaction();
        try
        {
            Nodes.Update(node);
            SaveChanges();
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            return false;
        }
        return true;
    }

    public bool DeleteNode(Guid id)
    {
        var node = Nodes.Include(n => n.Children).FirstOrDefault(n => n.Id == id);
        if (node == null) return false;

        using var tx = Database.BeginTransaction();
        try
        {
            DeleteNodeRecursive(node);
            SaveChanges();
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            return false;
        }
        return true;
    }

    private void DeleteNodeRecursive(Node node)
    {
        foreach (var child in Nodes.Where(c => c.ParentId == node.Id).ToList())
        {
            DeleteNodeRecursive(child);
        }
        Nodes.Remove(node);
    }

    private bool IsCycle(Guid nodeId, Guid newParentId)
    {
        if (nodeId == newParentId) return true;
        var parent = Nodes.Find(newParentId);
        while (parent != null)
        {
            if (parent.Id == nodeId) return true;
            if (parent.ParentId == null) break;
            parent = Nodes.Find(parent.ParentId);
        }
        return false;
    }
}
