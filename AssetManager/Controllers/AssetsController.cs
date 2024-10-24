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
        // Try to parse the OfficeID from the form
        if (int.TryParse(Request.Form["OfficeID"], out int officeId))
        {
            asset.OfficeID = officeId;
        }

  

        // Handle New Equipment Type if it exists
        string newType = Request.Form["NewEquipmentType"];
        if (!string.IsNullOrWhiteSpace(newType))
        {
            asset.EquipmentType = newType; // Assign the new type to EquipmentType
        }

        // Save the asset to the database
        _context.Assets.Add(asset);
        _context.SaveChanges();

        // Redirect to the index page after successful save
        return RedirectToAction("Index");
    }





}