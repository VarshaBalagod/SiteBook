using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {            
            IEnumerable<Product> proudctList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
            return View(proudctList);
        }

        public IActionResult Details(int Id)
        {
            ShoppingCart shoppingCart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.ProductId == Id, includeProperties: "Category"),               
                Count = 1,
                ProductId = Id
            };
            shoppingCart.Product.ProductImages = _unitOfWork.ProductImage.GetAll(u => u.ProductId == Id && u.CoverImage == "NOCV").ToList();
            var CImage = _unitOfWork.ProductImage.Get(u => u.ProductId == Id && u.CoverImage != "NOCV").CoverImage;
            if (CImage == null)
            {
                shoppingCart.CoverImage = @"\images\missingImage.png";
            }
            else
            {
                shoppingCart.CoverImage = CImage;
            }
            return View(shoppingCart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingcart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userIId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingcart.ApplicationUserId = userIId;
           
            // got error of identity_insert off on both table so change both id with different name and reffer to another object


            ShoppingCart cartFromDB = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userIId && u.ProductId == shoppingcart.Product.ProductId);

            if (cartFromDB != null)
            {
                //shopping cart exists
                cartFromDB.Count += shoppingcart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDB);
                _unitOfWork.Save();
                //int tst = (_unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userIId)).Count;
                //HttpContext.Session.SetInt32(SD.SessionCart, (_unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userIId).Count));
            }
            else
            {
                //add shopping cart
                ShoppingCart shoppingcart2 = new()
                {
                    ProductId = shoppingcart.Product.ProductId,
                    Count = shoppingcart.Count,
                    ApplicationUserId = userIId
                };
                _unitOfWork.ShoppingCart.Add(shoppingcart2);
                _unitOfWork.Save();
                int tst = (_unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userIId)).Count();
                HttpContext.Session.SetInt32(SD.SessionCart, (_unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userIId).Count()));
            }
         
            TempData["success"] = "Shopping card updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}