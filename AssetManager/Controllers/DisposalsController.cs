using AssetManager.Helpers;
using AssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using NuGet.ContentModel;
using YourProjectNamespace.Helpers;

namespace AssetManager.Controllers
{
    public class DisposalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogger _activityLoggerService; // Define a private field for the logger
        public DisposalsController(ApplicationDbContext context, ActivityLogger activityLoggerService)
        {
            _context = context;
            _activityLoggerService = activityLoggerService;  // Assign to the private variable
        }

        public IActionResult Index()
        {
            var disposals = _context.AssetDisposals.Include(a => a.Asset).ToList();
            return View(disposals);
        }

        [SessionAuthorize]
        public IActionResult ReportNewDisposal()
        {
            var assets = _context.Assets
                 .Include(a => a.Office)
                 .Where(a => !_context.AssetDisposals.Any(ad => ad.AssetID == a.AssetID))
                 .ToList(); 
            var equipmentTypes = assets.Select(a => a.EquipmentType).Distinct().ToList();
            var offices = assets.Select(a => a.Office.OfficeName).Distinct().ToList();

            ViewBag.EquipmentTypes = equipmentTypes;
            ViewBag.Offices = offices;

            return View(assets);
        }


        [HttpGet]
        [SessionAuthorize]
        public IActionResult AssetDisposalsForm(int assetID)
        {
            var asset = _context.Assets
            .Include(a => a.Office)
            .FirstOrDefault(a => a.AssetID == assetID);
            return View(asset);

        }



        [HttpPost]
        [SessionAuthorize]
        public IActionResult AssetDisposalForm(AssetDisposal model)
        {
           

            var assetDisposal = new AssetDisposal
            {
                AssetID = model.AssetID,
                DisposalDescription = model.DisposalDescription,
                DisposalReason = model.DisposalReason,
                Notes = model.Notes,
                DisposalStatus = model.DisposalStatus,
                DateDisposed = model.DateDisposed


            };

            _context.AssetDisposals.Add(assetDisposal);
            _context.SaveChanges();

            // Log the activity
            _activityLoggerService.LogActivity(
                userId: GetCurrentUserId(),
                activityType: ActivityType.CreateDisposal,
                description: $"Disposal report created for AssetID: {model.AssetID} DamageID: {model.AssetDisposalID}",
                createdAt: System.DateTime.Now

            );

            return RedirectToAction("Index");

        }


        [HttpGet]        
        [SessionAuthorize]
        public IActionResult EditDisposal(int id)
        {
            var assetDisposal = _context.AssetDisposals
                .Include(ad => ad.Asset) 
                .Include(a => a.Asset.Office)
                .FirstOrDefault(ad => ad.AssetDisposalID == id);

            if (assetDisposal == null)
            {
                return NotFound();
            }

            return View(assetDisposal);
        }

        [HttpPost]
        [SessionAuthorize]
        public IActionResult EditDisposal(AssetDisposal model)
        {
            var assetDisposal = _context.AssetDisposals.Find(model.AssetDisposalID);
            if (assetDisposal == null)
            {
                return NotFound();
            }

            // Update the fields only if they are not null/empty
            if (!string.IsNullOrEmpty(model.DisposalStatus))
            {
                assetDisposal.DisposalStatus = model.DisposalStatus;
            }

            // Only update DateDisposed if it's provided
            if (model.DateDisposed.HasValue)
            {
                assetDisposal.DateDisposed = model.DateDisposed;
            }

            // Update DisposalDescription if provided
            if (!string.IsNullOrEmpty(model.DisposalDescription))
            {
                assetDisposal.DisposalDescription = model.DisposalDescription;
            }

            // Update Notes only if provided
            if (!string.IsNullOrEmpty(model.Notes))
            {
                assetDisposal.Notes = model.Notes;
            }

            // Log the activity
            _activityLoggerService.LogActivity(
                userId: GetCurrentUserId(),
                activityType: ActivityType.EditDisposal,
                description: $"Disposal report updated for AssetID: {model.AssetID} DisposalID: {model.AssetDisposalID}",
                createdAt: System.DateTime.Now

            );

            _context.SaveChanges();
            return RedirectToAction("Index");
        }







        [HttpPost]
        [SessionAuthorize]
        public IActionResult BulkUpdate([FromBody] BulkUpdateRequest model)
        {
            if (model == null || model.Ids == null || model.Ids.Count == 0)
            {
                return BadRequest("No IDs provided for update.");
            }

            var disposalsToUpdate = _context.AssetDisposals
                .Where(d => model.Ids.Contains(d.AssetDisposalID))
                .ToList();

            foreach (var disposal in disposalsToUpdate)
            {
                // Update status only if provided
                if (!string.IsNullOrEmpty(model.Status))
                {
                    disposal.DisposalStatus = model.Status;
                }

                // Update DateDisposed only if provided
                if (model.DateDisposed.HasValue)
                {
                    disposal.DateDisposed = model.DateDisposed;
                }

                // Append notes only if provided
                if (!string.IsNullOrEmpty(model.AdditionalNotes))
                {
                    disposal.Notes += $"\n{model.AdditionalNotes}";
                }
            }

            _context.SaveChanges();

            return Ok();
        }


        // Nested class specific to DisposalsController
        public class BulkUpdateRequest
        {
            public List<int> Ids { get; set; }
            public string Status { get; set; }
            public DateTime? DateDisposed { get; set; }
            public string AdditionalNotes { get; set; }
        }


        [HttpPost("Disposals/DeleteDisposal/{assetDisposalID}")]
        [SessionAuthorize]
        public IActionResult DeleteDisposal(int assetDisposalID)
        {
            var disposal = _context.AssetDisposals.Find(assetDisposalID);
            if (disposal == null)
            {
                return NotFound();
            }

            _context.AssetDisposals.Remove(disposal);
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
