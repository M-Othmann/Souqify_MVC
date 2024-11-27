using Souqify.Models;

namespace Souqify.Services
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAll();

        Task Create(Category model);

        Task<Category?> GetById(int id);

        Category Update(Category category);

        void Delete(int id);
    }
}
