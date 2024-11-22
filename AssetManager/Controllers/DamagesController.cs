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
                DamageType = model.DamageType
            };

            _context.AssetDamages.Add(assetDamage);
            _context.SaveChanges();

            return RedirectToAction("Index");
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
            assetDamage.DamageType = model.DamageType;

            _context.SaveChanges();
            return RedirectToAction("Index");
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


        [HttpPost]
        public IActionResult BulkUpdate([FromBody] BulkUpdateModel model)
        {
            if (model == null || model.Ids == null || !model.Ids.Any())
            {
                return BadRequest("Invalid data.");
            }

            // Process each ID in the model
            foreach (var id in model.Ids)
            {
                var assetDamage = _context.AssetDamages.Find(id);
                if (assetDamage != null)
                {
                    assetDamage.RepairStatus = model.Status;
                    assetDamage.RepairDate = model.DateRepaired;
                    assetDamage.Notes = model.Notes;
                }
            }

            _context.SaveChanges();

            return Ok("Damages updated successfully.");
        }
        public class BulkUpdateModel
        {
            public List<int> Ids { get; set; }
            public string Status { get; set; }
            public DateTime? DateRepaired { get; set; }
            public string Notes { get; set; }
        }


        [HttpPost("Damages/DeleteDamage/{assetDamageID}")]
        public IActionResult DeleteDamage(int assetDamageID)
        {
            var damage = _context.AssetDamages.Find(assetDamageID);
            if (damage == null)
            {
                return NotFound(); // Return 404 if not found
            }

            _context.AssetDamages.Remove(damage);
            _context.SaveChanges();

            return Ok(); // Return success for AJAX
        }



    }
}
