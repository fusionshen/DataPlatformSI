using DataPlatformSI.Entities;
using System.Collections.Generic;

namespace DataPlatformSI.Services.Contracts
{
    public interface IProductService
    {
        void AddNewProduct(Product product);
        IList<Product> GetAllProducts();
    }
}