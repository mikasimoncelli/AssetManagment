using AssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
public class AssetsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AssetsController(ApplicationDbContext context)
    {
        _context = context;
    }


    public IActionResult Index()
    {
        var assetsWithOffices = _context.Assets
            .Include(a => a.Office)
            .ToList(); 

        return View(assetsWithOffices);
    }


    public IActionResult CheckoutAsset()
    {
        return View();
    }


    [HttpGet]
    public IActionResult AddAsset()
    {
        ViewBag.Offices = _context.Offices.ToList();
        ViewBag.EquipmentTypes = _context.Assets
    .Select(a => a.EquipmentType)
    .Distinct()
    .ToList();
        return View();
    }


    [HttpPost]
    public IActionResult AddAsset(Asset asset)
    {
        if (int.TryParse(Request.Form["OfficeID"], out int officeId))
        {
            asset.OfficeID = officeId;
        }
        
        string newType = Request.Form["NewEquipmentType"];

        if (!string.IsNullOrWhiteSpace(newType))
        {
            asset.EquipmentType = newType; 
        }

        _context.Assets.Add(asset);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }


    [HttpGet]
    public IActionResult EditAsset(int id)
    {
        var asset = _context.Assets
            .Include(a => a.Office)
            .FirstOrDefault(a => a.AssetID == id);

        if (asset == null)
        {
            return NotFound();
        }

        ViewBag.Offices = _context.Offices.ToList(); 
        ViewBag.EquipmentTypes = _context.Assets.Select(a => a.EquipmentType).Distinct().ToList(); 

        return View(asset);
    }


    [HttpPost]
    public IActionResult UpdateAsset(Asset asset)
    {
        var existingAsset = _context.Assets.Find(asset.AssetID);
        if (existingAsset == null)
        {
            return NotFound();
        }

        existingAsset.Description = asset.Description;
        existingAsset.Manufacturer = asset.Manufacturer;
        existingAsset.EquipmentType = asset.EquipmentType;
        existingAsset.AssetNumber = asset.AssetNumber;
        existingAsset.SerialNumber = asset.SerialNumber;
        existingAsset.OfficeID = asset.OfficeID;

        _context.SaveChanges();

        return RedirectToAction("Index");
    }




    [HttpPost]
    public IActionResult DeleteAsset(int assetID)
    {
        var asset = _context.Assets.Find(assetID);
        if (asset != null)
        {
            _context.Assets.Remove(asset);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }






    public IActionResult AssetTypeView(string equipmentType)
    {
        var assetsByType = _context.Assets.Where(a => a.EquipmentType == equipmentType);
        return View(assetsByType); 
    }


    [HttpGet]
    public IActionResult AssetDamages(int assetID)
    {
        var asset = _context.Assets
            .Include(a => a.Office)
            .FirstOrDefault(a => a.AssetID == assetID);


        return View(asset);
    }


 

    [HttpPost]
    public IActionResult AssetDamages(AssetDamage model)
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


    public IActionResult AllDamagedAssets()
    {
        var damagedAssets = _context.AssetDamages.Include(ad => ad.Asset).ToList();
        return View(damagedAssets);
    }


    public IActionResult EditAssetDamage(int id)
    {
        var assetDamage = _context.AssetDamages.Include(a=>a.Asset).Include(a=>a.Asset.Office).FirstOrDefault(a=>a.AssetDamageID==id);
        if (assetDamage == null)
        {
            return NotFound();
        }
        return View(assetDamage);
    }

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


}