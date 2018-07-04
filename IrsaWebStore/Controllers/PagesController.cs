using IrsaWebStore.Models.Data;
using IrsaWebStore.Models.ViewModel.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IrsaWebStore.Controllers
{
    public class PagesController : Controller
    {
        // GET: Page
        public ActionResult Index(string page = "")
        {
            // Check whether it's a logged in User
            if (Request.IsAuthenticated && User.IsInRole("User"))
            {
                Session["LoggedInUser"] = "yes";
                //Debug.WriteLine("#1" + "---------- Page: Index: LoggedInUser? " 
                //    + Session["LoggedInUser"].ToString());
            }
            else
            {
                Session["LoggedInUser"] = "no";
                //Debug.WriteLine("#1" + "---------- Page: Index: LoggedInUser? " 
                //    + Session["LoggedInUser"].ToString());
                Session["CheckoutRequest"] = "no";
            }

            // Get/set page slug
            if (page == "")
                page = "home";

            // Declare model and DTO
            PageVM model;
            PageDTO dto;

            // Check if page exists
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                {
                    return RedirectToAction("Index", new { page = "" });
                }
            }

            // Get page DTO
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            // Set page title
            ViewBag.PageTitle = dto.Title;

            // Check for sidebar
            if (dto.HasSideBar == true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";
            }

            // Init model
            model = new PageVM(dto);

            // Return view with model
            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            List<PageVM> pageVMList;

            // Get all pages except home
            using (Db db = new Db())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Slug != "home").Select(x => new PageVM(x)).ToList();
            }
            // Return partial view with list
            return PartialView(pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            // Declare model
            SidebarVM model;

            // Init model
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);

                model = new SidebarVM(dto);
            }

            // Return partial view with model
            return PartialView(model);
        }
    }
}