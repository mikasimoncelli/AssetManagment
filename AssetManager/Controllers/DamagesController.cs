using AssetManager.Helpers;
using AssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.ContentModel;

namespace AssetManager.Controllers
{
    public class DamagesController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly ActivityLogger _activityLoggerService; // Define a private field for the logger

        public DamagesController(ApplicationDbContext context, ActivityLogger activityLoggerService)
        {
            _context = context;
            _activityLoggerService = activityLoggerService;  // Assign to the private variable
        }

        public IActionResult Index()
        {
            var damagedAssets = _context.AssetDamages
                                        .Include(ad => ad.Asset)
                                        .Where(ad => ad.Asset != null)
                                        .ToList(); 
            return View(damagedAssets);
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



        // post for asset damage form focus on asset
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

            // Log the activity
            _activityLoggerService.LogActivity(
                userId: GetCurrentUserId(),
                activityType: ActivityType.CreateDamage,
                description: $"Damage report created for AssetID: {model.AssetID} DamageID: {model.AssetDamageID}",
                createdAt: System.DateTime.Now

            );


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

            // Log the activity
            _activityLoggerService.LogActivity(
                userId: GetCurrentUserId(),
                activityType: ActivityType.EditDamage,
                description: $"Damage report updated for AssetID: {model.AssetID} DamageID: {model.AssetDamageID}",
                createdAt: System.DateTime.Now

            );

            return RedirectToAction("Index");
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
                    // Only update fields if provided in the request
                    if (!string.IsNullOrEmpty(model.Status))
                    {
                        assetDamage.RepairStatus = model.Status;
                    }

                    if (model.DateRepaired.HasValue)
                    {
                        assetDamage.RepairDate = model.DateRepaired;
                    }

                    // Append new notes only if provided, using the same logic as in Disposals
                    if (!string.IsNullOrEmpty(model.Notes))
                    {
                        assetDamage.Notes += $"\n{model.Notes}";
                    }
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

        public int GetCurrentUserId()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return 0;
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user != null)
            {
                return user.UserID;
            }

            return 0;
        }

    }
}
