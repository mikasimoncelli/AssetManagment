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
            var assets = _context.Assets.Include(a => a.Office).ToList();
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
    }
