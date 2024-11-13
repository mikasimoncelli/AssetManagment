using AssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManager.Controllers
{
    public class OfficesController : Controller
    {

        private readonly ApplicationDbContext _context;

        public OfficesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var offices = _context.Offices
                .Select(o => new
                {
                    o.OfficeID,
                    o.OfficeName,
                    o.Location,
                    AssetCount = _context.Assets.Count(a => a.OfficeID == o.OfficeID) 
                })
                .ToList();
            ViewBag.TotalOffices = offices.Count; 

            ViewBag.Offices = offices;
            return View();
        }


        [Route("Assets/Office/{officeID}")]
        public IActionResult ViewOfficeAssets(int officeID)
        {
            var assets = _context.Assets
                .Include(a=>a.Office)
                .Where(c => c.OfficeID == officeID)
                .ToList();

            ViewBag.AssetTypes = assets.Select(a => a.EquipmentType).Distinct().ToList();
            var office = _context.Offices.FirstOrDefault(o => o.OfficeID == officeID);

            ViewBag.Office = office;
            return View(assets);
        }

    }
}
