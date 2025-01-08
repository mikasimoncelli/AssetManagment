using AssetManager.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourProjectNamespace.Helpers;

namespace AssetManager.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            
            var users = _context.Users.ToList();

           
            return View(users);
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddUser(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(user);
        }


        [SessionAuthorize]
        public IActionResult EditUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }



        [HttpPost]
        [SessionAuthorize]
        public IActionResult UpdateUser(User user)
        {
            var existingUser = _context.Users.Find(user.UserID);
            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        [HttpPost]
        [SessionAuthorize]
        public IActionResult DeleteUser(int userID)
        {
            var user = _context.Users.Find(userID);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }



        public IActionResult ViewUser(int id)
        {
            // Fetch the user by ID
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            // Find all checked-out assets for the user
            var checkedOutAssets = _context.CheckedOutAssets
                .Include(c => c.Asset) // Include related asset details
                .Where(c => c.UserID == id)
                .ToList();

            // Separate current loans and previous loans
            ViewData["CurrentLoans"] = checkedOutAssets.Where(c => c.DateReturned == null).ToList();
            ViewData["PreviousLoans"] = checkedOutAssets.Where(c => c.DateReturned != null).ToList();

            return View(user);
        }



        [HttpGet]
        public JsonResult CheckUserReferences(int userID)
        {
            var checkedOutAsset = _context.CheckedOutAssets
                .Include(c => c.Asset)
                .FirstOrDefault(c => c.UserID == userID);

            if (checkedOutAsset != null)
            {
                return Json(new
                {
                    hasReferences = true,
                    checkedOutID = checkedOutAsset.CheckedOutID,
                    assetDescription = checkedOutAsset.Asset?.Description ?? "Unknown",
                    checkedOutDate = checkedOutAsset.DateLentOut.ToString("dd-MM-yyyy") ?? "N/A"
                });
            }

            return Json(new { hasReferences = false });
        }





    }



}
