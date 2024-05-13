using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolueEnergyTrader.Models;
namespace VolueEnergyTrader.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly OutputBidPacketAPiFetcher _outputBidPacketAPiFetcher;
    
    private ApplicationDbContext _context;
    public List<OutputBidApiModel> SeriesList;
    public List<OutputBidPacketApiModel> BidResultList;
    public List<Position> PositionList;
    public List<BidPacketHistoryApiModel> UpdateHistoryList;
    
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, ILogger<OutputBidPacketAPiFetcher> outputLogger)
    {
        _logger = logger;
        _outputBidPacketAPiFetcher = new OutputBidPacketAPiFetcher(outputLogger);
        _context = context;
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();    
    }

    public async Task<IActionResult> Index()
    {
        
        // Fetching results from API
        var result = await _outputBidPacketAPiFetcher.FetchBidResultsAsync();

        // Fetching content in series and positions
        List<OutputBidApiModel> series = new List<OutputBidApiModel>();
        foreach (var serie in result.Series)
        {
            // Create new list of positions for each serie
            List<Position> positions = new List<Position>();
            foreach (var position in serie.Positions)
            {
                Position pos = new Position(double.Parse(position.Quantity.ToString()));
                // Add posistions to database 
                positions.Add(pos);
            }
            //  Creat object of serie S, and add values to series
            OutputBidApiModel S = new OutputBidApiModel()
            {
                ExternalId = serie.ExternalId,
                CustomerId = serie.CustomerId,
                Status = serie.Status,
                Direction = serie.Direction,
                Currency = serie.Currency,
                PriceArea = serie.PriceArea,
                AssetId = serie.AssetId,
                Price = serie.Price,
                StartInterval =serie.StartInterval,
                EndInterval = serie.EndInterval,
                Resolution = serie.Resolution,
                Positions = positions
            };
            // Adds serie to database each itteration
            series.Add(S);
        }
        
        List<BidPacketHistoryApiModel> histories = new List<BidPacketHistoryApiModel>();
        
        // Fetching elements in Updatehistory
        foreach (var history in result.Updatehistory)
        {
            BidPacketHistoryApiModel hist = new BidPacketHistoryApiModel()
            {
                UpdateTime = history.UpdateTime,
                FromStatus = history.FromStatus,
                ToStatus = history.ToStatus
            };
            histories.Add(hist);
        }
        
        // Creating object of bidResult
        OutputBidPacketApiModel outputBidPacketApiModel = new OutputBidPacketApiModel()
        {
            ExternalId = result.ExternalId,
            Country = result.Country,
            Day = result.Day,
            DateOfLastChange = result.DateOfLastChange,
            Market = result.Market,
            Status = result.Status,
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
        Console.WriteLine(@"this is the {position}");
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