using AssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;


namespace AssetManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }





        public IActionResult Index()
        {
            var currentlyCheckedOutAssetIds = _context.CheckedOutAssets
                .GroupBy(c => c.AssetID)
                .Where(g => g.OrderByDescending(c => c.DateLentOut).FirstOrDefault().DateReturned == null)
                .Select(g => g.Key)
                .ToList();

            var assetCount = _context.Assets
                .Include(a => a.Office)
                .GroupBy(a => a.Office.OfficeID)
                .Select(g => new
                {
                    OfficeName = $"{g.FirstOrDefault().Office.OfficeName} - {g.FirstOrDefault().Office.Location}",
                    AssetsCount = g.Count()
                })
                .ToList();

            ViewBag.assetCount = assetCount;

            ViewBag.phoneAvailability = CalculateAvailabilityPercentage("Phone", currentlyCheckedOutAssetIds);
            ViewBag.laptopAvailability = CalculateAvailabilityPercentage("Laptop", currentlyCheckedOutAssetIds);

            return View();
        }

        private double CalculateAvailabilityPercentage(string equipmentType, List<int> checkedOutAssetIds)
        {
            int totalAssets = _context.Assets.Count(a => a.EquipmentType == equipmentType);
            int availableAssets = _context.Assets
                .Where(a => a.EquipmentType == equipmentType && !checkedOutAssetIds.Contains(a.AssetID))
                .Count();

            return totalAssets > 0 ? (availableAssets / (double)totalAssets) * 100 : 0;
        }






        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

       

    }
}
