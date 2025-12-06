using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Mikhaylova_lr2.Models;

namespace Mikhaylova_lr2
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Subject> Subjects { get; set; }
        public DbSet<ClassSchedule> ClassSchedules { get; set; }
        public DbSet<WeeklySchedule> WeeklySchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subject>()
                .HasKey(s => s.Id);
            modelBuilder.Entity<Subject>()
                .Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ClassSchedule>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<ClassSchedule>()
                .Property(c => c.Classroom)
                .IsRequired()
                .HasMaxLength(10);
            modelBuilder.Entity<ClassSchedule>()
                .Property(c => c.GroupNumber)
                .IsRequired()
                .HasMaxLength(8); // Формат: XX-00-00
            modelBuilder.Entity<ClassSchedule>()
                .Property(c => c.ClassType)
                .IsRequired()
                .HasMaxLength(50);
            modelBuilder.Entity<ClassSchedule>()
                .Property(c => c.TeacherName)
                .IsRequired()
                .HasMaxLength(100);

            // Связь ClassSchedule с Subject
            modelBuilder.Entity<ClassSchedule>()
                .HasOne(c => c.Subject)
                .WithMany()
                .HasForeignKey(c => c.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка WeeklySchedule
            modelBuilder.Entity<WeeklySchedule>()
                .HasKey(w => w.Id);
            modelBuilder.Entity<WeeklySchedule>()
                .Property(w => w.GroupNumber)
                .IsRequired()
                .HasMaxLength(8);
            modelBuilder.Entity<WeeklySchedule>()
                .HasMany(w => w.Classes)
                .WithOne()
                .HasForeignKey(c => c.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
