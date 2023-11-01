using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        public UserController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult RoleManagement(string userId)
        {
            RoleManagementVM roleManagementVM = new()
            {
                ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, includeProperties: "Company"),
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            roleManagementVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == userId)).GetAwaiter().GetResult().FirstOrDefault();

            return View(roleManagementVM);
        }
        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {
            var oldrole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == roleManagementVM.ApplicationUser.Id)).GetAwaiter().GetResult().FirstOrDefault();
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagementVM.ApplicationUser.Id);
            if (oldrole != roleManagementVM.ApplicationUser.Role)
            {
                //update              
                if (roleManagementVM.ApplicationUser.Role == SD.Role_Company)
                {
                    if (roleManagementVM.ApplicationUser.CompanyId != null)
                    {
                        applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                    }
                    else
                    {
                        roleManagementVM.RoleList = _roleManager.Roles.Select(i => new SelectListItem
                        {
                            Text = i.Name,
                            Value = i.Name
                        });
                        roleManagementVM.CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                        {
                            Text = i.Name,
                            Value = i.Id.ToString()
                        });
                        TempData["error"] = "Please select company from dropbox";
                        ViewBag.Error = "Please select company from dropbox";
                        return View(roleManagementVM);
                    }
                }
                if (oldrole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                TempData["success"] = "User Role Updated Successfully.";
                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(applicationUser, oldrole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            else
            {
                if (oldrole == SD.Role_Company && applicationUser.CompanyId != roleManagementVM.ApplicationUser.CompanyId)
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                    TempData["success"] = "User Role Updated Successfully.";
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

            return RedirectToAction(nameof(Index));
        }
        #region API CALLS

        [HttpGet]
        public IActionResult GetUsers()
        {
            List<ApplicationUser> userList = _unitOfWork.ApplicationUser.GetAll(includeProperties: "Company").ToList();
            foreach (var user in userList)
            {
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
                if (user.Company == null)
                {
                    user.Company = new() { Name = "" };
                }
            }

            return Json(new { data = userList });
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string? Id)
        {
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == Id);
            if (applicationUser == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            if (applicationUser.LockoutEnd != null && applicationUser.LockoutEnd > DateTime.Now)
            {
                //user is currently locked and we need to unlcok them
                applicationUser.LockoutEnd = DateTime.Now;
                TempData["success"] = "User unlock successfully.";
            }
            else
            {
                // we locking user
                applicationUser.LockoutEnd = DateTime.Now.AddYears(1000);
                TempData["success"] = "User lock successfully.";
            }

            _unitOfWork.ApplicationUser.Update(applicationUser);
            _unitOfWork.Save();

            return Json(new { success = true, message = TempData["success"] });
        }
        #endregion

        #region download Exel       
        [HttpPost]
        [ActionName(nameof(Index))]
        public IActionResult DownloadUserList()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Users");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Name";
                worksheet.Cell(currentRow, 2).Value = "Email";
                worksheet.Cell(currentRow, 3).Value = "Phone";
                worksheet.Cell(currentRow, 4).Value = "Company";
                worksheet.Cell(currentRow, 5).Value = "Role";
                worksheet.Cell(currentRow, 6).Value = "LockUnlock";
                worksheet.Cell(currentRow, 7).Value = "LockoutDate";
                List<ApplicationUser> userList = _unitOfWork.ApplicationUser.GetAll(includeProperties: "Company").ToList();
                string aRole;
                foreach (var user in userList)
                {
                    aRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == user.Id)).GetAwaiter().GetResult().FirstOrDefault();
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = user.UserName;
                    worksheet.Cell(currentRow, 2).Value = user.Email;
                    worksheet.Cell(currentRow, 3).Value = user.PhoneNumber;
                    if (aRole != SD.Role_Company)
                    {
                        worksheet.Cell(currentRow, 4).Value = "NA";
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 4).Value = user.Company.Name;
                    }
                    worksheet.Cell(currentRow, 5).Value = aRole;
                    worksheet.Cell(currentRow, 6).Value = user.LockoutEnabled;
                    worksheet.Cell(currentRow, 7).Value = user.LockoutEnd.ToString();
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "users_" + DateTime.Now.ToShortDateString() + ".xlsx");
                }
            }
        }
        #endregion
    }
}
