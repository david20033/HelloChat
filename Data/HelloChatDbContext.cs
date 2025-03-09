using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HelloChat.Data;

public class HelloChatDbContext : IdentityDbContext<ApplicationUser>
{
    public HelloChatDbContext(DbContextOptions<HelloChatDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<Message> Messages{ get; set; }
    public virtual DbSet<Conversation> Conversation { get; set; }
    public virtual DbSet<FriendRequest> FriendRequest { get; set; }
    public virtual DbSet<Friendship> Friendship { get; set; }
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

                builder.Entity<FriendRequest>()
            .HasOne(fr => fr.Requester)
            .WithMany(u => u.SentRequests)
            .HasForeignKey(fr => fr.RequesterId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.Entity<FriendRequest>()
            .HasOne(fr => fr.Receiver)
            .WithMany(u => u.ReceivedRequests)
            .HasForeignKey(fr => fr.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Friendship>()
            .HasKey(f => new { f.User1Id, f.User2Id });

        builder.Entity<Friendship>()
            .HasOne(f => f.User1)
            .WithMany(u => u.FriendshipsInitiated)
            .HasForeignKey(f => f.User1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Friendship>()
            .HasOne(f => f.User2)
            .WithMany(u => u.FriendshipsReceived)
            .HasForeignKey(f => f.User2Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
