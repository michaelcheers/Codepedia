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

        public virtual DbSet<BlogPost> BlogPosts { get; set; }
        public virtual DbSet<CommitDraft> CommitDrafts { get; set; }
        public virtual DbSet<EntryCommit> EntryCommits { get; set; }
        public virtual DbSet<Hierachy> Hierachies { get; set; }
        public virtual DbSet<HierachyEntry> HierachyEntries { get; set; }
        public virtual DbSet<Inbox> Inboxes { get; set; }
        public virtual DbSet<SuggestionCommit> SuggestionCommits { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<WikiCommit> WikiCommits { get; set; }
        public virtual DbSet<WikiEntry> WikiEntries { get; set; }
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
            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BlogContent).IsRequired();

                entity.Property(e => e.LastUpdated).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.TimeCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<CommitDraft>(entity =>
            {
                entity.HasIndex(e => e.BaseCommitId, "CommitDraft_ibfk_1");

                entity.HasIndex(e => e.Owner, "Owner");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BaseCommitId).HasColumnName("BaseCommitID");

                entity.Property(e => e.HierachyPosition)
                    .IsRequired()
                    .HasColumnType("json");

                entity.Property(e => e.LastUpdated).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Message).HasColumnType("varchar(4096)");

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.Slug).HasMaxLength(256);

                entity.Property(e => e.TimeCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.BaseCommit)
                    .WithMany(p => p.CommitDrafts)
                    .HasForeignKey(d => d.BaseCommitId)
                    .HasConstraintName("CommitDrafts_ibfk_1");

                entity.HasOne(d => d.OwnerNavigation)
                    .WithMany(p => p.CommitDrafts)
                    .HasForeignKey(d => d.Owner)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("CommitDrafts_ibfk_2");
            });

            modelBuilder.Entity<EntryCommit>(entity =>
            {
                entity.HasKey(e => e.CommitId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.ApprovedBy, "ApprovedBy");

                entity.HasIndex(e => e.EntryId, "EntryID");

                entity.Property(e => e.CommitId).HasColumnName("CommitID");

                entity.Property(e => e.EntryId).HasColumnName("EntryID");

                entity.Property(e => e.TimeCommited).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.ApprovedByNavigation)
                    .WithMany(p => p.EntryCommits)
                    .HasForeignKey(d => d.ApprovedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("EntryCommits_ibfk_3");

                entity.HasOne(d => d.Commit)
                    .WithOne(p => p.EntryCommit)
                    .HasForeignKey<EntryCommit>(d => d.CommitId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("EntryCommits_ibfk_1");

                entity.HasOne(d => d.Entry)
                    .WithMany(p => p.EntryCommits)
                    .HasForeignKey(d => d.EntryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("EntryCommits_ibfk_2");
            });

            modelBuilder.Entity<Hierachy>(entity =>
            {
                entity.HasKey(e => e.Child)
                    .HasName("PRIMARY");

                entity.ToTable("Hierachy");

                entity.HasIndex(e => e.Parent, "Parent");

                entity.Property(e => e.Idx).HasColumnName("IDX");

                entity.HasOne(d => d.ChildNavigation)
                    .WithOne(p => p.HierachyChildNavigation)
                    .HasForeignKey<Hierachy>(d => d.Child)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Hierachy_ibfk_2");

                entity.HasOne(d => d.ParentNavigation)
                    .WithMany(p => p.HierachyParentNavigations)
                    .HasForeignKey(d => d.Parent)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Hierachy_ibfk_1");
            });

            modelBuilder.Entity<HierachyEntry>(entity =>
            {
                entity.ToTable("HierachyEntry");

                entity.HasIndex(e => e.EntryId, "EntryID");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.EntryId).HasColumnName("EntryID");

                entity.Property(e => e.Title).HasMaxLength(256);

                entity.HasOne(d => d.Entry)
                    .WithMany(p => p.HierachyEntries)
                    .HasForeignKey(d => d.EntryId)
                    .HasConstraintName("HierachyEntry_ibfk_1");
            });

            modelBuilder.Entity<Inbox>(entity =>
            {
                entity.ToTable("Inbox");

                entity.HasIndex(e => e.User, "User");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.TimeCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.Inboxes)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Inbox_ibfk_1");
            });

            modelBuilder.Entity<SuggestionCommit>(entity =>
            {
                entity.HasKey(e => e.CommitId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.SuggestionId, "SuggestionID");

                entity.HasIndex(e => e.BaseEntryCommitId, "Suggestions_Commits_ibfk_3");

                entity.Property(e => e.CommitId).HasColumnName("CommitID");

                entity.Property(e => e.BaseEntryCommitId).HasColumnName("BaseEntryCommitID");

                entity.Property(e => e.SuggestionId).HasColumnName("SuggestionID");

                entity.HasOne(d => d.BaseEntryCommit)
                    .WithMany(p => p.SuggestionCommits)
                    .HasForeignKey(d => d.BaseEntryCommitId)
                    .HasConstraintName("SuggestionCommits_ibfk_3");

                entity.HasOne(d => d.Commit)
                    .WithOne(p => p.SuggestionCommit)
                    .HasForeignKey<SuggestionCommit>(d => d.CommitId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("SuggestionCommits_ibfk_2");

                entity.HasOne(d => d.Suggestion)
                    .WithMany(p => p.SuggestionCommits)
                    .HasForeignKey(d => d.SuggestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("SuggestionCommits_ibfk_1");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.GoogleUserId, "GoogleUserID")
                    .IsUnique();

                entity.HasIndex(e => e.Username, "UserName")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DisplayName).HasMaxLength(130);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.GoogleUserId)
                    .HasMaxLength(128)
                    .HasColumnName("GoogleUserID");

                entity.Property(e => e.Password)
                    .HasMaxLength(128)
                    .IsFixedLength(true);

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasColumnType("enum('User','Admin')");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<WikiCommit>(entity =>
            {
                entity.HasIndex(e => new { e.Name, e.Markdown, e.Words }, "Name");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Markdown)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.Message).HasColumnType("varchar(4096)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.TimeCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Words).HasColumnType("varchar(5000)");
            });

            modelBuilder.Entity<WikiEntry>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");
            });

            modelBuilder.Entity<WikiSuggestion>(entity =>
            {
                entity.HasIndex(e => e.EntryId, "EntryID");

                entity.HasIndex(e => e.MergingCommitId, "MergingCommitID");

                entity.HasIndex(e => e.Status, "Status");

                entity.HasIndex(e => e.SuggestedBy, "SuggestedBy");

                entity.HasIndex(e => e.UserRejected, "UserRejected");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.EntryId).HasColumnName("EntryID");

                entity.Property(e => e.MergingCommitId).HasColumnName("MergingCommitID");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnType("enum('Retracted','Rejected','Unreviewed','Accepted')")
                    .HasDefaultValueSql("'Unreviewed'");

                entity.HasOne(d => d.Entry)
                    .WithMany(p => p.WikiSuggestions)
                    .HasForeignKey(d => d.EntryId)
                    .HasConstraintName("WikiSuggestions_ibfk_3");

                entity.HasOne(d => d.MergingCommit)
                    .WithMany(p => p.WikiSuggestions)
                    .HasForeignKey(d => d.MergingCommitId)
                    .HasConstraintName("WikiSuggestions_ibfk_4");

                entity.HasOne(d => d.SuggestedByNavigation)
                    .WithMany(p => p.WikiSuggestionSuggestedByNavigations)
                    .HasForeignKey(d => d.SuggestedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("WikiSuggestions_ibfk_2");

                entity.HasOne(d => d.UserRejectedNavigation)
                    .WithMany(p => p.WikiSuggestionUserRejectedNavigations)
                    .HasForeignKey(d => d.UserRejected)
                    .HasConstraintName("WikiSuggestions_ibfk_5");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
