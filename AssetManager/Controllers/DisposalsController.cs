using AssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using NuGet.ContentModel;

namespace AssetManager.Controllers
{
    public class DisposalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DisposalsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var disposals = _context.AssetDisposals.Include(a => a.Asset).ToList();
            return View(disposals);
        }

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
        public IActionResult AssetDisposalsForm(int assetID)
        {
            var asset = _context.Assets
            .Include(a => a.Office)
            .FirstOrDefault(a => a.AssetID == assetID);
            return View(asset);

        }



        [HttpPost]
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

            return RedirectToAction("Index");

        }


        // GET Edit Disposal
        public IActionResult EditDisposal(int id)
        {
            var assetDisposal = _context.AssetDisposals
                .Include(ad => ad.Asset) // Include the Asset details
                .FirstOrDefault(ad => ad.AssetDisposalID == id);

            if (assetDisposal == null)
            {
                return NotFound();
            }

            return View(assetDisposal);
        }

        // POST Edit Disposal
        [HttpPost]
        public IActionResult EditDisposal(AssetDisposal model)
        {
            var assetDisposal = _context.AssetDisposals.Find(model.AssetDisposalID);
            if (assetDisposal == null)
            {
                return NotFound();
            }

            assetDisposal.DisposalStatus = model.DisposalStatus;
            assetDisposal.DateDisposed = model.DateDisposed;
            assetDisposal.DisposalDescription = model.DisposalDescription;
            assetDisposal.Notes = model.Notes;

            _context.SaveChanges();
            return RedirectToAction("Index"); // Redirect to an appropriate action
        }




        [HttpPost]
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
                disposal.DisposalStatus = model.Status;
                disposal.DateDisposed = model.DateDisposed ?? disposal.DateDisposed;
                disposal.Notes += $"\n{model.AdditionalNotes}";
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



    }
}
