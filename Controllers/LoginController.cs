using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using ShoppingCart.Data;
using System.Data.SqlTypes;


namespace ShoppingCart.Controllers
{
    public class LoginController : Controller
    {
        private Database db;

        public LoginController(IConfiguration cfg)
        {
            db = new Database(cfg.GetConnectionString("db_conn"));
        }

        public IActionResult Index(IFormCollection form)
        {
            Guid sessionId = Guid.NewGuid();
            //usually better for controller to generate Guid (sessionId) so can decide if database saves the Guid or not to help with actual users verification.

            if (Request.Cookies["SessionId"] == null || db.GetUserBySession(Request.Cookies["SessionId"]) == null)
            {
                string username = form["username"];
                string password = form["password"];

                if (username == null && password == null)
                {
                    return View();
                }
                else
                {
                    User user = db.GetUserByUsername(username);
                    if (user != null)
                    {

                        if (user.Password == password)
                        {
                            
                            //only after http response/IAction Result back, then the client will append the cookies.
                            //FYI: the value of the cookie-key will be updated if same key: SessionId sent back by server.
                            //session storage e.g. is always dictionary that keeps string key and string values

                            // Get Cart from Session
                            string maybeGuestUser = Request.Cookies["SessionId"];
                            //cannot just check if maybeGuestUser null directly as some users may have cookies not cleared after logout (some disable function) and then cookies not associated with the user, hence not with a cart at all.
                            //*Guid.Parse convert string back to Guid
                            //this string maybeGuestUser will have the fake username if its not loggedin user
                            //this string will be null if its a user who just wants to login and not redirected from cart by fake username. but the retrieve cart SQL statement can query WHERE null, just that retrieval
                            //of 0 rows and since list is already initiated at the start, it will be an empty list.
                            //meaning normal users who login will be directed to the same path as usual- home index. as normal users have no shopping cart under guestuser/without login in
                            List<ProductCart> cart = db.RetrieveCart(maybeGuestUser);
                            if (cart.Count == 0)
                            {
                                db.AddSession(user.Username, sessionId);
                                Response.Cookies.Append("SessionId", sessionId.ToString());
                                return RedirectToAction("Index", "Home");
                            }
                            else
                            {
                                db.AddSession(user.Username, Guid.Parse(maybeGuestUser));

                            }
                            //if it is a GuestUser, the count of cart should be > 0, then redirect to method: purchase after login, pass in two values: real username and fakeusername.
                            //
                            //if cart has something, call purchase
                            //linking the fake username and real username together- associate them together.

                            return RedirectToAction("PurchaseAfterLogin", "Home", new { username = username, cartUser = maybeGuestUser });

                        }
                        else
                        {
                            ViewData["message"] = "Your username or password is incorrect. Please try again. ";
                            return View();
                        }

                    }
                    else
                    {

                        ViewData["message"] = "Your username or password is incorrect. Please try again. ";
                        return View();
                    }
                }
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }

           
        }

    }
}

