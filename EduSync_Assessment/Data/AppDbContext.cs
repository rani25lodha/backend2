using System;
using System.Collections.Generic;
using EduSync_Assessment.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSync_Assessment.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AssessmentTable> AssessmentTables { get; set; }

    public virtual DbSet<CourseTable> CourseTables { get; set; }

    public virtual DbSet<ResultTable> ResultTables { get; set; }

    public virtual DbSet<UserTable> UserTables { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        //=> optionsBuilder.UseSqlServer("Server=tcp:edusyncserverrani.database.windows.net,1433;Initial Catalog=edusyncDB;Persist Security Info=False;User ID=project;Password=Rani@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssessmentTable>(entity =>
        {
            entity.HasKey(e => e.AssessmentId);

            entity.ToTable("Assessment_Table");

            entity.Property(e => e.AssessmentId).ValueGeneratedNever();
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Course).WithMany(p => p.AssessmentTables)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_Assessment_Table_Course_Table");
        });

        modelBuilder.Entity<CourseTable>(entity =>
        {
            entity.HasKey(e => e.CourseId);

            entity.ToTable("Course_Table");

            entity.Property(e => e.CourseId).ValueGeneratedNever();
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MediaUrl).IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Instructor).WithMany(p => p.CourseTables)
                .HasForeignKey(d => d.InstructorId)
                .HasConstraintName("FK_Course_Table_User_Table");
        });

        modelBuilder.Entity<ResultTable>(entity =>
        {
            entity.HasKey(e => e.ResultId);

            entity.ToTable("Result_Table");

            entity.Property(e => e.ResultId).ValueGeneratedNever();
            entity.Property(e => e.AttemptDate).HasColumnType("datetime");

            entity.HasOne(d => d.Assessment).WithMany(p => p.ResultTables)
                .HasForeignKey(d => d.AssessmentId)
                .HasConstraintName("FK_Result_Table_Assessment_Table");

            entity.HasOne(d => d.User).WithMany(p => p.ResultTables)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Result_Table_User_Table");
        });

        modelBuilder.Entity<UserTable>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("User_Table");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
