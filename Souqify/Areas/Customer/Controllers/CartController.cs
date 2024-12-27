using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Souqify.DataAccess.Repository.IRepository;
using Souqify.Models;
using Souqify.Models.ViewModels;
using Souqify.Utilities;
using Stripe.Checkout;
using System.Security.Claims;

namespace Souqify.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
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
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(c => c.AppUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach (var cart in CartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                CartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(CartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            CartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(c => c.AppUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };

            CartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == userId);


            CartVM.OrderHeader.Name = CartVM.OrderHeader.ApplicationUser.Name;
            CartVM.OrderHeader.PhoneNumber = CartVM.OrderHeader.ApplicationUser.PhoneNumber;
            CartVM.OrderHeader.StreetAddress = CartVM.OrderHeader.ApplicationUser.StreetAddress;
            CartVM.OrderHeader.City = CartVM.OrderHeader.ApplicationUser.City;
            CartVM.OrderHeader.State = CartVM.OrderHeader.ApplicationUser.State;
            CartVM.OrderHeader.PostalCode = CartVM.OrderHeader.ApplicationUser.PostalCode;



            foreach (var cart in CartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                CartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(CartVM);

        }


        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            CartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(c => c.AppUserId == userId, includeProperties: "Product");


            CartVM.OrderHeader.OrderDate = System.DateTime.Now;
            CartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser appUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == userId);


            foreach (var cart in CartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                CartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (appUser.CompanyId.GetValueOrDefault() == 0)
            {
                //regulat customer and we need to capture payment
                CartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                CartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                //company user
                CartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                CartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            _unitOfWork.OrderHeader.Add(CartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in CartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = CartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }
            if (appUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:44314/";
                //regular user we need to capture payment stripe logic
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={CartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in CartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), //$20.50 => 2050
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new Stripe.Checkout.SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentId(CartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);

            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = CartVM.OrderHeader.Id });

        }



        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader.OrderStatus != SD.PaymentStatusDelayedPayment)
            {
                //order made by customer
                var service = new SessionService();

                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(sc => sc.AppUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();


            return View(id);
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
