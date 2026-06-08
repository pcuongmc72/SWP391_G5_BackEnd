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

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server =(local); database = FlippedClassroom ;uid=sa;pwd=123;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcademicTerm>(entity =>
        {
            entity.HasIndex(e => e.TermCode, "UQ_AcademicTerms_TermCode").IsUnique(); // Bổ sung TermCode
            entity.Property(e => e.TermCode).HasMaxLength(20).IsUnicode(false); // Bổ sung TermCode

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
            entity.Property(e => e.StartDate);
            entity.Property(e => e.EndDate);
            entity.Property(e => e.LecturerId)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.Name).HasMaxLength(255).IsRequired(true);

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

            entity.HasOne(d => d.Class).WithMany(p => p.ClassStudents)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_ClassStudents_Class");

            entity.HasOne(d => d.Student).WithMany(p => p.ClassStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassStudents_Student");
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
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Content).IsUnicode(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.AuthorId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.ClassId).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(0); // 0: Pending, 1: Approved, 2: Rejected
            entity.Property(e => e.Keywords).HasMaxLength(500).IsUnicode(true);

            entity.HasOne(d => d.Author).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Blogs_Author");

            entity.HasOne(d => d.Course).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Blogs_Course");

            entity.HasOne(d => d.Class).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Blogs_Class");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Content).IsUnicode(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.AuthorId).HasMaxLength(20).IsUnicode(false);

            entity.HasOne(d => d.Author).WithMany(p => p.Comments)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comments_Author");

            entity.HasOne(d => d.Blog).WithMany(p => p.Comments)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Comments_Blog");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
