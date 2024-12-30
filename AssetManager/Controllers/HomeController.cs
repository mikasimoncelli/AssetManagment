using AssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using iText.Layout.Properties;
using iText.Commons.Actions.Contexts;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Draw;

namespace AssetManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            //Used to Calculate the phone and laptop availability
            var currentlyCheckedOutAssetIds = _context.CheckedOutAssets
                .GroupBy(c => c.AssetID)
                .Where(g => g.OrderByDescending(c => c.DateLentOut).FirstOrDefault().DateReturned == null)
                .Select(g => g.Key)
                .ToList();

            //Total Asset Count
            var assetCount = _context.Assets
                .Include(a => a.Office)
                .GroupBy(a => a.Office.OfficeID)
                .Select(g => new
                {
                    OfficeName = $"{g.FirstOrDefault().Office.OfficeName} - {g.FirstOrDefault().Office.Location}",
                    OfficeID = g.FirstOrDefault().Office.OfficeID,
                    AssetsCount = g.Count()
                })
                .ToList();

            //Calculate Overdue Loans
            var overdueReturns = _context.CheckedOutAssets
                .Where(c => c.DateReturned == null && c.DueDate < DateTime.Now)
                .OrderBy(c => c.DueDate)
                .Select(c => new
                {
                    CheckedOutID = c.CheckedOutID, // Include CheckedOutID
                    AssetName = c.Asset.Description,
                    AssetType = c.Asset.EquipmentType,
                    DueDate = c.DueDate,
                    UserName = c.User.FirstName + ' ' + c.User.LastName
                })
                .ToList();

            //Calulcate Upcoming Loan Returns
            var upcomingReturns = _context.CheckedOutAssets
                .Where(c => c.DateReturned == null && c.DueDate >= DateTime.Now && c.DueDate <= DateTime.Now.AddDays(7))
                .OrderBy(c => c.DueDate)
                .Select(c => new
                {
                    CheckedOutID = c.CheckedOutID, 
                    AssetName = c.Asset.Description,
                    AssetType = c.Asset.EquipmentType,
                    DueDate = c.DueDate,
                    UserName = c.User.FirstName + ' ' + c.User.LastName
                })
                .ToList();

            ViewBag.OverdueReturns = overdueReturns;
            ViewBag.UpcomingReturns = upcomingReturns;

            ViewBag.totalAssets = assetCount.Sum(a => a.AssetsCount);
            ViewBag.assetCount = assetCount;
            ViewBag.phoneAvailability = CalculateAvailabilityPercentage("Phone", currentlyCheckedOutAssetIds);
            ViewBag.laptopAvailability = CalculateAvailabilityPercentage("Laptop", currentlyCheckedOutAssetIds);

            return View();
        }


        //Calculates the percentage of Laptops and Phones
        private double CalculateAvailabilityPercentage(string equipmentType, List<int> checkedOutAssetIds)
        {
            int totalAssets = _context.Assets.Count(a => a.EquipmentType == equipmentType);
            int availableAssets = _context.Assets
                .Where(a => a.EquipmentType == equipmentType && !checkedOutAssetIds.Contains(a.AssetID))
                .Count();

            double percentage = totalAssets > 0 ? (availableAssets / (double)totalAssets) * 100 : 0;
            return Math.Round(percentage, 2); // Rounds to 2 decimal places
        }



        //Reports page
        public IActionResult Reports()
        {
            return View();
        }

        public IActionResult Account()
        {
            return View();
        }

        //Download Assets Report
        [HttpGet]
        public IActionResult AssetsReport()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4); // Define A4 page size
                document.SetMargins(20, 20, 20, 20); // Set margins

                // Add Logo at the top of the first page
                var logoPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo.jpg");
                var imageData = ImageDataFactory.Create(logoPath);
                var logo = new iText.Layout.Element.Image(imageData).ScaleToFit(100, 100);
                document.Add(logo);

                // Add Title and Date
                document.Add(new Paragraph("Asset Report").SetFontSize(14).SetBold().SetTextAlignment(TextAlignment.CENTER));
                string currentDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");
                document.Add(new Paragraph($"Date: {currentDate}").SetFontSize(12).SetTextAlignment(TextAlignment.RIGHT).SetBold());

                // Add Introductory Message
                string message = "This report lists each asset along with its associated office details. Reports are printed on the date displayed and assets counted based on the data present in the system. If any details need to be updated or changes made, please contact the Helpdesk in the first instance.";
                document.Add(new Paragraph(message).SetFontSize(6).SetTextAlignment(TextAlignment.JUSTIFIED).SetMarginTop(10));

                // Fetch Data from the Database
                var assets = _context.Assets
                    .Include(a => a.Office)
                    .Select(a => new
                    {
                        AssetNumber = a.AssetNumber,         // Assuming this field exists
                        SerialNumber = a.SerialNumber,       // Assuming this field exists
                        AssetType = a.EquipmentType,         // Replace with actual asset type property

                        AssetName = a.Description,                  // Replace with actual asset name property
                        OfficeName = a.Office.OfficeName,
                        OfficeLocation = a.Office.Location
                    })
                    .ToList();

                // Calculate Total Asset Count and Count Per Office
                int totalAssets = assets.Count;
                var officeAssetCounts = assets
                    .GroupBy(a => a.OfficeName)
                    .Select(g => new
                    {
                        OfficeName = g.Key,
                        OfficeCount = g.Count()
                    })
                    .ToList();

                // Display Total Asset Count and Office Asset Counts
                document.Add(new Paragraph($"Total Asset Count: {totalAssets}").SetFontSize(8).SetBold().SetMarginTop(10));
                foreach (var office in officeAssetCounts)
                {
                    document.Add(new Paragraph($"{office.OfficeName}: {office.OfficeCount}").SetFontSize(8).SetMarginLeft(0).SetBold());
                }

                // Add a line separator before the table
                document.Add(new LineSeparator(new SolidLine()).SetMarginTop(10).SetMarginBottom(10));

                // Create and style the table with adjusted column widths
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1, 1, 3, 2, 2 })).UseAllAvailableWidth();
                table.SetMarginTop(10);

                // Define header style
                var headerCellStyle = new Style()
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetBold();

                // Add Table Headers
                table.AddHeaderCell(new Cell().Add(new Paragraph("Asset Number")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Serial Number")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Asset Type")).AddStyle(headerCellStyle));

                table.AddHeaderCell(new Cell().Add(new Paragraph("Asset Name")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Office Name")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Office Location")).AddStyle(headerCellStyle));

                // Populate the table with asset data
                foreach (var asset in assets)
                {
                    table.AddCell(new Cell().Add(new Paragraph(asset.AssetNumber)).SetFontSize(8));  // Small font size
                    table.AddCell(new Cell().Add(new Paragraph(asset.SerialNumber)).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(asset.AssetType)).SetFontSize(8));

                    table.AddCell(new Cell().Add(new Paragraph(asset.AssetName)).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(asset.OfficeName)).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(asset.OfficeLocation)).SetFontSize(8));
                }

                document.Add(table);

                document.Close(); // Finalize the document

                return File(stream.ToArray(), "application/pdf", "AssetReport.pdf");
            }
        }


        [HttpGet]
        public IActionResult LoanedAssetsReport()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);
                document.SetMargins(20, 20, 20, 20);

                // Add Logo
                var logoPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo.jpg");
                var imageData = ImageDataFactory.Create(logoPath);
                var logo = new iText.Layout.Element.Image(imageData).ScaleToFit(100, 100);
                document.Add(logo);

                // Add Title and Date
                document.Add(new Paragraph("Loaned Assets Report").SetFontSize(18).SetBold().SetTextAlignment(TextAlignment.CENTER));
                string currentDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");
                document.Add(new Paragraph($"Date: {currentDate}").SetFontSize(12).SetTextAlignment(TextAlignment.RIGHT));

                // Add Introductory Message
                string message = "This report lists each asset along with its associated loan details. Reports are printed on the date displayed and assets counted based on the data present in the system. If any details need to be updated or changes made, please contact the Helpdesk in the first instance.";
                document.Add(new Paragraph(message).SetFontSize(10).SetTextAlignment(TextAlignment.JUSTIFIED).SetMarginTop(10));

                // Fetch Data for Loaned Assets and Order by Date Lent Out
                var allCheckedOutAssets = _context.CheckedOutAssets
                    .Include(c => c.Asset)
                    .Include(c => c.User)
                    .OrderBy(c => c.DateLentOut) // Order by Date Lent Out
                    .ToList();

                var checkedOutAssets = allCheckedOutAssets.Where(c => c.DateReturned == null).ToList();
                var returnedAssets = allCheckedOutAssets.Where(c => c.DateReturned != null).ToList();

                // Summary Information
                int totalLoansMade = allCheckedOutAssets.Count;
                int totalReturned = returnedAssets.Count;
                int totalNotReturned = checkedOutAssets.Count;

                // Display Summary Information
                document.Add(new Paragraph($"Total Loans Made: {totalLoansMade}").SetFontSize(12).SetBold());
                document.Add(new Paragraph($"Total Returned: {totalReturned}").SetFontSize(12).SetBold());
                document.Add(new Paragraph($"Total Not Returned: {totalNotReturned}").SetFontSize(12).SetBold());

                // Add a line separator
                document.Add(new LineSeparator(new SolidLine()).SetMarginTop(10).SetMarginBottom(10));

                // Create and style the table with an additional column for "Date Returned"
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 2, 2, 2, 2, 3, 2, 2 })).UseAllAvailableWidth();
                table.SetMarginTop(10);

                var headerCellStyle = new Style()
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetBold();

                // Add Table Headers
                table.AddHeaderCell(new Cell().Add(new Paragraph("Date Lent Out")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Date Returned")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Checked Out ID")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Equipment Type")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Description")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("User First Name")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("User Last Name")).AddStyle(headerCellStyle));

                // Populate the table with loaned asset data
                foreach (var asset in allCheckedOutAssets)
                {
                    table.AddCell(new Cell().Add(new Paragraph(asset.DateLentOut.ToString("yyyy-MM-dd"))).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(asset.DateReturned?.ToString("yyyy-MM-dd") ?? "Not Returned")).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(asset.CheckedOutID.ToString())).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(asset.Asset?.EquipmentType ?? "Unknown")).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(asset.Asset?.Description ?? "Unknown Asset")).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(asset.User?.FirstName ?? "Unknown")).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(asset.User?.LastName ?? "Unknown")).SetFontSize(8));
                }

                document.Add(table);
                document.Close(); // Finalize the document

                return File(stream.ToArray(), "application/pdf", "LoanedAssetsReport.pdf");
            }
        }

        [HttpGet]
        public IActionResult UsersReport()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);
                document.SetMargins(20, 20, 20, 20);

                // Add Logo
                var logoPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo.jpg");
                var imageData = ImageDataFactory.Create(logoPath);
                var logo = new iText.Layout.Element.Image(imageData).ScaleToFit(100, 100);
                document.Add(logo);

                // Add Title and Date
                document.Add(new Paragraph("Users Report").SetFontSize(18).SetBold().SetTextAlignment(TextAlignment.CENTER));
                string currentDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");
                document.Add(new Paragraph($"Date: {currentDate}").SetFontSize(12).SetTextAlignment(TextAlignment.RIGHT));

                // Add Introductory Message
                string message = "This report lists each user registered in the system. Reports are printed on the date displayed and user data is current based on the information present in the system. If any details need to be updated or changes made, please contact the Helpdesk in the first instance.";
                document.Add(new Paragraph(message).SetFontSize(10).SetTextAlignment(TextAlignment.JUSTIFIED).SetMarginTop(10));

                // Fetch Data for Users and Calculate Total Users
                var users = _context.Users.ToList();
                int totalUsers = users.Count;

                // Display Total Users
                document.Add(new Paragraph($"Total Users: {totalUsers}").SetFontSize(12).SetBold());

                // Add a line separator
                document.Add(new LineSeparator(new SolidLine()).SetMarginTop(10).SetMarginBottom(10));

                // Create and style the table
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2, 2, 3 })).UseAllAvailableWidth();
                table.SetMarginTop(10);

                var headerCellStyle = new Style()
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetBold();

                // Add Table Headers
                table.AddHeaderCell(new Cell().Add(new Paragraph("User ID")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("First Name")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Last Name")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Email")).AddStyle(headerCellStyle));

                // Populate the table with user data
                foreach (var user in users)
                {
                    table.AddCell(new Cell().Add(new Paragraph(user.UserID.ToString())).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(user.FirstName)).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(user.LastName)).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(user.Email)).SetFontSize(8));
                }

                document.Add(table);
                document.Close(); // Finalize the document

                return File(stream.ToArray(), "application/pdf", "UsersReport.pdf");
            }
        }

        public IActionResult DamagesReport()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);
                document.SetMargins(20, 20, 20, 20);

                // Add Logo
                var logoPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo.jpg");
                var imageData = ImageDataFactory.Create(logoPath);
                var logo = new iText.Layout.Element.Image(imageData).ScaleToFit(100, 100);
                document.Add(logo);

                // Add Title and Date
                document.Add(new Paragraph("Damages Report").SetFontSize(18).SetBold().SetTextAlignment(TextAlignment.CENTER));
                string currentDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");
                document.Add(new Paragraph($"Date: {currentDate}").SetFontSize(12).SetTextAlignment(TextAlignment.RIGHT));

                // Add Introductory Message
                string message = "This report provides details of all recorded damages for assets. Reports are generated based on the latest data available in the system.";
                document.Add(new Paragraph(message).SetFontSize(10).SetTextAlignment(TextAlignment.JUSTIFIED).SetMarginTop(10));

                // Fetch Data for Asset Damages
                var damages = _context.AssetDamages.Include(d => d.Asset).ToList();
                int totalDamages = damages.Count;

                // Display Total Damages
                document.Add(new Paragraph($"Total Damages: {totalDamages}").SetFontSize(12).SetBold());

                // Add a line separator
                document.Add(new LineSeparator(new SolidLine()).SetMarginTop(10).SetMarginBottom(10));

                // Create and style the table
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 1,1,1,1,1,1 })).UseAllAvailableWidth();
                table.SetMarginTop(10);

                var headerCellStyle = new Style()
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetBold();

                // Add Table Headers
                table.AddHeaderCell(new Cell().Add(new Paragraph("Damage ID")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Asset Name")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Asset Number")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Damage Description")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Damage Type")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Repair Status")).AddStyle(headerCellStyle));

                // Populate the table with damage data
                foreach (var damage in damages)
                {
                    table.AddCell(new Cell().Add(new Paragraph(damage.AssetDamageID.ToString())).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(damage.Asset.Description.ToString())).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(damage.Asset.AssetNumber.ToString())).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(damage.DamageDescription ?? "N/A")).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(damage.DamageType ?? "N/A")).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(string.IsNullOrEmpty(damage.RepairStatus) ? ("Empty") : damage.RepairStatus)).SetFontSize(8));
                }

                document.Add(table);
                document.Close(); // Finalize the document

                return File(stream.ToArray(), "application/pdf", "DamagesReport.pdf");
            }
        }


        public IActionResult DisposalsReport()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);
                document.SetMargins(20, 20, 20, 20);

                // Add Logo
                var logoPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo.jpg");
                var imageData = ImageDataFactory.Create(logoPath);
                var logo = new iText.Layout.Element.Image(imageData).ScaleToFit(100, 100);
                document.Add(logo);

                // Add Title and Date
                document.Add(new Paragraph("Disposals Report").SetFontSize(18).SetBold().SetTextAlignment(TextAlignment.CENTER));
                string currentDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");
                document.Add(new Paragraph($"Date: {currentDate}").SetFontSize(12).SetTextAlignment(TextAlignment.RIGHT));

                // Add Introductory Message
                string message = "This report provides details of all disposed assets. Reports are generated based on the latest data available in the system.";
                document.Add(new Paragraph(message).SetFontSize(10).SetTextAlignment(TextAlignment.JUSTIFIED).SetMarginTop(10));

                // Fetch Data for Asset Disposals
                var disposals = _context.AssetDisposals.Include(d => d.Asset).ToList();
                int totalDisposals = disposals.Count;

                // Display Total Disposals
                document.Add(new Paragraph($"Total Disposals: {totalDisposals}").SetFontSize(12).SetBold());

                // Add a line separator
                document.Add(new LineSeparator(new SolidLine()).SetMarginTop(10).SetMarginBottom(10));

                // Create and style the table
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 2, 2, 2, 2, 2, 2 })).UseAllAvailableWidth();
                table.SetMarginTop(10);

                var headerCellStyle = new Style()
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetBold();

                // Add Table Headers
                table.AddHeaderCell(new Cell().Add(new Paragraph("Disposal ID")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Asset Name")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Disposal Description")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Disposal Reason")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Disposal Status")).AddStyle(headerCellStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Date Disposed")).AddStyle(headerCellStyle));

                // Populate the table with disposal data
                foreach (var disposal in disposals)
                {
                    table.AddCell(new Cell().Add(new Paragraph(disposal.AssetDisposalID.ToString())).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(disposal.Asset.Description.ToString())).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(disposal.DisposalDescription ?? "N/A")).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(disposal.DisposalReason ?? "N/A")).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(disposal.DisposalStatus ?? "N/A")).SetFontSize(8));
                    table.AddCell(new Cell().Add(new Paragraph(disposal.DateDisposed?.ToShortDateString() ?? "N/A")).SetFontSize(8));
                }

                document.Add(table);
                document.Close(); // Finalize the document

                return File(stream.ToArray(), "application/pdf", "DisposalsReport.pdf");
            }
        }



        public IActionResult GetAllData()
        {
            // Materialize Assets with navigation properties in memory
            var assets = _context.Assets
                .Include(a => a.Office)
                .Select(a => new
                {
                    Type = "Asset",
                    a.AssetID,
                    a.Description,
                    a.Manufacturer,
                    a.AssetNumber,
                    a.SerialNumber,
                    OfficeName = a.Office.OfficeName,
                    OfficeLocation = a.Office.Location
                })
                .ToList(); // Materialize the query

            // Materialize Offices in memory
            var offices = _context.Offices
                .Select(o => new
                {
                    Type = "Office",
                    o.OfficeID,
                    o.OfficeName,
                    o.Location
                })
                .ToList(); // Materialize the query

            // Materialize Users in memory
            var users = _context.Users
                .Select(u => new
                {
                    Type = "User",
                    u.UserID,
                    FullName = u.FirstName + " " + u.LastName,
                    u.Email
                })
                .ToList(); // Materialize the query

            // Combine the data sets in memory
            var combinedData = assets.Cast<object>()
                .Concat(offices)
                .Concat(users)
                .ToList();

            return Json(combinedData); // Return the combined data
        }









    }
}
