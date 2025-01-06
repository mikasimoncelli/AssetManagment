using AssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AssetManager.Helpers;

public class AssetsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ActivityLogger _activityLoggerService; // Define a private field for the logger


    public AssetsController(ApplicationDbContext context, ActivityLogger activityLoggerService)
    {
        _context = context;
        _activityLoggerService = activityLoggerService;  // Assign to the private variable
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

        var now = System.DateTime.Now;
        Console.WriteLine(now);
        asset.DateAdded = System.DateTime.Now;

        _context.Assets.Add(asset);
        _context.SaveChanges();

        // Log the activity
        _activityLoggerService.LogActivity(
            userId: GetCurrentUserId(),
            activityType: ActivityType.CreateAsset,
            description: $"Created a new asset with AssetID: {asset.AssetID}",
            createdAt: now // Pass current date and time explicitly

        );

       

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


    [HttpGet("Assets/ViewAsset/{id}")]
    public IActionResult ViewAsset(int id)
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

        // Log the activity
        _activityLoggerService.LogActivity(
            userId: GetCurrentUserId(),
            activityType: ActivityType.EditAsset,
            description: $"Updated the asset with AssetID: {asset.AssetID}",
            createdAt: System.DateTime.Now // Pass current date and time explicitly

        );

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