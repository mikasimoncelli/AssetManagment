using AssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
public class AssetsController : Controller
{
    private readonly ApplicationDbContext _context;

    // Inject the DbContext via the constructor
    public AssetsController(ApplicationDbContext context)
    {
        _context = context;
    }




    public IActionResult Index()
    {
        // Fetch all assets and include related office data
        var assetsWithOffices = _context.Assets
            .Include(a => a.Office)
            .ToList(); // Retrieve all data

        // Pass the data to the view
        return View(assetsWithOffices);
    }









    public IActionResult CheckoutAsset()
    {
        return View();
    }


    [HttpGet]
    public IActionResult AddAsset()
    {
        // Pass the list of offices to the view using ViewBag
        ViewBag.Offices = _context.Offices.ToList();
        return View();
    }

    [HttpPost]
    public IActionResult AddAsset(Asset asset)
    {
        if (int.TryParse(Request.Form["OfficeID"], out int officeId))
        {
            asset.OfficeID = officeId;
        }
        // Check if the model state is valid
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
        // Add the new asset to the database
        _context.Assets.Add(asset);
        _context.SaveChanges();

        // Redirect to Index after saving
        return RedirectToAction("Index");
        

        // If validation fails, reload the list of offices for the dropdown
        ViewBag.Offices = _context.Offices.ToList();
        return View(asset);
    }


}
