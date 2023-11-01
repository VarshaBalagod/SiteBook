using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfwork;

        public CategoryController(IUnitOfWork unitOfwork)
        {
            _unitOfwork = unitOfwork;
        }
        public IActionResult Index()
        {
            List<Category> CategoryList = _unitOfwork.Category.GetAll().ToList();
            return View(CategoryList);
        }        

        public IActionResult Upsert(int? Id)
        {
            if (Id == 0 || Id == null)
            {
                //create
                return View();
            }
            else
            {
                //update
                Category category = _unitOfwork.Category.Get(u => u.Id == Id);
                return View(category);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id != 0)
                {
                    //edit
                    _unitOfwork.Category.Update(category);
                    _unitOfwork.Save();
                    TempData["success"] = "Category updated successfully";
                    return RedirectToAction("Index", "Category");
                }
                else
                {
                    //create
                    _unitOfwork.Category.Add(category);
                    _unitOfwork.Save();
                    TempData["success"] = "Category created successfully";
                    return RedirectToAction("Index", "Category");
                }
            }
            return View();
        }

        #region API CALLS
        //https://localhost:7148/Admin/category/GetCategories
        [HttpGet]
        public IActionResult GetCategories()
        {
            List<Category> CategoryList = _unitOfwork.Category.GetAll().ToList();
            return Json(new { data = CategoryList });
        }
        [HttpDelete]
        public IActionResult Delete(int? Id)
        {
            Category? objCat = _unitOfwork.Category.Get(u => u.Id == Id);       
          
            if (objCat == null)
                return Json(new { success = false, message = "Error while deleting" });
            _unitOfwork.Category.Remove(objCat);
            _unitOfwork.Save();
            TempData["success"] = "Category deleted successfully";
            return Json(new { success = true, message = "Category deleted successfully." });
        }
        #endregion
    }
}
