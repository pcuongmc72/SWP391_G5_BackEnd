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

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    // SQL table name: LearningMaterials
    public virtual DbSet<Material> Materials { get; set; }

    public virtual DbSet<MaterialCompletion> MaterialCompletions { get; set; }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server =(local); database = FlippedClassroom ;uid=sa;pwd=123;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── AcademicTerms ─────────────────────────────────────────────────────
        modelBuilder.Entity<AcademicTerm>(entity =>
        {
            entity.HasIndex(e => e.TermCode, "UQ_AcademicTerms_TermCode")
                  .IsUnique()
                  .HasFilter("[TermCode] IS NOT NULL"); // matches SQL partial index

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.TermCode).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        // ── Courses ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(e => e.Code, "UQ_Courses_Code").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        // ── Users ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.Id).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(512);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysdatetime())");
        });

        // ── Classes ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Class>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.LecturerId).HasMaxLength(20).IsUnicode(false);

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

        // ── ClassStudents ─────────────────────────────────────────────────────
        modelBuilder.Entity<ClassStudent>(entity =>
        {
            entity.HasKey(e => new { e.ClassId, e.StudentId });

            entity.Property(e => e.ClassId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.StudentId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.EnrolledAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassStudents)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_ClassStudents_Class");

            entity.HasOne(d => d.Student).WithMany(p => p.ClassStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassStudents_Student");
        });

        // ── LearningMaterials ─────────────────────────────────────────────────
        // SQL table is named [LearningMaterials], C# model is Material.
        modelBuilder.Entity<Material>(entity =>
        {
            entity.ToTable("LearningMaterials");   // ← maps to correct SQL table name

            entity.HasKey(e => e.Id);
            // Id is uniqueidentifier — no HasMaxLength, no IsUnicode
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");

            entity.Property(e => e.ClassId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(255);

            // varchar(20) NOT NULL — column name in SQL is [MaterialType]
            entity.Property(e => e.MaterialType)
                  .HasColumnName("MaterialType")
                  .HasMaxLength(20)
                  .IsUnicode(false);

            // nvarchar(500) NULL — column name in SQL is [FileUrl]
            entity.Property(e => e.FileUrl)
                  .HasColumnName("FileUrl")
                  .HasMaxLength(500);

            entity.Property(e => e.FileSize).HasMaxLength(50);

            // date NOT NULL DEFAULT CONVERT([date], sysdatetime())
            entity.Property(e => e.UploadedAt)
                  .HasColumnType("date")
                  .HasDefaultValueSql("(CONVERT([date], sysdatetime()))");

            // datetime2(0) NOT NULL DEFAULT (sysdatetime())
            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime2(0)")
                  .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Class)
                .WithMany()
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_LearningMaterials_Class");
        });

        // ── MaterialCompletions ───────────────────────────────────────────────
        modelBuilder.Entity<MaterialCompletion>(entity =>
        {
            entity.HasKey(e => new { e.MaterialId, e.StudentId });

            // MaterialId is uniqueidentifier — maps to LearningMaterials.Id
            entity.Property(e => e.StudentId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.CompletedAt)
                  .HasColumnType("datetime2(0)")
                  .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Material)
                .WithMany(p => p.MaterialCompletions)
                .HasForeignKey(d => d.MaterialId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_MaterialCompletions_Material");

            entity.HasOne(d => d.Student)
                .WithMany()
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_MaterialCompletions_Student");
        });

        // ── Assignments ───────────────────────────────────────────────────────
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Id is uniqueidentifier — no HasMaxLength
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");

            entity.Property(e => e.ClassId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(255);

            // date NOT NULL
            entity.Property(e => e.DueDate).HasColumnType("date");

            // decimal(5,2) NOT NULL DEFAULT ((10))
            entity.Property(e => e.MaxPoints)
                  .HasColumnType("decimal(5,2)")
                  .HasDefaultValue(10m);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Class)
                .WithMany()
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Assignments_Class");
        });

        // ── Submissions ───────────────────────────────────────────────────────
        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Id is uniqueidentifier — no HasMaxLength
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");

            // AssignmentId is uniqueidentifier — no HasMaxLength
            entity.Property(e => e.StudentId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.FileName).HasMaxLength(255);
            // NOTE: SQL has NO FileUrl column in Submissions — not mapped here

            // nvarchar(max) NULL — no HasMaxLength needed for nvarchar(max)
            // entity.Property(e => e.StudentNotes) — implicit

            // varchar(20) NOT NULL  CHECK: 'GRADED'|'SUBMITTED'
            entity.Property(e => e.Status)
                  .HasMaxLength(20)
                  .IsUnicode(false)
                  .HasDefaultValue("SUBMITTED");

            entity.Property(e => e.Grade).HasColumnType("decimal(5,2)");

            entity.Property(e => e.SubmittedAt)
                  .HasColumnType("datetime2(0)")
                  .HasDefaultValueSql("(sysdatetime())");

            entity.Property(e => e.GradedAt).HasColumnType("datetime2(0)");

            entity.HasIndex(e => new { e.AssignmentId, e.StudentId }, "UQ_Submissions_Assignment_Student")
                  .IsUnique();

            entity.HasOne(d => d.Assignment)
                .WithMany(p => p.Submissions)
                .HasForeignKey(d => d.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Submissions_Assignment");

            entity.HasOne(d => d.Student)
                .WithMany()
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Submissions_Student");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
