using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace XlsToJsonConverter
{
    public partial class MpeiMessagesContext : DbContext
    {
        public MpeiMessagesContext()
        {
        }

        public MpeiMessagesContext(DbContextOptions<MpeiMessagesContext> options)
            : base(options)
        {
        }

        public virtual DbSet<DbMessage> MsgsMessage { get; set; }
        public virtual DbSet<MsgsMessageTo> MsgsMessageTo { get; set; }
        public virtual DbSet<StudentInfo> MsgsStudentinfo { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=mpei_messages;Username=postgres;Password=adminadmin");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbMessage>(entity =>
            {
                entity.ToTable("msgs_message");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Body)
                    .IsRequired()
                    .HasColumnName("body");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Sender)
                    .IsRequired()
                    .HasColumnName("sender")
                    .HasMaxLength(254);

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasColumnName("subject")
                    .HasMaxLength(255);

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("timestamp with time zone");
            });

            modelBuilder.Entity<MsgsMessageTo>(entity =>
            {
                entity.ToTable("msgs_message_to");

                entity.HasIndex(e => e.MessageId)
                    .HasName("msgs_message_to_message_id_ba369f13");

                entity.HasIndex(e => e.StudentinfoId)
                    .HasName("msgs_message_to_studentinfo_id_b8f1e31e");

                entity.HasIndex(e => new { e.MessageId, e.StudentinfoId })
                    .HasName("msgs_message_to_message_id_studentinfo_id_8944a98a_uniq")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.MessageId).HasColumnName("message_id");

                entity.Property(e => e.StudentinfoId).HasColumnName("studentinfo_id");

                entity.HasOne(d => d.Message)
                    .WithMany(p => p.MsgsMessageTo)
                    .HasForeignKey(d => d.MessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("msgs_message_to_message_id_ba369f13_fk_msgs_message_id");

                entity.HasOne(d => d.Studentinfo)
                    .WithMany(p => p.MsgsMessageTo)
                    .HasForeignKey(d => d.StudentinfoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("msgs_message_to_studentinfo_id_b8f1e31e_fk_msgs_studentinfo_id");
            });

            modelBuilder.Entity<StudentInfo>(entity =>
            {
                entity.ToTable("msgs_studentinfo");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(254);

                entity.Property(e => e.Group)
                    .IsRequired()
                    .HasColumnName("group")
                    .HasMaxLength(255);

                entity.Property(e => e.Institute)
                    .IsRequired()
                    .HasColumnName("institute")
                    .HasMaxLength(255);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255);

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasColumnName("phone")
                    .HasMaxLength(128);

                entity.Property(e => e.Telegram)
                    .IsRequired()
                    .HasColumnName("telegram")
                    .HasMaxLength(255);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Vk)
                    .IsRequired()
                    .HasColumnName("vk")
                    .HasMaxLength(255);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
