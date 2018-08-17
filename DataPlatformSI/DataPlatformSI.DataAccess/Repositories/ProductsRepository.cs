using DataPlatformSI.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPlatformSI.DataAccess.Repositories
{
    public class ProductsRepository
    {
        private readonly ProductContext _context;

        public ProductsRepository(ProductContext context)
        {
            _context = context;

            if (_context.Products.Count() == 0)
            {
                _context.Products.AddRange(
                    new Product
                    {
                        Name = "Learning ASP.NET Core",
                        Description = "A best-selling book covering the fundamentals of ASP.NET Core"
                    },
                    new Product
                    {
                        Name = "Learning EF Core",
                        Description = "A best-selling book covering the fundamentals of Entity Framework Core"
                    });
                _context.SaveChanges();
            }
        }

        public IEnumerable<Product> GetProducts()
        {
            return _context.Products.ToList();
        }

        public bool TryGetProduct(int id, out Product product)
        {
            product = _context.Products.Find(id);

            return (product != null);
        }

        public async Task<int> AddProductAsync(Product product)
        {
            int rowsAffected = 0;

            _context.Products.Add(product);
            rowsAffected = await _context.SaveChangesAsync();

            return rowsAffected;
        }

        public IEnumerable<Movie> GetMovies()
        {
            return _context.Movies.ToList();
        }
    }
}
