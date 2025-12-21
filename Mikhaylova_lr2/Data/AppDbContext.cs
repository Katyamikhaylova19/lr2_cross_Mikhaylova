using Microsoft.EntityFrameworkCore;
using Mikhaylova_lr2.Models;

namespace Mikhaylova_lr2.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<TeacherGroup> TeacherGroups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Конфигурация для TeacherGroup (связь многие-ко-многим)
        modelBuilder.Entity<TeacherGroup>()
            .HasKey(tg => tg.Id);

        modelBuilder.Entity<TeacherGroup>()
            .HasOne(tg => tg.Teacher)
            .WithMany(t => t.TeacherGroups)
            .HasForeignKey(tg => tg.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TeacherGroup>()
            .HasOne(tg => tg.Group)
            .WithMany(g => g.TeacherGroups)
            .HasForeignKey(tg => tg.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Уникальность связи преподаватель-группа
        modelBuilder.Entity<TeacherGroup>()
            .HasIndex(tg => new { tg.TeacherId, tg.GroupId })
            .IsUnique();

        // Конфигурация для Rating
        modelBuilder.Entity<Rating>()
            .HasIndex(r => new { r.StudentId, r.TeacherId })
            .IsUnique();

        modelBuilder.Entity<Rating>()
            .HasOne(r => r.Student)
            .WithMany(s => s.Ratings)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rating>()
            .HasOne(r => r.Teacher)
            .WithMany(t => t.Ratings)
            .HasForeignKey(r => r.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        // Конфигурация для Student
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Group)
            .WithMany(g => g.Students)
            .HasForeignKey(s => s.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed данных (для тестирования)
        modelBuilder.Entity<Group>().HasData(
            new Group { Id = 1, GroupNumber = "АС-22-04" },
            new Group { Id = 2, GroupNumber = "АС-22-05" },
            new Group { Id = 3, GroupNumber = "АА-22-07" }
        );

        modelBuilder.Entity<Teacher>().HasData(
            new Teacher { Id = 1, FirstName = "Татьяна", LastName = "Михайлова", MiddleName = "Папилина" },
            new Teacher { Id = 2, FirstName = "Антон", LastName = "Тупысев", MiddleName = "Михайлович" },
            new Teacher { Id = 3, FirstName = "Александр", LastName = "Асирян", MiddleName = "Вячеславович" }
        );

        modelBuilder.Entity<Student>().HasData(
            new Student { Id = 1, FirstName = "Екатерина", LastName = "Михайлова", MiddleName = "Сергеевна", GroupId = 1 },
            new Student { Id = 2, FirstName = "Екатерина", LastName = "Ткачева", MiddleName = "Дмитриевна", GroupId = 1 },
            new Student { Id = 3, FirstName = "Никита", LastName = "Клейменов", MiddleName = "Игоревич", GroupId = 1 },
            new Student { Id = 4, FirstName = "Иван", LastName = "Иванов", MiddleName = "Иванович", GroupId = 2 },
            new Student { Id = 5, FirstName = "Просковья", LastName = "Простова", MiddleName = "Петровна", GroupId = 2 },
            new Student { Id = 6, FirstName = "Геннадий", LastName = "Громов", MiddleName = "Васильевич", GroupId = 3 }
        );

        modelBuilder.Entity<TeacherGroup>().HasData(
            new TeacherGroup { Id = 1, TeacherId = 1, GroupId = 1 },
            new TeacherGroup { Id = 2, TeacherId = 2, GroupId = 1 },
            new TeacherGroup { Id = 3, TeacherId = 2, GroupId = 2 },
            new TeacherGroup { Id = 4, TeacherId = 2, GroupId = 3 },
            new TeacherGroup { Id = 5, TeacherId = 3, GroupId = 3 }
        );
    }
}