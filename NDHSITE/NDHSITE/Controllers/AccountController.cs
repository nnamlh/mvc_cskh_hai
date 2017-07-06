using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using NDHSITE.Models;
using System.IO;
using OfficeOpenXml;

namespace NDHSITE.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();

        public AccountController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (returnUrl != null && returnUrl != "/" && !returnUrl.Contains("/home"))
            {
                ViewBag.MSG = "Bạn không có quyền truy cập nội dung này.";
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);

                // check tk

                if (user != null)
                {

                    bool isActive = false;
                    var staff = db.HaiStaffs.Where(p => p.UserLogin == user.UserName).FirstOrDefault();
                    if (staff != null)
                    {
                        if (staff.IsLock != 1)
                            isActive = true;
                    }
                    else
                    {
                        var agency = db.CInfoCommons.Where(p => p.UserLogin == user.UserName).FirstOrDefault();
                        if (agency != null)
                        {
                            if (agency.IsClock != 1)
                                isActive = true;
                        }
                    }


                    if (!isActive)
                        ModelState.AddModelError("", "Tài khoản đã bị khóa.");
                    else
                    {
                        await SignInAsync(user, model.RememberMe);
                        return RedirectToLocal(returnUrl);
                    }


                }
                else
                {
                    ModelState.AddModelError("", "Sai tài khoản.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        // customize register: staff and cutomer
        //
        // GET: /Account/RegisterStaff
        [Authorize(Roles = "Administrator")]
        public ActionResult RegisterStaff()
        {
            ViewBag.Branch = db.HaiBranches.ToList();
            return View();
        }

        /*
        // GET: /Account/Register
        [Authorize(Roles = "Administrator")]
        public ActionResult Register()
        {
            return View();
        }
        */
        /*
        // POST: /Account/Register
        [HttpPost]
        [Authorize(Roles="Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser() { 
                    UserName = model.UserName,
                    IsActivced = 1
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                   // await SignInAsync(user, isPersistent: false);
                   // return RedirectToAction("Index", "Home");

                    return RedirectToAction("register", "account");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        */
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterStaff(RegisterViewModel model, string StaffId, string role)
        {
            if (ModelState.IsValid)
            {
                var check = db.HaiStaffs.Find(StaffId);

                if (check == null)
                {
                    return RedirectToAction("error", "home");
                }

                var user = new ApplicationUser()
                {
                    UserName = model.UserName,
                    IsActivced = 1,
                    FullName = check.FullName,
                    AccountType = "STAFF"
                };

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    check.UserLogin = user.UserName;
                    db.Entry(check).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    result = UserManager.AddToRole(user.Id, role);

                    RedirectToAction("modifystaff", "haistaff", new { Id = StaffId });
                }
                else
                {
                    AddErrors(result);
                }
            }
            // If we got this far, something failed, redisplay form
            return RedirectToAction("modifystaff", "haistaff", new { Id = StaffId, model });
        }


        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterAgency(RegisterViewModel model, string AgencyId, string role, string AgencyType)
        {
            string returnAction = "modify";
            switch (AgencyType)
            {
                case "CI":
                    returnAction += "ci";
                    break;
                case "CII":
                    returnAction += "cii";
                    break;
                case "FARMER":
                    returnAction += "farmer";
                    break;
            }

            if (ModelState.IsValid)
            {

                var check = checkAgency(AgencyId, AgencyType);

                if (check == null)
                {
                    return RedirectToAction("error", "home");
                }

                var user = new ApplicationUser()
                {
                    UserName = model.UserName,
                    IsActivced = 1,
                    FullName = check.CName,
                    AccountType = AgencyType
                };

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    check.UserLogin = user.UserName;
                    db.Entry(check).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    result = UserManager.AddToRole(user.Id, role);

                    RedirectToAction(returnAction, "agency", new { Id = AgencyId });
                }
                else
                {
                    AddErrors(result);
                }
            }
            // If we got this far, something failed, redisplay form
            return RedirectToAction(returnAction, "agency", new { Id = AgencyId });
        }


        private CInfoCommon checkAgency(string Id, string AgencyType)
        {
            if (AgencyType == "CI")
            {
                var agency = db.C1Info.Find(Id);
                if (agency != null)
                {
                    return agency.CInfoCommon;
                }
            }
            else if (AgencyType == "CII")
            {
                var agency = db.C2Info.Find(Id);
                if (agency != null)
                {
                    return agency.CInfoCommon;
                }
            }
            else if (AgencyType == "FARMER")
            {
                var agency = db.FarmerInfoes.Find(Id);
                if (agency != null)
                {
                    return agency.CInfoCommon;
                }
            }

            return null;
        }

        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        // change pass for user -- permission for admin role

        [Authorize(Roles = "Administrator")]
        public ActionResult AdminManage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";

            ViewBag.ReturnUrl = Url.Action("AdminManage");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdminManage(ManageAdminViewModel model)
        {
            ViewBag.ReturnUrl = Url.Action("AdminManage");
            // User does not have a password so remove any validation errors caused by a missing OldPassword field

            if (model.UserName == User.Identity.Name)
            {
                ModelState.AddModelError("", "Bạn không thể tự đổi mật khẩu của mình bằng cách này.");
                return View(model);
            }


            if (ModelState.IsValid)
            {
                var checkUser = db.AspNetUsers.Where(p => p.UserName == model.UserName).FirstOrDefault();

                if (checkUser == null)
                    ModelState.AddModelError("", "Sai tên tài khoản.");
                else
                {

                    var checkRole = checkUser.AspNetRoles.FirstOrDefault();
                    if (checkRole != null)
                    {
                        if (checkRole.Name == "Administrator")
                        {
                            ModelState.AddModelError("", "Không thể đổi mật khẩu của Admin khác.");
                            return View(model);
                        }
                    }

                    checkUser.PasswordHash = null;
                    db.Entry(checkUser).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    IdentityResult result = await UserManager.AddPasswordAsync(checkUser.Id, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("AdminManage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }


            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }



        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /*
        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
            }
        }

        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser() { UserName = model.UserName };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }
        */
        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        /*
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }
        */
        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion


        // xoa user
        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteUser()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult AddUser()
        {
            ViewBag.RoleList = db.AspNetRoles.Where(p=> p.GroupRole == "CUS").ToList();
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<ActionResult> AddUser(HttpPostedFileBase files, string role)
        {


            if (files != null && files.ContentLength > 0 && (role == "User" || role == "Guest"))
            {
                string extension = System.IO.Path.GetExtension(files.FileName);
                if (extension.Equals(".xlsx") || extension.Equals(".xls"))
                {
                    string fileSave = "account_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + extension;
                    string path = Server.MapPath("~/temp/" + fileSave);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    files.SaveAs(path);

                    FileInfo newFile = new FileInfo(path);
                    var package = new ExcelPackage(newFile);
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    int totalRows = sheet.Dimension.End.Row;
                    int totalCols = sheet.Dimension.End.Column;

                    List<AccountAgencyResult> resultCheck = new List<AccountAgencyResult>();

                    for (int i = 2; i <= totalRows; i++)
                    {
                        string code = Convert.ToString(sheet.Cells[i, 1].Value).Trim();

                        string user = Convert.ToString(sheet.Cells[i, 2].Value).Trim();

                        string pass = Convert.ToString(sheet.Cells[i, 3].Value).Trim();


                        var cinfo = db.CInfoCommons.Where(p => p.CCode == code).FirstOrDefault();

                        if (cinfo != null && !String.IsNullOrEmpty(user) && !String.IsNullOrEmpty(pass))
                        {

                            cinfo.Phone = pass;

                            var userLoginOld = cinfo.UserLogin;

                            if (!String.IsNullOrEmpty(userLoginOld))
                            {
                                var checkAspUser = db.AspNetUsers.Where(p => p.UserName == userLoginOld).FirstOrDefault();
                                if (checkAspUser != null)
                                {
                                    db.AspNetUsers.Remove(checkAspUser);
                                    db.SaveChanges();
                                }
                            }

                            var accountAgency = new ApplicationUser()
                            {
                                UserName = user,
                                IsActivced = 1,
                                FullName = cinfo.CName,
                                AccountType = cinfo.CType
                            };

                            var result = await UserManager.CreateAsync(accountAgency, pass);
                            if (result.Succeeded)
                            {
                                cinfo.UserLogin = accountAgency.UserName;
                                db.Entry(cinfo).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                result = UserManager.AddToRole(accountAgency.Id, role);


                                resultCheck.Add(new AccountAgencyResult()
                                {
                                    code = code,
                                    user = user,
                                    status = "success"
                                });


                            }
                            else
                                resultCheck.Add(new AccountAgencyResult()
                                {
                                    code = code,
                                    user = user,
                                    status = "fail"
                                });

                        }
                        else
                            resultCheck.Add(new AccountAgencyResult()
                            {
                                code = code,
                                user = user,
                                status = "Sai ma khach hang hoac tk da ton tai"
                            });

                    }


                    string pathRoot = Server.MapPath("~/haiupload/accountresult.xlsx");
                    string name = "accountresult" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
                    string pathTo = Server.MapPath("~/temp/" + name);

                    System.IO.File.Copy(pathRoot, pathTo);
                    try
                    {
                        FileInfo export = new FileInfo(pathTo);

                        using (ExcelPackage packageExport = new ExcelPackage(export))
                        {
                            ExcelWorksheet worksheet = packageExport.Workbook.Worksheets["Sheet1"];

                            for (var i = 0; i < resultCheck.Count; i++)
                            {

                                try
                                {
                                    worksheet.Cells[i + 2, 1].Value = resultCheck[i].code;
                                    worksheet.Cells[i + 2, 2].Value = resultCheck[i].user;
                                    worksheet.Cells[i + 2, 3].Value = resultCheck[i].status;
                                }
                                catch
                                {

                                }

                            }

                            packageExport.Save();

                        }

                    }
                    catch
                    {
                        return RedirectToAction("error", "home");
                    }


                    return File(pathTo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("danh-sach-account" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".{0}", "xlsx"));


                }

                // import excel
            }


            return View();
        }


        [Authorize(Roles = "Administrator")]
        // xoa tai khoan
        public ActionResult UserInfo(string user = "")
        {


            var cInfo = db.CInfoCommons.Where(p => p.UserLogin == user).FirstOrDefault();

            ViewBag.User = user;

            var result = new UserInfoData();

            if (cInfo != null)
            {
                if (cInfo.CType == "CI")
                    result.type = "Đại lý cấp 1";
                else if (cInfo.CType == "CII")
                    result.type = "Đại lý cấp 2";
                else if (cInfo.CType == "FARMER")
                    result.type = "Nông dân";

                result.fullname = cInfo.CName;
                result.phone = cInfo.Phone;
                result.address = cInfo.AddressInfo;
                if (cInfo.BirthDay != null)
                    result.birthday = cInfo.BirthDay.Value.ToShortDateString();
                result.user = user;
                result.area = cInfo.HaiArea.Name;
                result.code = cInfo.CCode;
                result.branch = cInfo.BranchCode;

                return View(result);
            }
            else
            {
                var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();
                if (staff != null)
                {
                    result.type = "Nhân viên";
                    result.fullname = staff.FullName;
                    result.phone = staff.Phone;
                    result.address = staff.HaiBranch.Name;
                    result.area = staff.HaiBranch.HaiArea.Name;
                    result.user = user;
                    if (staff.BirthDay != null)
                        result.birthday = staff.BirthDay.Value.ToShortDateString();
                    result.code = staff.Code;
                    result.branch = staff.HaiBranch.Code;


                    return View(result);
                }
            }


            return View();
        }

        public ActionResult DeleteAccount(string user)
        {
            var userCheck = UserManager.FindByName(user);
            if (userCheck != null)
            {
                var userRole = userCheck.Roles.ToList();

                foreach (var role in userRole)
                {
                    UserManager.RemoveFromRole(userCheck.Id, role.Role.Name);
                }
                var cInfo = db.CInfoCommons.Where(p => p.UserLogin == user).FirstOrDefault();
                if (cInfo != null)
                {
                    cInfo.UserLogin = "";
                    db.Entry(cInfo).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    var staff = db.HaiStaffs.Where(p => p.UserLogin == user).FirstOrDefault();
                    if (staff != null)
                    {
                        staff.UserLogin = "";
                        db.Entry(staff).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                 var account = new ApplicationUser()
                            {
                                UserName = userCheck.UserName,
                                Id = userCheck.Id
                            };

     
            }

            return RedirectToAction("UserInfo", "Account", new { user = user });

        }
    }
}