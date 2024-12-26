using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Souqify.DataAccess.Repository.IRepository;
using Souqify.Models;
using Souqify.Utilities;

namespace Souqify.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var CompanyList = _unitOfWork.Company.GetAll().ToList();
            return View(CompanyList);
        }

        public IActionResult Upsert(int? id)
        {
            var model = new Company();

            if (id is null || id == 0)
                return View(model);

            model = _unitOfWork.Company.GetFirstOrDefault(c => c.Id == id);
            return View(model);
        }

        [HttpPost]
        public IActionResult Upsert(Company model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    _unitOfWork.Company.Add(model);

                }
                else
                {
                    _unitOfWork.Company.Update(model);

                }

                _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");

            }
            else
            {
                return View(model);
            }
        }


        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companies = _unitOfWork.Company.GetAll().ToList();

            return Json(new { data = companies });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToDelete = _unitOfWork.Company.GetFirstOrDefault(c => c.Id == id);

            if (companyToDelete is null)
                return Json(new { success = false, message = "Error while deleting" });

            _unitOfWork.Company.Remove(companyToDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Company deleted successfully" });
        }

        #endregion

    }
}
