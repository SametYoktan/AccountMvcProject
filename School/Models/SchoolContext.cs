using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
namespace School.Models
{
    public class SchoolContext:DbContext
    {
        public SchoolContext(DbContextOptions<SchoolContext>options) :base(options)
        {
        
        }

        public DbSet<Student> Student { get; set; }
        // Yeni tabloları DbContext'e ekliyoruz
        public DbSet<User> Users { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }  // Kullanıcı-Rol ilişkisi
        public DbSet<NewUser> NewUsers { get; set; }  // Kullanıcı-Rol ilişkisi
        public DbSet<NewUserLoginHistory> NewLoginHistort { get; set; }  // Kullanıcı-Rol ilişkisi
        public DbSet<NewPasswordResetHistoryToken> NewPasswordHistory { get; set; }  // Kullanıcı-Rol ilişkisi
        public DbSet<NewEmailHistoryToken> NewEmailHistory { get; set; }  // Kullanıcı-Rol ilişkisi

    }
}
