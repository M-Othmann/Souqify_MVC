using Souqify.Models;

namespace Souqify.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Category> GetAll()
        {
            return _context.Categories
                .AsNoTracking()
                .ToList();
        }
        public async Task Create(Category model)
        {
            _context.Add(model);
            await _context.SaveChangesAsync();
        }
    }
}
