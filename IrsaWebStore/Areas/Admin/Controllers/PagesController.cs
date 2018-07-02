using IrsaWebStore.Models.Data;
using IrsaWebStore.Models.ViewModel.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IrsaWebStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            // decalse list of pageVM
            List<PageVM> pagesList;

            // Init the list
            using (Db db = new Db())
            {
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(
                    x => new PageVM(x)).ToList();
            }

            // retunr view with list
            return View(pagesList);
        }

        // GET : Admin/Pages/AddPage
        public ActionResult AddPage()
        {
            return View();
        }

        // POST : Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                return View(model);

            }

            using (Db db = new Db())
            {
                // Declare slug
                string slug;
                // Init pageDTO
                PageDTO dto = new PageDTO();
                // DTO title
                dto.Title = model.Title;
                // Check for and set slug if need be
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }
                // Make sure title and slug are unique
                if (db.Pages.Any(x => x.Title == model.Title) 
                    || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists.");
                    return View(model);

                }
                // DTO reset
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSideBar = model.HasSideBar;
                dto.Sorting = 100;
                // Save DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            //Set Tempdata msg
            TempData["SM"] = "You have added a new page!";

            // Redirect

            return RedirectToAction("AddPage");
        }

        // GET : Admin/Pages/EditPage/id
        public ActionResult EditPage(int id)
        {
            // Declare pageVM
            PageVM model;
            using(Db db = new Db())
            {
                // Get the page
                PageDTO dto = db.Pages.Find(id);
                // Confirm page exists
                if (dto == null)
                {
                    return Content("The page does not exist.");

                }

                // Init pageVM
                model = new PageVM(dto);

            }


            // Return view with model 

            return View(model);
        }

        // POST : Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {   
            if (!ModelState.IsValid)
            {
                return View(model);

            }

            using (Db db = new Db())
            {
                int id = model.Id;
                string slug = "home"; // init
                PageDTO dto = db.Pages.Find(id);
                dto.Title = model.Title;

                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                if(db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) 
                    || db.Pages.Where(x => x.Id != id).Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists.");
                    return View(model);
                }

                // DTO rest
                dto.Slug = model.Slug;
                dto.Body = model.Body;
                dto.HasSideBar = model.HasSideBar;

                db.SaveChanges();
            }

            TempData["SM"] = "You have edited the page!";


            return RedirectToAction("EditPage");
        }

        // GET : Admin/Pages/PageDetails/id
        public ActionResult PageDetails(int id)
        {
            PageVM model;

            using (Db db = new Db())
            {
                PageDTO dto = db.Pages.Find(id);

                if ( dto == null)
                {
                    return Content("The page does not exist");
                }

                model = new PageVM(dto);

            }

                return View(model);
        }

        // GET : Admin/Pages/DeletePage/id
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                PageDTO dto = db.Pages.Find(id);

                db.Pages.Remove(dto);
                db.SaveChanges();
            }


                return RedirectToAction("Index");
        }

        // POST : Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                int count = 1;

                PageDTO dto;

                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();
                    count++;
                }
            }
        }

        // POST : Admin/Pages/EditSidebar
        public ActionResult EditSidebar()
        {
            SidebarVM model;

            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);
                model = new SidebarVM(dto);

            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);

                dto.Body = model.Body;
                db.SaveChanges();
            }
            TempData["SM"] = "You have edited the sidebar!";
            
            return RedirectToAction("EditSidebar");
        }
    }
}