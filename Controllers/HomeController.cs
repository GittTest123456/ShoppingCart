using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using ShoppingCart.Data;
using System;
using System.Text.Json;
using System.Xml.Linq;

namespace ShoppingCart.Controllers;

public class HomeController : Controller
{
    private Database db;

    public HomeController (IConfiguration cfg)
    {
        this.db = new Database(cfg.GetConnectionString("db_conn"));
    }

    public IActionResult Index(string searchStr)
    {
        string sessionId;
        if (Request.Cookies["SessionId"] == null)
        {
            sessionId = Guid.NewGuid().ToString();
            Response.Cookies.Append("SessionId", sessionId);
        } else
        {
            sessionId = Request.Cookies["SessionId"];
        }
        User user = db.GetUserBySession(sessionId);
        string username;
        bool isLoggedIn;
        if (user == null)
        {
            username = sessionId;
            isLoggedIn = false;
        }
        else
        {
            username = user.Username;
            isLoggedIn = true;
        }
        if (searchStr == null)
        {
            searchStr = "";
        }
        List<Product> products = db.SearchProduct(searchStr, searchStr);

        ViewData["searchStr"] = searchStr;
        ViewData["products"] = products;
        ViewData["username"] = username;
        List<ProductCart> cart = db.RetrieveCart(username);
        ViewData["Cart"] = cart;
        ViewData["isLoggedIn"] = isLoggedIn;
        return View();

    }

    public IActionResult AddtoCart(int ProductId, string username)
    {
        db.AddToCart(ProductId, username);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AddQuantity(int ProductId, string username)
    {
        db.AddQuantity(ProductId, username);
        return RedirectToAction("Cart", "Home");
    }
    public IActionResult DecreaseQuantity(int ProductId, string username)
    {
        db.DecreaseQuantity(ProductId, username);
        return RedirectToAction("Cart", "Home");
    }

    public IActionResult RemoveProduct(int ProductId, string username)
    {
        db.RemoveFromCart(ProductId, username);
        return RedirectToAction("Cart", "Home");
    }

    public IActionResult Cart(string username, bool isLoggedIn)
    {
        if (username == null)
        {
            return RedirectToAction("Index", "Login");
        }
        List<ProductCart> Cart = new List<ProductCart>();
        Cart = db.RetrieveCart(username);
        ViewData["username"] = username;
        ViewData["Cart"] = Cart;
        ViewData["isLoggedIn"] = isLoggedIn;
        return View();
    }

    // From Client
    public IActionResult Purchase(string username, bool isLoggedIn)
    {
        List<ProductCart> Cart = new List<ProductCart>();
        Cart = db.RetrieveCart(username);
        if (Cart.Count == 0)
        {
            return RedirectToAction("Cart", "Home");
        }

        if (isLoggedIn)
        {
            db.AddToPurchase(username, username);
            db.ClearCart(username);
            return RedirectToAction("ViewPurchase", "Home", new { username = username });
        }
        else
        {
            return RedirectToAction("Index", "Login");
        }
    }

    // From Server (redirected from the Home/Index page when GuestUser checkout cart and login to account)
    public IActionResult PurchaseAfterLogin(string username, string cartUser)
    {
        //add variables to addtopurchasemethod.
            db.AddToPurchase(username, cartUser);
        //clear cart from the fake user so next time if same person on the browser as fake user access the webpage of home bookmark, don't see the same cart,
            db.ClearCart(cartUser);
            return RedirectToAction("ViewPurchase", "Home", new { username = username });
    }

    public IActionResult ViewPurchase(string username)
    {
        IEnumerable<OrderDetails> Purchase = new List<OrderDetails>();
        Purchase = db.RetrievePurchase(username);
        ViewData["username"] = username;
        foreach (var order in Purchase)
        {
            if (!db.CheckPersonalRating(order.ProductId, username, order.PurchaseDate)){
                order.Rating = "0";
            }
            else
            {
                order.Rating = db.RetrievePersonalRating(order.ProductId, username, order.PurchaseDate);
            }
        }
        ViewData["Purchase"] = Purchase;
        return View();
    }

    public IActionResult SubmitRating(int rating, int ProductId, string username,string purchaseDate)
    {
        if (rating == 0)
        {
            return Content("User to try again");
        }
        db.AddIntoPersonalRating(rating,ProductId,username,purchaseDate);
        db.UpdateProductRating(ProductId);
        return RedirectToAction("ViewPurchase", "Home");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

