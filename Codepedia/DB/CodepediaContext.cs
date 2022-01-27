using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Codepedia.DB
{
    public partial class CodepediaContext : DbContext
    {
        public CodepediaContext()
        {
        }

        public CodepediaContext(DbContextOptions<CodepediaContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<WikiCommit> WikiCommits { get; set; }
        public virtual DbSet<WikiEntry> WikiEntries { get; set; }
        public virtual DbSet<WikiPostSuggestion> WikiPostSuggestions { get; set; }
        public virtual DbSet<WikiSuggestion> WikiSuggestions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseMySQL("server=127.0.0.1;uid=root;pwd=mypass123;database=Codepedia");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.GoogleUserId, "GoogleUserID")
                    .IsUnique();

                entity.HasIndex(e => e.Username, "UserName")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.GoogleUserId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("GoogleUserID");

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasColumnType("enum('User','Admin')");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<WikiCommit>(entity =>
            {
                entity.HasIndex(e => e.ApprovedBy, "ApprovedBy");

                entity.HasIndex(e => e.EntryId, "EntryID");

                entity.HasIndex(e => e.Markdown, "Markdown");

                entity.HasIndex(e => e.SuggestedBy, "SuggestedBy");

                entity.HasIndex(e => e.Words, "Words");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.EntryId).HasColumnName("EntryID");

                entity.Property(e => e.Markdown)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.TimeCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Words).HasColumnType("varchar(5000)");

                entity.HasOne(d => d.ApprovedByNavigation)
                    .WithMany(p => p.WikiCommitApprovedByNavigations)
                    .HasForeignKey(d => d.ApprovedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("WikiCommits_ibfk_4");

                entity.HasOne(d => d.Entry)
                    .WithMany(p => p.WikiCommits)
                    .HasForeignKey(d => d.EntryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("WikiCommits_ibfk_3");

                entity.HasOne(d => d.SuggestedByNavigation)
                    .WithMany(p => p.WikiCommitSuggestedByNavigations)
                    .HasForeignKey(d => d.SuggestedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("WikiCommits_ibfk_5");
            });

            modelBuilder.Entity<WikiEntry>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");
            });

            modelBuilder.Entity<WikiPostSuggestion>(entity =>
            {
                entity.HasIndex(e => e.Status, "Status");

                entity.HasIndex(e => e.SuggestedBy, "SuggestedBy");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Markdown).IsRequired();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnType("enum('Rejected','Unreviewed','Accepted')")
                    .HasDefaultValueSql("'Unreviewed'");

                entity.Property(e => e.TimeCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.SuggestedByNavigation)
                    .WithMany(p => p.WikiPostSuggestions)
                    .HasForeignKey(d => d.SuggestedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("WikiPostSuggestions_ibfk_1");
            });

            modelBuilder.Entity<WikiSuggestion>(entity =>
            {
                entity.HasIndex(e => e.CommitId, "CommitID");

                entity.HasIndex(e => e.Status, "Status");

                entity.HasIndex(e => e.SuggestedBy, "SuggestedBy");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CommitId).HasColumnName("CommitID");

                entity.Property(e => e.Markdown).IsRequired();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnType("enum('Rejected','Unreviewed','Accepted')")
                    .HasDefaultValueSql("'Unreviewed'");

                entity.Property(e => e.TimeCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.Commit)
                    .WithMany(p => p.WikiSuggestions)
                    .HasForeignKey(d => d.CommitId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("WikiSuggestions_ibfk_1");

                entity.HasOne(d => d.SuggestedByNavigation)
                    .WithMany(p => p.WikiSuggestions)
                    .HasForeignKey(d => d.SuggestedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("WikiSuggestions_ibfk_2");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
