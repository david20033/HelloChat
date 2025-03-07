using System.Reflection.Emit;
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
    public virtual DbSet<Conversation> Conversations { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Message>()
            .Property(m => m.Reaction)
        .HasConversion<int>();

        builder.Entity<Message>()
            .HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Message>()
            .HasOne(m => m.From_User)
            .WithMany()
            .HasForeignKey(m => m.From_id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Message>()
            .HasOne(m => m.To_User)
            .WithMany()
            .HasForeignKey(m => m.To_id)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Entity<Conversation>()
            .HasOne(c => c.User1)
            .WithMany() 
            .HasForeignKey(c => c.User1Id)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.Entity<Conversation>()
            .HasOne(c => c.User2)
            .WithMany() 
            .HasForeignKey(c => c.User2Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
