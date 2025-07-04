using Microsoft.AspNetCore.Mvc;
using CrudApp.Models;
using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CrudApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly string _connectionString;

        public UsersController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        private IDbConnection Connection => new SqlConnection(_connectionString);

        public IActionResult Index()
        {
            using var db = Connection;
            var users = db.Query<User>("SELECT * FROM Users").ToList();
            return View(users);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(User user)
        {
            using var db = Connection;
            db.Execute("INSERT INTO Users (Username, Email) VALUES (@Username, @Email)", user);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            using var db = Connection;
            var user = db.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = id });
            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User user)
        {
            using var db = Connection;
            db.Execute("UPDATE Users SET Username = @Username, Email = @Email WHERE Id = @Id", user);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            using var db = Connection;
            var user = db.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = id });
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            using var db = Connection;
            db.Execute("DELETE FROM Users WHERE Id = @Id", new { Id = id });
            return RedirectToAction(nameof(Index));
        }
    }

}
