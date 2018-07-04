using IrsaWebStore.Models.Data;
using IrsaWebStore.Models.ViewModel.Cart;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace IrsaWebStore.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //Debug.WriteLine("#1" + "---------- Cart: Index: LoggedInUser? "
            //        + Session["LoggedInUser"].ToString());

            // Init the cart list

            if (!Request.IsAuthenticated)
            {
                Session["CheckoutRequest"] = "yes";
                return Redirect("/account/login");
            }

            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }

            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            return View(cart);
        }

        public ActionResult CartPartial()
        {
            CartVM model = new CartVM();

            int qty = 0;

            decimal price = 0m;

            // Check for cart session
            if (Session["cart"] != null)
            {
                // Get total qty and price
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;

            }
            else
            {
                model.Quantity = 0;
                model.Price = 0m;
            }

            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            // Init CartVM list
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                ProductDTO product = db.Products.Find(id);

                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                // If not, add new
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                else
                {
                    // If it is, increment
                    productInCart.Quantity++;
                }
            }

            // Get total qty and price and add to model

            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            // Save cart back to session
            Session["cart"] = cart;

            // Return partial view with model
            return PartialView(model);
        }

        public JsonResult IncrementProduct(int productId)
        {
            Debug.WriteLine("Yoyo ++");
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Get cartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Increment qty
                model.Quantity++;

                // Store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                // Return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult DecrementProduct(int productId)
        {
            // Init cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Get model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Decrement qty
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                // Store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                // Return json
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
        public void RemoveProduct(int productId)
        {
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Get model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Remove model from list
                cart.Remove(model);
            }

        }

        public ActionResult PaypalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            return PartialView(cart);
        }

        [HttpPost]
        public void PlaceOrder()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            Debug.WriteLine("Yoyo");

            string username = User.Identity.Name;

            int orderId = 0;

            using (Db db = new Db())
            {
                OrderDTO orderDTO = new OrderDTO();

                // Get user id
                var q = db.Users.FirstOrDefault(x => x.Username == username);
                int userId = q.Id;

                // Add to OrderDTO and save
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);

                db.SaveChanges();

                // Get inserted id
                orderId = orderDTO.OrderId;

                // Init OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                // Add to OrderDetailsDTO
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();
                }
            }

            // Email admin
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("018cb3cf5ec5ba", "01458da0dc47bf"),
                EnableSsl = true
            };
            client.Send("admin@example.com", "admin@example.com", "New Order", "You have a new order. Order number " + orderId);
            Debug.WriteLine("Sent");
            
            // Reset session
            Session["cart"] = null;
        }

    }
}