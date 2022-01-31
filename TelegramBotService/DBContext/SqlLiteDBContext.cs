﻿using Microsoft.EntityFrameworkCore;
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
        public DbSet<Users> Users { get; set; }
        public DbSet<Payers> Payers { get; set; }

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

            modelBuilder.Entity<Users>().ToTable("Users");
            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(k => k.ID);
                entity.HasIndex(i => i.Name);
            });

            modelBuilder.Entity<Payers>().ToTable("Payers");
            modelBuilder.Entity<Payers>(entity =>
            {
                entity.HasKey(k => k.ID);
                entity.HasIndex(i => i.Name);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}