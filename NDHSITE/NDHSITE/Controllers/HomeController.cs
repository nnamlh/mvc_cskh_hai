﻿using NDHSITE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NDHSITE.Controllers
{
    public class HomeController : Controller
    {

        NDHDBEntities db = new NDHDBEntities();


        [Authorize]
        public ActionResult Index(int? page)
        {

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        [Authorize]
        public ActionResult Relogin()
        {
            return View();
        }

         [Authorize]
        public ActionResult AdminMenu(){

            var user = db.AspNetUsers.Where(p => p.UserName == User.Identity.Name).FirstOrDefault();

            var role = user.AspNetRoles.FirstOrDefault();

            return PartialView("_MenuAdmin", role);
         }

        [Authorize]
         public ActionResult AdminSlideMenu(string code)
         {
             return PartialView("_AdminSlideMenu", code);
         }

        [Authorize]
        public ActionResult ShowMenu()
        {

            var groupList = db.FuncGroups.ToList();

            var user = db.AspNetUsers.Where(p=> p.UserName == User.Identity.Name).FirstOrDefault();

            var role = user.AspNetRoles.FirstOrDefault();

            if (role == null)
                return RedirectToAction("error", "home");

            List<MenuGroup> menuGroups = new List<Models.MenuGroup>();

            foreach (var group in groupList)
            {
                MenuGroup menuGroup = new Models.MenuGroup()
                {
                    Name = group.Name
                };


                var listMenu = db.FuncRoles.Where(p => p.RoleId == role.Id && p.FuncInfo.GroupId == group.Id).OrderBy(p=> p.FuncInfo.Number).ToList();
                var menus = new List<MenuInfo>();
                foreach (var item in listMenu)
                {
                    menus.Add(new Models.MenuInfo()
                    {
                        Url = item.FuncInfo.UrlInfo,
                        Icon= item.FuncInfo.IconInfo,
                        Position = item.FuncInfo.Position,
                        Name = item.FuncInfo.Name
                    });
                }

                menuGroup.Menus = menus;

                menuGroups.Add(menuGroup);
            }


            return PartialView( "_MenuHai",menuGroups);
        }


        public ActionResult Policy()
        {
            return View();
        }

        public ActionResult FindCode()
        {
            ViewBag.Branchs = db.HaiBranches.ToList();

            return View(db.Provinces.ToList());
        }


    }
}