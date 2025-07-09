using Microsoft.AspNetCore.Mvc;
using CrudApp.Models;
using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace CrudApp.Controllers
{
    [Authorize(Roles ="Admin")]
    public class UsersController(IConfiguration config) : Controller
    {
        private readonly string? _connectionString = config.GetConnectionString("DefaultConnection");

        private IDbConnection Connection => new SqlConnection(_connectionString);

        public IActionResult Index()
        {
            using var db = Connection;
            var users = db.Query<User>(@"
                                       SELECT  u.Id, u.Username, u.Password, r.Name as Role
                                       FROM Users u
                                       JOIN Roles r on u.Role = r.Id").ToList();
            return View(users);
        }

        public IActionResult Create()
        {
            using var db = Connection;
            var roles = db.Query<string>("SELECT Name FROM Roles").ToList();
            ViewBag.Roles = roles;
            return View();
        }

        [HttpPost]
        public IActionResult Create(User user)
        {
            using var db = Connection;
            if (ModelState.IsValid)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password, workFactor: 12);

    
                db.Execute(@"
            INSERT INTO Users (Username, Password, Role)
            SELECT @Username, @Password, Id
            FROM Roles
            WHERE Name = @Role",
                new
                {
                    user.Username,
                    user.Password,
                    user.Role
                });

                return RedirectToAction(nameof(Index));
            }

            var roles = db.Query<string>("SELECT Name FROM Roles").ToList();
            ViewBag.Roles = roles;
            return View(user);
        }

        public IActionResult Edit(int id)
        {
            using var db = Connection;
            var roles = db.Query<string>("SELECT Name FROM Roles").ToList();
            ViewBag.Roles = roles;
            var user = db.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = id });
            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User user)
        {
            using var db = Connection;
            db.Execute(@"
    UPDATE Users
    SET
        Username = @Username,
        Password = @Password,
        Role = Roles.Id
    FROM Roles
    WHERE Roles.Name = @Role AND Users.Id = @Id", user);

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

            var orderCount = db.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM Orders WHERE UserId = @Id", new { Id = id });

            if (orderCount > 0)
            {
                TempData["Error"] = "❌ Cannot delete user because they are linked to one or more orders.";
                return RedirectToAction(nameof(Index));
            }

            db.Execute("DELETE FROM Users WHERE Id = @Id", new { Id = id });
            return RedirectToAction(nameof(Index));
        }
    }
}