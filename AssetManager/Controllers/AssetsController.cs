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

        ViewBag.TotalAssets = assetsWithOffices.Count;

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


    [HttpGet("Assets/EditAsset/{id}")]
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




    [HttpGet]
    public JsonResult CheckReferences(int assetID)
    {
        var checkedOutAsset = _context.CheckedOutAssets.Include(c => c.User).FirstOrDefault(c => c.AssetID == assetID);

        if (checkedOutAsset != null)
        {
            return Json(new
            {
                hasReferences = true,
                checkedOutID = checkedOutAsset.CheckedOutID,
                userName = checkedOutAsset.User.FirstName + ' ' + checkedOutAsset.User.LastName,
                checkedOutDate = checkedOutAsset.DateLentOut.ToString("dd-MM-yyyy")
            });
        }



        return Json(new { hasReferences=false});



    }

    [HttpDelete]
    public IActionResult DeleteAsset(int assetID)
    {
        var checkedOutAssets = _context.CheckedOutAssets.Where(c => c.AssetID == assetID).ToList();
        _context.CheckedOutAssets.RemoveRange(checkedOutAssets);
        _context.Assets.Remove(_context.Assets.Find(assetID));
        _context.SaveChanges();
        return Ok();
    }







    public IActionResult AssetTypeView(string equipmentType)
    {
        var assetsByType = _context.Assets.Where(a => a.EquipmentType == equipmentType);
        return View(assetsByType); 
    }




}