using Microsoft.AspNetCore.Mvc;
using Souqify.DataAccess.Repository.IRepository;

namespace Souqify.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var productList = _unitOfWork.Product.GetAll();
            return View(productList);
        }
    }
}
