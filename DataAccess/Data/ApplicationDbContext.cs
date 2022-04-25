﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<EmployeeNote> EmployeeNotes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Department>()
                .HasMany(d => d.Employees)
                .WithOne(d => d.Department)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Employee>()
                .HasMany(e => e.NotesWritten)
                .WithOne(n => n.Author)
                .HasForeignKey(n => n.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Employee>()
                .HasMany(e => e.NotesAbout)
                .WithOne(n => n.Employee)
                .HasForeignKey(n => n.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
