using CrudApp.Models;
using CrudApp.ViewModels;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace CrudApp.Controllers
{
    [Authorize(Roles ="Admin, Accountant, Cashier")]
    public class OrdersController : Controller
    {
        private readonly IDbConnection _db;

        public OrdersController(IDbConnection db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var sql = @"
        SELECT o.*, u.*
        FROM Orders o
        JOIN Users u ON o.UserId = u.Id";

            var orders = _db.Query<Order, User, Order>(sql, (order, user) =>
            {
                order.User = user;
                return order;
            }).ToList();

            return View(orders);
        }

        public IActionResult Details(int id)
        {
            var sql = @"
            SELECT o.Id, o.OrderDate, u.Id, u.Username,
                   p.Id, p.Name, p.Price, oi.Quantity
            FROM Orders o
            JOIN Users u ON o.UserId = u.Id
            JOIN OrderItems oi ON o.Id = oi.OrderId
            JOIN Products p ON oi.ProductId = p.Id
            WHERE o.Id = @Id";

            var orderLookup = new Dictionary<int, Order>();

            _db.Query<Order, User, Product, OrderItem, Order>(
                sql,
                (order, user, product, item) =>
                {
                    if (!orderLookup.TryGetValue(order.Id, out var ord))
                    {
                        ord = order;
                        ord.User = user;
                        ord.Items = new List<OrderItem>();
                        orderLookup.Add(ord.Id, ord);
                    }

                    item.Product = product;
                    ord.Items.Add(item);
                    return ord;
                },
                new { Id = id },
                splitOn: "Id,Id,Quantity"
            );

            var orderResult = orderLookup.Values.FirstOrDefault();
            return View(orderResult);
        }

        public IActionResult Create()
        {
            ViewBag.Users = _db.Query<User>("SELECT * FROM Users").ToList();
            ViewBag.Products = _db.Query<Product>("SELECT * FROM Products").ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(CreateOrderViewModel vm)
        {
            var orderId = _db.ExecuteScalar<int>(
                @"INSERT INTO Orders (UserId, OrderDate) VALUES (@UserId, GETDATE());
              SELECT CAST(SCOPE_IDENTITY() as int);",
                new { vm.UserId });

            foreach (var item in vm.Products)
            {
                _db.Execute(
                    @"INSERT INTO OrderItems (OrderId, ProductId, Quantity)
                  VALUES (@OrderId, @ProductId, @Quantity)",
                    new { OrderId = orderId, item.ProductId, item.Quantity });
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var order = _db.QueryFirstOrDefault<Order>("SELECT * FROM Orders WHERE Id = @Id", new { Id = id });
            if (order == null) return NotFound();

            ViewBag.Users = _db.Query<User>("SELECT * FROM Users").ToList();
            return View(order);
        }

        [HttpPost]
        public IActionResult Edit(Order order)
        {
            _db.Execute("UPDATE Orders SET UserId = @UserId, OrderDate = @OrderDate WHERE Id = @Id", order);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var sql = @"
        SELECT o.*, u.*
        FROM Orders o
        JOIN Users u ON o.UserId = u.Id
        WHERE o.Id = @Id";

            var order = _db.Query<Order, User, Order>(sql, (order, user) =>
            {
                order.User = user;
                return order;
            }, new { Id = id }).FirstOrDefault();

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _db.Execute("DELETE FROM OrderItems WHERE OrderId = @Id", new { Id = id });
            _db.Execute("DELETE FROM Orders WHERE Id = @Id", new { Id = id });

            return RedirectToAction("Index");
        }
    }
}
