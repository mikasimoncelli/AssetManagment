using AssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManager.Controllers
{
    public class DamagesController : Controller
    {

        private readonly ApplicationDbContext _context;

        public IActionResult Index()
        {
            var damagedAssets = _context.AssetDamages.Include(ad => ad.Asset).ToList();
            return View(damagedAssets);
        }


        public DamagesController(ApplicationDbContext context)
        {
            _context = context;
        }



        // asset damage form focus on asset
        [HttpGet]
        public IActionResult AssetDamagesForm(int assetID)
        {
            var asset = _context.Assets
                .Include(a => a.Office)
                .FirstOrDefault(a => a.AssetID == assetID);


            return View(asset);
        }



        // post for assetr damage form focus on asset
        [HttpPost]
        public IActionResult AssetDamagesForm(AssetDamage model)
        {
            var assetDamage = new AssetDamage
            {
                AssetID = model.AssetID,
                DamageDescription = model.DamageDescription,
                Notes = model.Notes,
                RepairStatus = model.RepairStatus,
            };

            _context.AssetDamages.Add(assetDamage);
            _context.SaveChanges();

            return RedirectToAction("AllDamagedAssets");
        }


        // edit
        public IActionResult EditAssetDamage(int id)
        {
            var assetDamage = _context.AssetDamages.Include(a => a.Asset).Include(a => a.Asset.Office).FirstOrDefault(a => a.AssetDamageID == id);
            if (assetDamage == null)
            {
                return NotFound();
            }
            return View(assetDamage);
        }



        // edit
        [HttpPost]
        public IActionResult EditAssetDamage(AssetDamage model)
        {
            var assetDamage = _context.AssetDamages.Find(model.AssetDamageID);
            if (assetDamage == null)
            {
                return NotFound();
            }

            assetDamage.DamageDescription = model.DamageDescription;
            assetDamage.RepairStatus = model.RepairStatus;
            assetDamage.RepairDate = model.RepairDate;
            assetDamage.Notes = model.Notes;

            _context.SaveChanges();
            return RedirectToAction("AllDamagedAssets");
        }


        // add new damage report
        [HttpGet]
        public IActionResult ReportNewDamage()
        {
            var assets = _context.Assets.Include(a => a.Office).ToList();
            var equipmentTypes = assets.Select(a => a.EquipmentType).Distinct().ToList();
            var offices = assets.Select(a => a.Office.OfficeName).Distinct().ToList();

            ViewBag.EquipmentTypes = equipmentTypes;
            ViewBag.Offices = offices;

            return View(assets);
        }





    }
}
