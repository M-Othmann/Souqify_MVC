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

        public async Task<Category?> GetById(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public Category Update(Category category)
        {
            _context.Update(category);
            _context.SaveChanges();

            return category;
        }

        public void Delete(int id)
        {
            var category = _context.Categories.Find(id);

            if (category is not null)
                _context.Remove(category);

            _context.SaveChanges();

        }
    }
}
