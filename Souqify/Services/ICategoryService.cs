using Souqify.Models;

namespace Souqify.Services
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAll();

        Task Create(Category model);
    }
}
