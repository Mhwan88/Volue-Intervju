using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolueEnergyTrader.Models;
namespace VolueEnergyTrader.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly OutputBidPacketApiController _outputBidPacketApiController;
    
    private ApplicationDbContext _context;
    public List<OutputBidApiModel> SeriesList;
    public List<OutputBidPacketApiModel> BidResultList;
    public List<Position> PositionList;
    public List<BidPacketHistoryApiModel> UpdateHistoryList;
    
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, ILogger<OutputBidPacketApiController> outputLogger)
    {
        _logger = logger;
        _outputBidPacketApiController = new OutputBidPacketApiController(outputLogger);
        _context = context;
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();    
    }

    public async Task<IActionResult> Index()
    {
        
        // Fetching results from API
        var result = await _outputBidPacketApiController.FetchBidResultsAsync();

        // Fetching content in series and positions
        List<OutputBidApiModel> series = new List<OutputBidApiModel>();
        foreach (var serie in result["series"])
        {
            // Create new list of positions for each serie
            List<Position> positions = new List<Position>();
            foreach (var position in serie["positions"])
            {
                Position pos = new Position(double.Parse(position["quantity"].ToString()));
                // Add posistions to database 
                positions.Add(pos);
            }
            //  Creat object of serie S, and add values to series
            OutputBidApiModel S = new OutputBidApiModel()
            {
                ExternalId = serie["externalId"].ToString(),
                CustomerId = serie["customerId"].ToString(),
                Status = serie["status"].ToString(),
                Direction = serie["direction"].ToString(),
                Currency = serie["currency"].ToString(),
                PriceArea = serie["priceArea"].ToString(),
                AssetId = serie["assetId"].ToString(),
                Price = decimal.Parse(serie["price"].ToString()),
                StartInterval = DateTime.Parse(serie["startInterval"].ToString()),
                EndInterval = DateTime.Parse(serie["endInterval"].ToString()),
                Resolution = serie["resolution"].ToString(),
                Positions = positions
            };
            // Adds serie to database each itteration
            series.Add(S);
        }
        
        List<BidPacketHistoryApiModel> histories = new List<BidPacketHistoryApiModel>();
        
        // Fetching elements in Updatehistory
        foreach (var history in result["updateHistory"])
        {
            BidPacketHistoryApiModel hist = new BidPacketHistoryApiModel()
            {
                UpdateTime = DateTime.Parse(history["updateTime"].ToString()),
                FromStatus = history["fromStatus"].ToString(),
                ToStatus = history["toStatus"].ToString()
            };
            histories.Add(hist);
        }
        
        // Creating object of bidResult
        OutputBidPacketApiModel outputBidPacketApiModel = new OutputBidPacketApiModel()
        {
            ExternalId = result["externalId"].ToString(),
            Country = result["country"].ToString(),
            Day = DateTime.Parse(result["day"].ToString()),
            DateOfLastChange = DateTime.Parse(result["dateOfLastChange"].ToString()),
            Market = result["market"].ToString(),
            Status = result["status"].ToString(),
            Series = series,
            Updatehistory = histories
        };

        // Adding Bidresults to database and save changes
        _context.Bidresults.Add(outputBidPacketApiModel);
        _context.SaveChangesAsync();


        // Adding content in ViewBag to fetch in HTML
        ViewBag.SeriesList = _context.Series.ToList();
        ViewBag.BidresultList = _context.Bidresults.ToList();
        ViewBag.PositionList = _context.Positions.ToList();
        ViewBag.UpdateHistoryList = _context.UpdateHistories.ToList();
        return View();
    }

    
    [HttpPost]
    public async Task<IActionResult> IncrementQuantity(int positionId)
    {
        // Find the position by ID
        var position = await _context.Positions.FirstOrDefaultAsync(p => p.Id == positionId);
        if (position != null)
        {
            // Increment the quantity
            position.Quantity += 1;

            // Save changes to the database
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
    
    
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}