﻿using IrsaWebStore.Models.Data;
using IrsaWebStore.Models.ViewModel.Shop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Web;
using System.Web.Mvc;

namespace IrsaWebStore.Controllers
{

    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            List<CategoryVM> categoryVMList;

            // Init the list
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            // Return partial with list
            return PartialView(categoryVMList);
        }

        

        public ActionResult SearchList(string search)
        {
            List<ProductVM> productVMList = new List<ProductVM>();
            List<ProductVM> searchByNames = new List<ProductVM>();
            List<ProductVM> searchByDescription = new List<ProductVM>();
            List<ProductVM> searchByCategories = new List<ProductVM>();

            using (Db db = new Db())
            {
                searchByNames = db.Products
                                  .ToArray()
                                  .Where(x => x.Name.ToLower().Contains(search.ToLower()))
                                  .Select(x => new ProductVM(x))
                                  .ToList();
                searchByDescription = db.Products
                                        .ToArray()
                                        .Where(x => x.Description.ToLower().Contains(search.ToLower()))
                                        .Select(x => new ProductVM(x))
                                        .ToList();
                searchByCategories = db.Products
                                       .ToArray()
                                       .Where(x => x.CategoryName.ToLower().Contains(search.ToLower()))
                                       .Select(x => new ProductVM(x))
                                       .ToList();

                productVMList.AddRange(searchByNames);
                productVMList.AddRange(searchByDescription.Where(x => !productVMList.Select(y => y.Id).Contains(x.Id)));
                productVMList.AddRange(searchByCategories.Where(x => !productVMList.Select(y => y.Id).Contains(x.Id)));

                ViewBag.SearchKeyword = search;

            }

                return View(productVMList);
        }

        public ActionResult Category(string name)
        {
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;
                var productCat = categoryDTO.Name;
                var productSlug = categoryDTO.Slug;

                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                ViewBag.CategoryName = productCat;

                var culture1 = new System.Globalization.CultureInfo(Request.Cookies["Language"].Value);
                ViewBag.CategoryName2 = Resources.GlobalRes.ResourceManager.GetString(productSlug, culture1); ;

                // var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                // ViewBag.CategoryName = productCat.CategoryName;

            }

                return View(productVMList);
        }

        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            // Declare the VM and DTO
            ProductVM model;
            ProductDTO dto;

            // Init product id
            int id = 0;

            using (Db db = new Db())
            {
                // Check if product exists
                if (!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                // Init productDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                // Get id
                id = dto.Id;

                model = new ProductVM(dto);
            }

            // Get gallery images
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                                .Select(fn => Path.GetFileName(fn));

            return View("ProductDetails", model);
        }
    }
}