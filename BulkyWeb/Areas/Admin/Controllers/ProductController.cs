using Bulky.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Bulky.DataAccess.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Bulky.Utility;
using Stripe;
using Product = Bulky.Models.Product;

namespace BulkyWeb.Areas.Admin.Controllers
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
            List<Product> ProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            return View(ProductList);
        }

        public IActionResult Upsert(int? Id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            if (Id == 0 || Id == null)
            {
                //create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.Product.Get(u => u.ProductId == Id, includeProperties: "ProductImages");
               // productVM.CoverImage = @"\images\missingImage2.png";
                var CImage = _unitOfWork.ProductImage.Get(u => u.ProductId == Id && u.CoverImage != "NOCV").CoverImage;
                if (CImage == null)
                {
                    productVM.CoverImage = @"\images\missingImage.png";
                }
                else
                {
                    productVM.CoverImage = CImage;
                }
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile>? files, IFormFile? file)
        {
            if (productVM.Product.Description == null)
            {
                TempData["error"] = "Please Fill Product Description.";
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if (productVM.Product.ProductId == 0)
                    {
                        //cheking image is null 
                        if (file == null || files == null || files.Count <= 0)
                        {
                            productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                            {
                                Text = u.Name,
                                Value = u.Id.ToString()
                            });

                            if (file == null)
                                TempData["error"] = "Please Select Cover Image.";
                            else
                                TempData["error"] = "Please Select Images.";

                            return View(productVM);
                        }
                        //create product
                        _unitOfWork.Product.Add(productVM.Product);
                        _unitOfWork.Save();
                        TempData["success"] = "Product Created Successfully.";
                    }
                    else
                    {
                        //cheking image is null 
                        productVM.CoverImage = _unitOfWork.ProductImage.Get(u => u.ProductId == productVM.Product.ProductId && u.CoverImage != "NOCV").CoverImage;
                        productVM.Product.ProductImages = _unitOfWork.ProductImage.GetAll(u => u.ProductId == productVM.Product.ProductId).ToList();
                        if (productVM.CoverImage == "NOCV" && file == null || productVM.Product.ProductImages == null)
                        {

                            productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                            {
                                Text = u.Name,
                                Value = u.Id.ToString()
                            });

                            if (productVM.CoverImage == "NOCV")
                                TempData["error"] = "Please Select Cover Image.";
                            else
                                TempData["error"] = "Please Select Images.";
                            return View(productVM);
                        }
                        //edit - update product
                        _unitOfWork.Product.Update(productVM.Product);
                        _unitOfWork.Save();
                        TempData["success"] = "Product Updated Successfully.";
                    }

                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    if (file != null)
                    {
                        string filenameC = @"CV-" + Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPathC = @"images\products\PR-" + productVM.Product.ProductId;
                        string productImgPathC = Path.Combine(wwwRootPath, productPathC);

                        if (!Directory.Exists(productImgPathC))
                        {
                            Directory.CreateDirectory(productImgPathC);
                        }
                        using (var filestream = new FileStream(Path.Combine(productImgPathC, filenameC), FileMode.Create))
                        {
                            file.CopyTo(filestream);
                        }

                        ProductImage productImage = new()
                        {
                            ImageUrl = "",
                            CoverImage = @"\" + productPathC + @"\" + filenameC,
                            ProductId = productVM.Product.ProductId
                        };

                        if (productVM.Product.ProductImages == null)
                        {
                            productVM.Product.ProductImages = new List<ProductImage>();
                        }

                        productVM.Product.ProductImages.Add(productImage);

                        _unitOfWork.Product.Update(productVM.Product);
                        _unitOfWork.Save();
                    }
                    if (files != null)
                    {
                        foreach (IFormFile Lfile in files)
                        {
                            string filename = @"PR-" + Guid.NewGuid().ToString() + Path.GetExtension(Lfile.FileName);
                            string productPath = @"images\products\PR-" + productVM.Product.ProductId;
                            string productImgPath = Path.Combine(wwwRootPath, productPath);

                            if (!Directory.Exists(productImgPath))
                            {
                                Directory.CreateDirectory(productImgPath);
                            }
                            using (var filestream = new FileStream(Path.Combine(productImgPath, filename), FileMode.Create))
                            {
                                Lfile.CopyTo(filestream);
                            }

                            ProductImage productImage = new()
                            {
                                ImageUrl = @"\" + productPath + @"\" + filename,
                                CoverImage = "NOCV",
                                ProductId = productVM.Product.ProductId
                            };

                            if (productVM.Product.ProductImages == null)
                            {
                                productVM.Product.ProductImages = new List<ProductImage>();
                            }

                            productVM.Product.ProductImages.Add(productImage);
                        }
                        _unitOfWork.Product.Update(productVM.Product);
                        _unitOfWork.Save();
                    }


                    return RedirectToAction("Index");
                }
                else
                {
                    productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    });
                    return View(productVM);
                }
            }
        }
        public IActionResult DeleteImage(int imageId, string strImgetype)
        {
            var imageToBeDeleted = _unitOfWork.ProductImage.Get(u => u.ProuctImageId == imageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (strImgetype == "COV" && !string.IsNullOrEmpty(imageToBeDeleted.CoverImage))
                {
                    //delete old cover image
                    string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.CoverImage.TrimStart('\\'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }                  
                    TempData["info"] = "Please select cover image. Image is deleted from the directory.!";
                    TempData["success"] = "Cover Image Deleted Successfully.";
                }
                else if (strImgetype == "IMG" && !string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    //delete old image
                    string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }

                    TempData["success"] = "Image Deleted Successfully.";                   
                }
                _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                _unitOfWork.Save();
            }
            return RedirectToAction(nameof(Upsert), new { id = productId });
        }



        #region API CALLS
        //https://localhost:7148/Admin/Product/GetProducts
        //if we run project we get data in json format
        [HttpGet]
        public IActionResult GetProducts()
        {
            List<Product> ProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = ProductList });
        }
        [HttpDelete]
        public IActionResult Delete(int? Id)
        {
            Product product = _unitOfWork.Product.Get(u => u.ProductId == Id);
            if (product == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            //delete old image          

            string productPath = @"images\products\PR-" + Id;
            string productImgPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(productImgPath))
            {
                string[] imagefiles = Directory.GetFiles(productImgPath);
                foreach (string imagefile in imagefiles)
                {
                    System.IO.File.Delete(imagefile);
                }
                Directory.Delete(productImgPath);
            }
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();

            TempData["success"] = "Product Deleted Successfully.";
            return Json(new { success = true, message = "Product Deleted Successfully." });
        }
        #endregion
    }
}
