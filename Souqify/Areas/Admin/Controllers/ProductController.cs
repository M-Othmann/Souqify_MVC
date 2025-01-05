using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Souqify.DataAccess.Repository.IRepository;
using Souqify.Models;
using Souqify.Models.ViewModels;
using Souqify.Utilities;

namespace Souqify.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> productList = _unitOfWork.Product
                .GetAll(includeProperties: "Category")
                .ToList();


            return View(productList);
        }

        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> categoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });

            //ViewBag.categoryList = categoryList;
            //ViewData["categoryList"] = categoryList;

            ProductVM productVM = new()
            {
                CategoryList = categoryList,
                Product = new Product()
            };

            if (id is null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id);
                return View(productVM);
            }

        }


        [HttpPost]
        public IActionResult Upsert(ProductVM model, List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                if (model.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(model.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(model.Product);

                }
                _unitOfWork.Save();
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (files is not null)
                {

                    foreach (IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"images\products\product-" + model.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if (!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        ProductImage prodImage = new()
                        {
                            ImageUrl = @"\" + productPath + @"\" + fileName,
                            ProductId = model.Product.Id
                        };

                        if (model.Product.ProductImages is null)
                            model.Product.ProductImages = new List<ProductImage>();

                        model.Product.ProductImages.Add(prodImage);
                    }

                    _unitOfWork.Product.Update(model.Product);
                    _unitOfWork.Save();


                    /*if (!string.IsNullOrEmpty(model.Product.ImageUrl))
                    {
                        //delete old image
                        var oldImagePath = Path.Combine(wwwRootPath, model.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                            System.IO.File.Delete(oldImagePath);

                    }*/

                    /* using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                     {
                         file.CopyTo(fileStream);
                     }*/

                    /* model.Product.ImageUrl = @"\images\products\" + fileName;*/
                }


                TempData["success"] = "Product created/updated successfully";

                return RedirectToAction("Index");
            }
            else
            {
                model.CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });


                return View(model);
            }



        }

        /*        public IActionResult Delete(int id)
                {
                    if (id == 0)
                        return NotFound();

                    var productFromDb = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id);
                    _unitOfWork.Product.Remove(productFromDb);
                    _unitOfWork.Save();
                    TempData["success"] = "Product deleted successfully";

                    return RedirectToAction("Index");
                }*/

        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> productList = _unitOfWork.Product
               .GetAll(includeProperties: "Category")
               .ToList();

            return Json(new { data = productList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id);

            if (productToBeDeleted is null)
                return Json(new { success = false, message = "Error while deleting" });

            /*var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
                System.IO.File.Delete(oldImagePath);*/


            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();


            return Json(new { success = true, message = "Product deleted successfully" });

        }

        #endregion
    }
}
