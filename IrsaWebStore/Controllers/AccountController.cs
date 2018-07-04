using IrsaWebStore.Models.Data;
using IrsaWebStore.Models.ViewModel.Account;
using IrsaWebStore.Models.ViewModel.Shop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace IrsaWebStore.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        public ActionResult Login()
        {
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("user-profile");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if the user is valid

            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }
            else
            {
                if(Session["LoggedInUser"].ToString() != "yes")
                {
                    Session["LoggedInUser"] = "yes";
                    //Debug.WriteLine("#1" + "---------- Account: Login: LoggedInUser? "
                    //+ Session["LoggedInUser"].ToString());
                }

                if(Session["CheckoutRequest"].ToString() == "yes")
                {
                    Session["CheckoutRequest"] = "no";
                    //Debug.WriteLine("#1" + "---------- Account: Login: LoggedInUser2? "
                    //+ Session["LoggedInUser"].ToString());
                    //Debug.WriteLine("#1" + "---------- Account: Login: CheckoutRequest");
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    return Redirect("/cart");
                }

                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
            }
        }

        [ActionName("create-account")]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            // Check if passwords match
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                // Make sure username is unique
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", "Username " + model.Username + " is taken.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                // Create userDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    Username = model.Username,
                    Password = model.Password
                };

                // Add the DTO
                db.Users.Add(userDTO);

                // Save
                db.SaveChanges();

                // Add to UserRolesDTO
                int id = userDTO.Id;

                UserRoleDTO userRolesDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRolesDTO);
                db.SaveChanges();
            }

            // Create a TempData message
            TempData["SM"] = "You are now registered and can login.";

            // Redirect
            return Redirect("~/account/login");

        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            if(Session["LoggedInUser"].ToString() != "no")
            {
                Session["LoggedInUser"] = "no";
                //Debug.WriteLine("#1" + "---------- Account: Logout: LoggedInUser? "
                //    + Session["LoggedInUser"].ToString());
            }

            return Redirect("~/account/login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            string username = User.Identity.Name;
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }

            return PartialView(model);
        }

        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            string username = User.Identity.Name;
            UserProfileVM model;

            using (Db db = new Db())
            {
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                model = new UserProfileVM(dto);
            }

            return View("UserProfile", model);
        }

        [ActionName("user-profile")]
        [HttpPost]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                string username = User.Identity.Name;

                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == username))
                {
                    ModelState.AddModelError("", "Username " + model.Username + " already exists.");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                db.SaveChanges();
            }

            TempData["SM"] = "You have edited your profile!";

            return Redirect("~/account/user-profile");
        }

        //[Authorize(Roles = "User")]
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                UserDTO user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                foreach (var order in orders)
                {
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    decimal total = 0m;

                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        decimal price = product.Price;

                        string productName = product.Name;

                        productsAndQty.Add(productName, orderDetails.Quantity);

                        total += orderDetails.Quantity * price;
                    }

                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }

            }

            // Return view with list of OrdersForUserVM
            return View(ordersForUser);
        }
    }
}