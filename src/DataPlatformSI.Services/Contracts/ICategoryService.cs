using DataPlatformSI.Entities;
using System.Collections.Generic;

namespace DataPlatformSI.Services.Contracts
{
    public interface ICategoryService
    {
        void AddNewCategory(Category category);
        IList<Category> GetAllCategories();
    }
}