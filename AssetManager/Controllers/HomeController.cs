using AssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DinkToPdf.Contracts;
using DinkToPdf;
using System.Text;


namespace AssetManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConverter _converter;


        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context, IConverter converter)
        {
            _converter = converter;
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
                    OfficeID = g.FirstOrDefault().Office.OfficeID,
                    AssetsCount = g.Count()
                })
                .ToList();

            ViewBag.totalAssets = assetCount.Sum(a => a.AssetsCount);

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



        public IActionResult Reports()
        {
            return View();
        }




    }
}
