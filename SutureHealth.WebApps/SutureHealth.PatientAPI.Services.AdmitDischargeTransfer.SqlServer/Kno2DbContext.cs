using Microsoft.EntityFrameworkCore;

namespace SutureHealth.Patients.Services.AdmitDischargeTransfer
{
    public partial class Kno2DbContext : DbContext
    {
        public Kno2DbContext()
        {
        }

        public Kno2DbContext(DbContextOptions<Kno2DbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ADT.Kno2.AdditionalNotification> AdditionalNotifications { get; set; }
        public virtual DbSet<ADT.Kno2.Attachment> Attachments { get; set; }
        public virtual DbSet<ADT.Kno2.AttachmentMeta> AttachmentMeta { get; set; }
        public virtual DbSet<ADT.Kno2.Classification> Classifications { get; set; }
        public virtual DbSet<ADT.Kno2.Conversation> Conversations { get; set; }
        public virtual DbSet<ADT.Kno2.IntegrationLog> IntegrationLogs { get; set; }
        public virtual DbSet<ADT.Kno2.Message> Messages { get; set; }
        public virtual DbSet<ADT.Kno2.Patient> Patients { get; set; }
        public virtual DbSet<ADT.Kno2.Signer> Signers { get; set; }
        public virtual DbSet<ADT.Kno2.Tab> Tabs { get; set; }
        public virtual DbSet<ADT.Kno2.Telecom> Telecoms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ADT.Kno2.AdditionalNotification>(entity =>
            {
                entity.ToTable("AdditionalNotification", "kno2");

                entity.HasIndex(e => e.ObfuscatedId, "UK_AdditionalNotification_ObfuscatedId")
                    .IsUnique();

                entity.Property(e => e.NotificationType)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ObfuscatedId)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Value).HasMaxLength(1000);

                entity.HasOne(d => d.Signer)
                    .WithMany(p => p.AdditionalNotifications)
                    .HasForeignKey(d => d.SignerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AdditionalNotification_Signer");
            });

