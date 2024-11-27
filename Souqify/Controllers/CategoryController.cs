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
            if (model.Name == model.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "Display order can not match name");
            }
            if (!ModelState.IsValid)
                return View();

            _categoryService.Create(model);
            TempData["success"] = "Category created successfully";

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
                return NotFound();
            var categoryFromDb = await _categoryService.GetById(id);

            if (categoryFromDb is null)
                return NotFound();

            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category model)
        {

            if (!ModelState.IsValid)
                return View();

            _categoryService.Update(model);
            TempData["success"] = "Category updated successfully";

            return RedirectToAction("Index");
        }


        public IActionResult Delete(int id)
        {
            if (id == 0)
                return NotFound();

            _categoryService.Delete(id);
            TempData["success"] = "Category Deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
