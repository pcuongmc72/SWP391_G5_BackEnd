using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SWP.DAL.Models;

namespace SWP.DAL.Context;

public partial class FlippedClassroomContext : DbContext
{
    public FlippedClassroomContext()
    {
    }

    public FlippedClassroomContext(DbContextOptions<FlippedClassroomContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcademicTerm> AcademicTerms { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassStudent> ClassStudents { get; set; }

    public virtual DbSet<ClassSession> ClassSessions { get; set; }

    public virtual DbSet<LearningMaterial> LearningMaterials { get; set; }

    public virtual DbSet<MaterialCompletion> MaterialCompletions { get; set; }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<SupportFeedback> SupportFeedbacks { get; set; }

    public virtual DbSet<DiscussionThread> DiscussionThreads { get; set; }

    public virtual DbSet<DiscussionReply> DiscussionReplies { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Chỉ dùng connection string mặc định khi chưa được cấu hình qua DI (appsettings.json)
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("server=.,1433;database=FlippedClassroom;uid=sa;pwd=123;TrustServerCertificate=true");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcademicTerm>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.Property(e => e.Id)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.LecturerId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.AcademicTerm).WithMany(p => p.Classes)
                .HasForeignKey(d => d.AcademicTermId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classes_Term");

            entity.HasOne(d => d.Course).WithMany(p => p.Classes)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classes_Course");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.Classes)
                .HasForeignKey(d => d.LecturerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classes_Lecturer");
        });

        modelBuilder.Entity<ClassSession>(entity =>
        {
            entity.HasIndex(e => new { e.ClassId, e.SessionDate }, "IX_ClassSessions_Class_Date");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.ClassId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Room).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Class).WithMany(p => p.ClassSessions)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ClassSessions_Class");
        });

        modelBuilder.Entity<ClassStudent>(entity =>
        {
            entity.HasKey(e => new { e.ClassId, e.StudentId });

            entity.Property(e => e.ClassId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.StudentId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.EnrolledAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ClassRole).HasMaxLength(20);

            entity.HasOne(d => d.Class).WithMany(p => p.ClassStudents)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_ClassStudents_Class");

            entity.HasOne(d => d.Student).WithMany(p => p.ClassStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassStudents_Student");
        });

        modelBuilder.Entity<LearningMaterial>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.ClassId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FileSize).HasMaxLength(50);
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.MaterialType).HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Class).WithMany(p => p.LearningMaterials)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_LearningMaterials_Class");
        });

        modelBuilder.Entity<MaterialCompletion>(entity =>
        {
            entity.HasKey(e => new { e.MaterialId, e.StudentId });

            entity.Property(e => e.StudentId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.CompletedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Material).WithMany(p => p.MaterialCompletions)
                .HasForeignKey(d => d.MaterialId)
                .HasConstraintName("FK_MaterialCompletions_Material");

            entity.HasOne(d => d.Student).WithMany()
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MaterialCompletions_Student");
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.ClassId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.MaxPoints).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Class).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Assignments_Class");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Grade).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.StudentId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Assignment).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Submissions_Assignment");

            entity.HasOne(d => d.Student).WithMany()
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Submissions_Student");
        });

        modelBuilder.Entity<SupportFeedback>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.ClassId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.SenderId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Sender).WithMany()
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupportFeedbacks_Sender");
        });

        modelBuilder.Entity<DiscussionThread>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.ClassId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.AuthorId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Author).WithMany()
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiscussionThreads_Author");
        });

        modelBuilder.Entity<DiscussionReply>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.AuthorId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Thread).WithMany(p => p.DiscussionReplies)
                .HasForeignKey(d => d.ThreadId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DiscussionReplies_Thread");

            entity.HasOne(d => d.Author).WithMany()
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiscussionReplies_Author");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(e => e.Code, "UQ_Courses_Code").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(512);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Bio).HasMaxLength(1000);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