            modelBuilder.Entity<ADT.Kno2.Attachment>(entity =>
            {
                entity.ToTable("Attachment", "kno2");

                entity.HasIndex(e => e.ObfuscatedId, "UK_Attachment_ObfuscatedId")
                    .IsUnique();

                entity.Property(e => e.DocumentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FileName).HasMaxLength(1000);

                entity.Property(e => e.Key)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.MimeType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ObfuscatedId)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ObfuscatedMessageId)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.PreviewAvailable)
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.PreviewKey)
                    .HasMaxLength(48)
                    .IsUnicode(false);

                entity.Property(e => e.TransformStatus)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.Message)
                    .WithMany(p => p.Attachments)
                    .HasForeignKey(d => d.MessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Attachment_Message");
            });

            modelBuilder.Entity<ADT.Kno2.AttachmentMeta>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("AttachmentMeta", "kno2");

                entity.HasIndex(e => e.AttachmentId, "UK_AttachmentMeta_AttachmentId")
                    .IsUnique();

                entity.Property(e => e.Confidentiality)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.DocumentDate).HasPrecision(2);

                entity.Property(e => e.DocumentDescription).HasMaxLength(1000);

                entity.Property(e => e.DocumentTitle).HasMaxLength(1000);

                entity.Property(e => e.DocumentType).HasMaxLength(50);

                entity.HasOne(d => d.Attachment)
                    .WithOne()
                    .HasForeignKey<ADT.Kno2.AttachmentMeta>(d => d.AttachmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AttachmentMeta_Attachment");

                entity.HasOne(d => d.Patient)
                    .WithMany()
                    .HasForeignKey(d => d.PatientId)
                    .HasConstraintName("FK_AttachmentMeta_Patient");
            });

            modelBuilder.Entity<ADT.Kno2.Classification>(entity =>
            {
                entity.ToTable("Classification", "kno2");

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Scheme)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ADT.Kno2.Conversation>(entity =>
            {
                entity.ToTable("Conversation", "kno2");

                entity.HasIndex(e => e.ObfuscatedId, "UK_Conversation_ObfuscatedId")
                    .IsUnique();

                entity.Property(e => e.ConversationStatus)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ObfuscatedId)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Type).HasMaxLength(50);
            });

            modelBuilder.Entity<ADT.Kno2.IntegrationLog>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("IntegrationLog", "kno2");

                entity.Property(e => e.Date).HasPrecision(2);

                entity.Property(e => e.Message).HasMaxLength(4000);

                entity.HasOne(d => d.MessageNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.MessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_IntegrationLog_Message");
            });

            modelBuilder.Entity<ADT.Kno2.Message>(entity =>
            {
                entity.ToTable("Message", "kno2");

                entity.HasIndex(e => e.ObfuscatedId, "UK_Message_ObfuscatedId")
                    .IsUnique();

                entity.Property(e => e.AttachmentSendType)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Attachments2Hl7).HasColumnName("Attachments2HL7");

                entity.Property(e => e.Body).HasMaxLength(4000);

                entity.Property(e => e.ChannelId)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasPrecision(2);

                entity.Property(e => e.FromAddress)
                    .HasMaxLength(320)
                    .IsUnicode(false);

                entity.Property(e => e.HispMessageIds).IsUnicode(false);

                entity.Property(e => e.MessageDate).HasPrecision(2);

                entity.Property(e => e.MessageType)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ObfuscatedId)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.OrganizationId)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Origin).HasMaxLength(50);

                entity.Property(e => e.OriginalObjectId)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.PatientName).HasMaxLength(100);

                entity.Property(e => e.Priority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ProcessTypes).IsUnicode(false);

                entity.Property(e => e.ProcessedType)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ReasonForDisclosure).HasMaxLength(1000);

                entity.Property(e => e.ReleaseTypeId)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.SourceType)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Subject).HasMaxLength(1000);

                entity.Property(e => e.ToAddress)
                    .HasMaxLength(320)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UnprocessedNotificationSent).HasPrecision(2);

                entity.Property(e => e.UpdatedDate).HasPrecision(2);

                entity.HasOne(d => d.Classification)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.ClassificationId)
                    .HasConstraintName("FK_Message_Classification");

                entity.HasOne(d => d.Conversation)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.ConversationId)
                    .HasConstraintName("FK_Message_Conversation");

                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.PatientId)
                    .HasConstraintName("FK_Message_Patient");
            });

            modelBuilder.Entity<ADT.Kno2.Patient>(entity =>
            {
                entity.ToTable("Patient", "kno2");

                entity.HasIndex(e => e.ObfuscatedId, "UK_Patient_ObfuscatedId")
                    .IsUnique();

                entity.Property(e => e.BirthDate).HasColumnType("date");

                entity.Property(e => e.City).HasMaxLength(100);

                entity.Property(e => e.Country).HasMaxLength(52);

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.FullName).HasMaxLength(160);

                entity.Property(e => e.Gender)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Issuer).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.MiddleName).HasMaxLength(50);

                entity.Property(e => e.ObfuscatedId)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ObfuscatedPatientIdRoot)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PatientIds).IsUnicode(false);

                entity.Property(e => e.PostalCode).HasMaxLength(10);

                entity.Property(e => e.State).HasMaxLength(100);

                entity.Property(e => e.StreetAddress1).HasMaxLength(100);

                entity.Property(e => e.StreetAddress2).HasMaxLength(50);

                entity.Property(e => e.Suffix).HasMaxLength(10);

                entity.Property(e => e.Telephone)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.VisitDate).HasPrecision(2);

                entity.Property(e => e.VisitId).HasMaxLength(50);

                entity.Property(e => e.Zip)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ADT.Kno2.Signer>(entity =>
            {
                entity.ToTable("Signer", "kno2");

                entity.HasIndex(e => e.ObfuscatedId, "UK_Signer_ObfuscatedId")
                    .IsUnique();

                entity.Property(e => e.CreatedDate).HasPrecision(2);

                entity.Property(e => e.Email)
                    .HasMaxLength(320)
                    .IsUnicode(false);

                entity.Property(e => e.HostEmail)
                    .HasMaxLength(320)
                    .IsUnicode(false);

                entity.Property(e => e.HostName)
                    .HasMaxLength(253)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.ObfuscatedId)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(31)
                    .IsUnicode(false);

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SignerType)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasPrecision(2);

                entity.HasOne(d => d.Message)
                    .WithMany(p => p.Signers)
                    .HasForeignKey(d => d.MessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Signer_Message");
            });

            modelBuilder.Entity<ADT.Kno2.Tab>(entity =>
            {
                entity.ToTable("Tab", "kno2");

                entity.Property(e => e.Label).HasMaxLength(50);

                entity.Property(e => e.ObfuscatedId)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Value).HasMaxLength(50);

                entity.HasOne(d => d.Signer)
                    .WithMany(p => p.Tabs)
                    .HasForeignKey(d => d.SignerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tab_Signer");
            });

            modelBuilder.Entity<ADT.Kno2.Telecom>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Telecom", "kno2");

                entity.Property(e => e.System)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Use)
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(320)
                    .IsUnicode(false);

                entity.HasOne(d => d.Patient)
                    .WithMany()
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Telecom_Patient");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
