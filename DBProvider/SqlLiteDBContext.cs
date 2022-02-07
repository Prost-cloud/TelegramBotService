using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TelegramBotService.DBContext
{
    public class SqlLiteDBContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingList> ShoppingList { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Payer> Payers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("FileName=sqlitedb", option =>
            {
                option.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(k => k.ID);
                entity.HasIndex(i => i.Name);
            });
           
            modelBuilder.Entity<ShoppingList>().ToTable("ShoppingList");
            modelBuilder.Entity<ShoppingList>(entity =>
            {
                entity.HasKey(k => k.ID);
                entity.HasIndex(i => i.Name);
            });

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(k => k.ID);
                entity.HasIndex(i => i.Name);
            });

            modelBuilder.Entity<Payer>().ToTable("Payers");
            modelBuilder.Entity<Payer>(entity =>
            {
                entity.HasKey(k => k.ID);
                entity.HasIndex(i => i.Name);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
