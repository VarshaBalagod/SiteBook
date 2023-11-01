using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
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
            List<Company> comapnyList = _unitOfWork.Company.GetAll().ToList();
            return View(comapnyList);
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
                Company company = _unitOfWork.Company.Get(u => u.Id == Id);
                return View(company);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id != 0)
                {                   
                    //edit
                    _unitOfWork.Company.Update(company);
                    _unitOfWork.Save();
                    TempData["success"] = "Company updated successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    //create
                    _unitOfWork.Company.Add(company);
                    _unitOfWork.Save();
                    TempData["success"] = "Company created successfully";
                    return RedirectToAction("Index");
                }
            }
            return View();
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetCompanys()
        {
            List<Company> companyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = companyList });
        }
        [HttpDelete]
        public IActionResult Delete(int? Id)
        {
            Company Company = _unitOfWork.Company.Get(u => u.Id == Id);
            if (Company == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Company.Remove(Company);
            _unitOfWork.Save();
            TempData["success"] = "Company deleted successfully.";
            return Json(new { success = true, message = "Company deleted successfully." });
        }
        #endregion
    }
}
