using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Souqify.DataAccess.Repository.IRepository;
using Souqify.Models;

namespace Souqify.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> productList = _unitOfWork.Product
                .GetAll()
                .ToList();


            return View(productList);
        }

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> categoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });

            //ViewBag.categoryList = categoryList;
            ViewData["categoryList"] = categoryList;

            return View();
        }


        [HttpPost]
        public IActionResult Create(Product model)
        {
            if (!ModelState.IsValid)
                return View();

            _unitOfWork.Product.Add(model);
            _unitOfWork.Save();
            TempData["success"] = "Product created successfully";

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            if (id == 0)
                return NotFound();

            var productFromDb = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id);

            if (productFromDb is null)
                return NotFound();

            return View(productFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Product model)
        {
            if (!ModelState.IsValid)
                return View();

            _unitOfWork.Product.Update(model);
            _unitOfWork.Save();
            TempData["success"] = "Product updated successfully";

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            if (id == 0)
                return NotFound();

            var productFromDb = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id);
            _unitOfWork.Product.Remove(productFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Product deleted successfully";

            return RedirectToAction("Index");
        }
    }
}
