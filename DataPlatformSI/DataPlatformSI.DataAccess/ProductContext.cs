using DataPlatformSI.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataPlatformSI.DataAccess
{
    public class ProductContext : DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options)
            : base(options)
        {
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=DataPlatformSI.WebAPI.NewDb;Trusted_Connection=True;");
        //}

        public DbSet<Product> Products { get; set; }

        public DbSet<Movie> Movies { get; set; }
    }
}
