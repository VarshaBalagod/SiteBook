using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe;
using System.Security.Claims;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;

        [BindProperty] // binding for post summary action
        public ShoppingCartVM? ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
        }

        #region Cart Page       

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userIId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartsList = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == userIId, includeProperties: "Product"),
                OrderHeader = new()
            };

            IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();

            foreach (var cart in ShoppingCartVM.ShoppingCartsList)
            {
                cart.Product.ProductImages = productImages.Where(u => u.ProductId == cart.ProductId).ToList();
                cart.CoverImage = _unitOfWork.ProductImage.Get(u => u.ProductId == cart.ProductId && u.CoverImage != "NOCV").CoverImage; 
                cart.TPrice = GetPriceBaseOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.TPrice * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int ShopCrtId)
        {
            var cartFromDB = _unitOfWork.ShoppingCart.Get(u => u.ShopCrtId == ShopCrtId);
            cartFromDB.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDB);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int ShopCrtId)
        {
            var cartFromDB = _unitOfWork.ShoppingCart.Get(u => u.ShopCrtId == ShopCrtId , tracked:true);
            if (cartFromDB.Count <= 1)
            {
                HttpContext.Session.SetInt32(SD.SessionCart, (_unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDB.ApplicationUserId).Count() - 1));
                //remove cart from shopping
                _unitOfWork.ShoppingCart.Remove(cartFromDB);
            }
            else
            {
                cartFromDB.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDB);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int ShopCrtId)
        {
            var cartFromDB = _unitOfWork.ShoppingCart.Get(u => u.ShopCrtId == ShopCrtId, tracked: true);
            HttpContext.Session.SetInt32(SD.SessionCart, (_unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDB.ApplicationUserId).Count() - 1));
            _unitOfWork.ShoppingCart.Remove(cartFromDB);          
            _unitOfWork.Save();
           
            return RedirectToAction(nameof(Index));
        }
        #endregion


        #region Order deatail / summary

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userIId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartsList = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == userIId, includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userIId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.Country = ShoppingCartVM.OrderHeader.ApplicationUser.Country;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ShoppingCartsList)
            {
                cart.TPrice = GetPriceBaseOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.TPrice * cart.Count);
            }

            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userIId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userIId, includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userIId;

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userIId);

            foreach (var cart in ShoppingCartVM.ShoppingCartsList)
            {
                cart.TPrice = GetPriceBaseOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.TPrice * cart.Count);
            }

            //checking user is company or customer 
            //because company user can make payment in 30 day span 
            //on other hand cutomer or any user have to pay instant
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //regular customer / user 
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                //company user
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            //order header created
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();


            //order Details need to fill now
            foreach (var cart in ShoppingCartVM.ShoppingCartsList)
            {
                OrderDetail ordeDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.OrderId,
                    OPrice = cart.TPrice,
                    OCount = cart.Count
                };
                _unitOfWork.OrderDetail.Add(ordeDetail);
                _unitOfWork.Save();
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //regular customer / user 
                //its a regular cutomer account and we need to capture payment
                //stipe logic
                // var domain = "https://localhost:7148/";
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var options = new SessionCreateOptions
                {

                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.OrderId}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.ShoppingCartsList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.TPrice * 100),//$20.5-=>2050
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

                var service = new SessionService();
                Session session = service.Create(options);

                //PaymentIntentId - will be null after payment done it will generate on stripe

                _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.OrderId, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.OrderId });
        }
        #endregion

        #region Confirmation page

        public IActionResult OrderConfirmation(int Id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.OrderId == Id, includeProperties: "ApplicationUser");
            if(orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //this is an order by customer
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if(session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(Id, SD.StatusApproved,SD.PaymentStatusApproved);                    
                    _unitOfWork.Save();
                }
                HttpContext.Session.Clear();
            }

            ////after oder complete send mail 
            ////calling send mail
            _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book", $"<p>New Order Created - {orderHeader.OrderId} </p>");

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u=> u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
          
            return View(Id);
        }

        #endregion

        private double GetPriceBaseOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else if (shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            else
            {
                return shoppingCart.Product.Price100;
            }
        }
    }
}
