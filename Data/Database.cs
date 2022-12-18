using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting.Server;
using ShoppingCart.Models;

namespace ShoppingCart.Data
{
    public class Database
    {
        private string connStr;

        public Database(string connStr)
        {
            this.connStr = connStr;
        }

        public User GetUserBySession(string sessionId)
        {
            User user = null;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT u.Username, Password
             FROM Session s, [User] u
                WHERE s.Username = u.Username AND 
                    s.SessionId = '{0}'", sessionId);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Username = reader.GetString(0),
                                Password = reader.GetString(1)
                            };
                        }
                    }
                }

                conn.Close();
            }

            return user;
        }

        public User GetUserByUsername(string username)
        {
            User user = null;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT * FROM [User] 
                WHERE Username = '{0}'", username);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Username = reader.GetString(0),
                                Password = reader.GetString(1)
                            };
                        }
                    }
                }

                conn.Close();
            }

            return user;
        }

        public string AddSession(string username, Guid guid)
        {
            string sessionId = null;


            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"INSERT INTO Session(
                SessionId, Username, Timestamp) VALUES('{0}', '{1}', {2})",
                        guid, username, DateTimeOffset.Now.ToUnixTimeSeconds());

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        sessionId = guid.ToString();
                    }
                }

                conn.Close();
            }

            return sessionId;
        }

        public bool RemoveSession(string sessionId)
        {
            bool status = false;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"DELETE FROM Session
                WHERE sessionId = '{0}'", sessionId);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        status = true;
                    }
                }

                conn.Close();
            }

            return status;
        }

        public List<Product> SearchProduct(string Name,
       string Description)
        {
            List<Product> products = new List<Product>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = string.Format(@"SELECT * FROM Product WHERE 
                [Name] LIKE '%{0}%' OR 
                [Description] LIKE '%{1}%'",
                        Name, Description);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Product product = new Product { };
                            product.ProductId = reader.GetInt32(0);
                            product.Name = reader.GetString(1);
                            product.Price = reader.GetInt32(2);
                            product.Description = reader.GetString(3);

                            if (reader.IsDBNull(4))
                            {
                                product.AvgRating = "0";
                            }
                            else
                            {
                                product.AvgRating = reader.GetInt32(4).ToString();
                            };

                            products.Add(product);
                        }
                    }
                }

                conn.Close();
            }

            return products;
        }

        public bool AddCart(int ProductId, string username)
        {
            bool status = false;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"INSERT INTO Cart VALUES('{0}', {1}, {2})",
                        username, ProductId, 1);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        status = true;
                    }
                }

                conn.Close();
            }
            return status;
        }

        public bool RemoveFromCart(int ProductId, string username)
        {

            bool status = false;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"DELETE FROM Cart
                WHERE ProductId = {0} AND Username = '{1}'", ProductId, username);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        status = true;
                    }
                }

                conn.Close();
            }

            return status;
        }


        public bool AddToCart(int ProductId, string username)
        {

            int Quantity = 0;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT Quantity FROM Cart WHERE ProductId = {0} AND Username = '{1}' ",
                        ProductId, username);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Quantity = reader.GetInt32(0);

                        }
                    }
                }

                conn.Close();
            }
            if (Quantity == 0)
            {
                if (AddCart(ProductId, username))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (AddQuantity(ProductId, username))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }


        public bool AddQuantity(int ProductId, string username)
        {
            bool status = false;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"UPDATE Cart SET Quantity= Quantity+1 WHERE Username= '{0}' AND ProductId = {1}",
                        username, ProductId);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        status = true;
                    }
                }

                conn.Close();
            }
            return status;

        }

        public bool DecreaseQuantity(int ProductId, string username)
        {
            bool status = false;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"UPDATE Cart SET Quantity= Quantity-1 WHERE Username= '{0}' AND ProductId = {1}",
                        username, ProductId);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        status = true;
                    }
                }

                conn.Close();
            }
            return status;

        }

        public List<ProductCart> RetrieveCart(string username)
        {
            List<ProductCart> Cart = new List<ProductCart>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT Cart.ProductId, Name, Price, Description, Quantity FROM Cart, Product WHERE Cart.ProductId = Product.ProductId AND Username = '{0}'",
                        username);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProductCart product = new ProductCart
                            {
                                ProductId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Price = reader.GetInt32(2),
                                Description = reader.GetString(3),
                                Quantity = reader.GetInt32(4)
                            };
                            Cart.Add(product);
                        }
                    }
                }
                conn.Close();

            }
            return Cart;
        }

        public IEnumerable<OrderDetails> RetrievePurchase(string username)
        {
            Dictionary<int, List<string>> ProductToDate = ProductToDates(username);
            List<OrderDetails> OrderInfo = new List<OrderDetails>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                foreach (var dictionary in ProductToDate)
                {
                    foreach (var date in dictionary.Value)
                    {
                        string q = String.Format(@"SELECT ActivationCode,PurchaseDate, Name, Description,OrderDetails.ProductId FROM OrderList, OrderDetails, Product WHERE OrderList.OrderId = OrderDetails.OrderId AND Username= '{0}'AND OrderDetails.ProductID = Product.ProductID AND OrderDetails.ProductID = {1} AND OrderList.PurchaseDate = '{2}'",
                   username, dictionary.Key, date);
                        List<Guid> Codes = new List<Guid>();

                        using (SqlCommand cmd = new SqlCommand(q, conn))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                OrderDetails OrderProduct = new OrderDetails { Quantity = 0, ActivationCode = Codes };

                                while (reader.Read())
                                {
                                    Codes.Add(reader.GetGuid(0));
                                    OrderProduct.PurchaseDate = reader.GetString(1);
                                    OrderProduct.Name = reader.GetString(2);
                                    OrderProduct.Description = reader.GetString(3);
                                    OrderProduct.ProductId = reader.GetInt32(4);

                                }

                                OrderInfo.Add(OrderProduct);

                            }

                        }

                    }

                }
                conn.Close();
            }
            IEnumerable<OrderDetails> OrderInformation = from order in OrderInfo orderby order.PurchaseDate descending select order;
            return OrderInformation;

        }


        public bool AddToPurchase(string username, string cartUser)
        {
            bool status = false;
            Guid orderId = Guid.NewGuid();
            string purchasedate = DateTime.Today.ToString("dd MMMM yyyy");
            Dictionary<int, int> ProductToQuantity = QuantityPerProduct(cartUser);
            //to retrieve the cart-> each product's quantity from the GuestUser side. before can insert to orderdetails of actual username.
            //insert into orderlist-> need insert into correct/actual username.
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"INSERT INTO OrderList (OrderId, Username, PurchaseDate) VALUES('{0}','{1}','{2}')",
                        orderId, username, purchasedate);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        status = true;
                    }
                }
                foreach (var dictionary in ProductToQuantity)
                {
                    var Quantity = dictionary.Value;

                    for (int i = 0; i < Quantity; i++)
                    {
                        string j = String.Format(@"INSERT INTO OrderDetails(ActivationCode, OrderId, ProductId) VALUES('{0}','{1}',{2})",
                        Guid.NewGuid(), orderId, dictionary.Key);
                        using (SqlCommand cmd = new SqlCommand(j, conn))
                        {
                            if (cmd.ExecuteNonQuery() == 1)
                            {
                                status = true;
                            }
                        }
                    }
                }

                conn.Close();
            }


            return status;

        }

        public Dictionary<int, int> QuantityPerProduct(string username)
        {
            Dictionary<int, int> ProductToQuantity = new Dictionary<int, int>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT ProductId,Quantity FROM Cart
                WHERE Username = '{0}'", username);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProductToQuantity.Add(reader.GetInt32(0), reader.GetInt32(1));

                        }
                    }
                }

                conn.Close();
            }
            return ProductToQuantity;


        }

        public Dictionary<int, List<string>> ProductToDates(string username)
        {
            Dictionary<int, List<string>> ProductToDates = new Dictionary<int, List<string>>();
            List<string> dates = new List<string>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT ProductId,PurchaseDate FROM OrderDetails, OrderList
                WHERE OrderDetails.OrderId = OrderList.OrderId and Username = '{0}'GROUP BY PurchaseDate,ProductId", username);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (ProductToDates.ContainsKey(reader.GetInt32(0)))
                            {
                                List<string> values = ProductToDates[reader.GetInt32(0)];
                                values.Add(reader.GetString(1));

                            }
                            else
                            {

                                ProductToDates.Add((reader.GetInt32(0)), new List<string>() { reader.GetString(1) });

                            }


                        }
                    }
                }

                conn.Close();
            }
            return ProductToDates;


        }

        public bool ClearCart(string username)
        {
            bool status = false;
            List<ProductCart> Cart = new List<ProductCart>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"DELETE FROM Cart
                WHERE Username = '{0}'", username);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    cmd.ExecuteNonQuery();

                }

                Cart = RetrieveCart(username);
                if (Cart.Count == 0)
                {
                    return true;
                }
                conn.Close();
            }

            return status;
        }

        public bool AddIntoPersonalRating(int Rating, int ProductId, string username, string purchaseDate)
        {
            bool NeedUpdate = CheckPersonalRating(ProductId, username, purchaseDate);
            bool status = false;
            if (NeedUpdate)
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string q = String.Format(@"UPDATE PersonalRating SET Rating={0} WHERE Username ='{1}'AND PurchaseDate='{2}'AND ProductId ={3}", Rating, username, purchaseDate, ProductId);

                    using (SqlCommand cmd = new SqlCommand(q, conn))
                    {
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            status = true;
                        }
                    }

                    conn.Close();
                }
                return status;

            }
            else {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string q = String.Format(@"INSERT INTO PersonalRating VALUES('{0}',{1},{2},'{3}')", purchaseDate, ProductId, Rating, username);

                    using (SqlCommand cmd = new SqlCommand(q, conn))
                    {
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            status = true;
                        }
                    }

                    conn.Close();
                }
                return status;
            }

        }

        public bool CheckPersonalRating(int ProductId, string username, string purchaseDate)
        {
            bool status = false;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT * FROM PersonalRating
                WHERE Username = '{0}' AND ProductId = {1} AND PurchaseDate = '{2}'", username, ProductId, purchaseDate);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            status = true;
                        }
                        else
                        {
                            status = false;
                        }
                    }
                }
                conn.Close();
            }
            return status;
        }

        public Dictionary<int, int> RetrieveAverageRating(int ProductId)
        {
            Dictionary<int, int> ProductToRating = new Dictionary<int, int>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT ProductId,AVG(Rating) FROM PersonalRating GROUP BY ProductId");

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProductToRating.Add(reader.GetInt32(0), reader.GetInt32(1));

                        }
                    }
                }

                conn.Close();
            }
            return ProductToRating;

        }

        public bool UpdateProductRating(int ProductId)
        {
            bool status = false;
            Dictionary<int, int> ProductToRating = RetrieveAverageRating(ProductId);
            foreach (var dictionary in ProductToRating)
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string q = String.Format(@"UPDATE Product SET AvgRating = {0} WHERE ProductId ={1}", dictionary.Value, dictionary.Key);

                    using (SqlCommand cmd = new SqlCommand(q, conn))
                    {
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            status = true;
                        }
                    }

                    conn.Close();
                }

            }
            return status;
        }

        public string RetrievePersonalRating(int ProductId, string username, string purchaseDate)
        {
            string rating = "0";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT Rating FROM PersonalRating
                WHERE Username = '{0}' AND ProductId = {1} AND PurchaseDate = '{2}'", username, ProductId, purchaseDate);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            rating = reader.GetInt32(0).ToString();
                        }
                    }
                }
                conn.Close();
            }
            return rating;
        }


    }
    }





