using Souqify.Models;
using Souqify.Services;

namespace Souqify.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            var categoryList = _categoryService.GetAll();
            return View(categoryList);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category model)
        {
            if (!ModelState.IsValid)
                return View();

            _categoryService.Create(model);

            return RedirectToAction("Index");
        }
    }
}
