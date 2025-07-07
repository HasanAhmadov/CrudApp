using CrudApp.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CrudApp.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly string _connectionString;

        public ProductsController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        private IDbConnection Connection => new SqlConnection(_connectionString);

        public IActionResult Index()
        {
            using var db = Connection;
            var products = db.Query<Product>("SELECT * FROM Products").ToList();
            return View(products);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(Product product)
        {
            using var db = Connection;
            db.Execute("INSERT INTO Products (Name, Price) VALUES (@Name, @Price)", product);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            using var db = Connection;
            var product = db.QueryFirstOrDefault<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = id });
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            using var db = Connection;
            db.Execute("UPDATE Products SET Name = @Name, Price = @Price WHERE Id = @Id", product);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            using var db = Connection;
            var product = db.QueryFirstOrDefault<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = id });
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            using var db = Connection;
            db.Execute("DELETE FROM Products WHERE Id = @Id", new { Id = id });
            return RedirectToAction(nameof(Index));
        }

    }
}
