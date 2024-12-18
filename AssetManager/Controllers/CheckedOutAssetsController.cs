

using AssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq;

namespace AssetManager.Controllers
{
    public class CheckedOutAssetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckedOutAssetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var checkedOutAssets = _context.CheckedOutAssets
                .Where(c => c.DateReturned == null)
                .Include(c => c.Asset)
                .Include(c => c.User)
                .OrderBy(c => c.DateLentOut)
                .ToList();


            ViewBag.LoansCount = checkedOutAssets.Count();
            ViewBag.checkedOutAssets = checkedOutAssets;

            return View();
        }




        public IActionResult ReturnedAssets()
        {
            var returnedCheckedOutAssets = _context.CheckedOutAssets
                .Where(c => c.DateReturned != null)
                .Include(c => c.Asset)
                .Include(c => c.User)
                .OrderBy(c => c.DateLentOut)
                .ToList();

            ViewBag.ReturnedLoansCount = returnedCheckedOutAssets.Count();
            ViewBag.returnedCheckedOutAssets = returnedCheckedOutAssets;

            return View();
        }




        
        public IActionResult ViewAvailableAssets()
        {
            var currentlyCheckedOutAssetIds = _context.CheckedOutAssets
                .GroupBy(c => c.AssetID)
                .Where(g => g.OrderByDescending(c => c.DateLentOut).FirstOrDefault().DateReturned == null) 
                .Select(g => g.Key)
                .ToList();

            var availableAssets = _context.Assets
                .Include(c => c.Office)
                .Where(a => !currentlyCheckedOutAssetIds.Contains(a.AssetID)) 
                .ToList();

            ViewBag.availableAssets = availableAssets;

            return View();
        }


        [HttpGet]
        public IActionResult AddNewCheckedOutAsset(int id)
        {
            var asset = _context.Assets
                .Include(a => a.Office)
                .FirstOrDefault(a => a.AssetID == id);

            ViewBag.users = _context.Users.ToList();

            return View(asset);
        }


        [HttpPost]
        public IActionResult AddNewCheckedOutAsset(int assetID, int userID, DateTime DueDate, string Notes)
        {
            var asset = _context.Assets.Find(assetID);
            var user = _context.Users.Find(userID);

            var checkedOutAsset = new CheckedOutAsset
            {
                AssetID = asset.AssetID,
                UserID = user.UserID,
                DateLentOut = System.DateTime.Now,
                DateReturned = null,
                DueDate = DueDate,
                Notes = Notes

            };

            _context.CheckedOutAssets.Add(checkedOutAsset);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }



        [HttpGet]
        public IActionResult EditCheckedOutAsset(int checkedoutid)
        {
            var asset = _context.CheckedOutAssets
                .Include(c => c.Asset)
                .Include(c => c.User)
                .Include(c=>c.Asset.Office)
           .FirstOrDefault(c => c.CheckedOutID == checkedoutid);

            return View(asset);

        }



        [HttpPost]
        public IActionResult EditCheckedOutAsset(int CheckedOutID, DateTime? DateReturned, DateTime? DueDate, string? Notes)
        {
            var asset = _context.CheckedOutAssets.FirstOrDefault(c => c.CheckedOutID == CheckedOutID);

            if(DueDate.HasValue)
            {
                asset.DueDate = DueDate;

            }
            if(DateReturned.HasValue)
            {
                asset.DateReturned = DateReturned;
            }

           
            asset.Notes = Notes;
            

            _context.SaveChanges();

            return RedirectToAction("Index");
        }





    }
}
