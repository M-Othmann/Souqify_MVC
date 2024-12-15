using Souqify.DataAccess.Repository.IRepository;
using Souqify.Models;


namespace Souqify.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var categoryList = _unitOfWork.Category.GetAll();
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

            _unitOfWork.Category.Add(model);
            _unitOfWork.Save();
            TempData["success"] = "Category created successfully";

            return RedirectToAction("Index");
        }


        public IActionResult Edit(int id)
        {
            if (id == 0)
                return NotFound();
            var categoryFromDb = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);

            if (categoryFromDb is null)
                return NotFound();

            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category model)
        {

            if (!ModelState.IsValid)
                return View();

            _unitOfWork.Category.Update(model);
            _unitOfWork.Save();
            TempData["success"] = "Category updated successfully";

            return RedirectToAction("Index");
        }


        public IActionResult Delete(int id)
        {
            if (id == 0)
                return NotFound();

            var categoryFromDb = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);
            _unitOfWork.Category.Remove(categoryFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Category Deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
