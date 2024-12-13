using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SouqifyRazor_Temp.Data;
using SouqifyRazor_Temp.Models;

namespace SouqifyRazor_Temp.Pages.Categories
{
    [BindProperties]
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _db;

        public Category Category { get; set; }
        public DeleteModel(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult OnGet(int? id)
        {
            if (id != null && id != 0)
                Category = _db.Categories.Find(id);

            Category? obj = _db.Categories.Find(Category.Id);

            if (obj is null)
                return NotFound();

            _db.Categories.Remove(obj);
            _db.SaveChanges();
            TempData["success"] = "Category deleted successfully";
            return RedirectToPage("Index");
        }
    }
}
