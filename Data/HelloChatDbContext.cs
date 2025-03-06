using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HelloChat.Data;

public class HelloChatDbContext : IdentityDbContext<IdentityUser>
{
    public HelloChatDbContext(DbContextOptions<HelloChatDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<Message> Messages { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Message>()
            .Property(m => m.Reaction)
            .HasConversion<int>();
    }
}
