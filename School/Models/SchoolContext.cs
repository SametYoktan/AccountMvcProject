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
        //public DbSet<User> Users { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }  // Kullanıcı-Rol ilişkisi

        public DbSet<NewEmailHistory> _NewEmailHistory {  get; set; }
        public DbSet<NewLoginHistory> _NewLoginHistory {  get; set; }
        public DbSet<NewPasswordHistory> _NewPasswordHistory {  get; set; }
        public DbSet<NewRoles> _NewRoles {  get; set; }
        public DbSet<NewUsers> _NewUsers {  get; set; }
        public DbSet<NewUserActivityLog> _NewUserActivityLog {  get; set; }
        public DbSet<NewUserIsActiveHistory> _NewUserIsActiveHistory {  get; set; }
        public DbSet<NewUserRoles> _NewUserRoles {  get; set; }
    }
}
