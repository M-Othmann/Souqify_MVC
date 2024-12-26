using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Souqify.DataAccess.Repository.IRepository;
using Souqify.Models;
using Souqify.Models.ViewModels;
using System.Security.Claims;

namespace Souqify.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM CartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            CartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(c => c.AppUserId == userId, includeProperties: "Product")
            };

            foreach (var cart in CartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                CartVM.OrderTotal += (cart.Price * cart.Count);
            }
            return View(CartVM);
        }

        public IActionResult Summary()
        {
            return View();
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);
            cartFromDb.Count += 1;

            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                //remove from cart
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;

                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);

            //remove from cart
            _unitOfWork.ShoppingCart.Remove(cartFromDb);

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(ShoppingCart cart)
        {
            if (cart.Count <= 50)
            {
                return cart.Product.Price;
            }
            else
            {
                if (cart.Count <= 100)
                {
                    return cart.Product.Price50;
                }
                else
                {
                    return cart.Product.Price100;

                }
            }
        }
    }
}
