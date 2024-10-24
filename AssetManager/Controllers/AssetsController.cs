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
        if (!ModelState.IsValid)
        {
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Error in {state.Key}: {error.ErrorMessage}");
                }
            }
        }
        _context.Assets.Add(asset);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }


}