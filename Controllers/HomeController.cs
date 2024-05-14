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
    
    // Lists to hold various data models for view rendering.
    public List<OutputBidApiModel> SeriesList;
    public List<OutputBidPacketApiModel> BidResultList;
    public List<Position> PositionList;
    public List<BidPacketHistoryApiModel> UpdateHistoryList;
    
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, ILogger<OutputBidPacketAPiFetcher> outputLogger)
    {
        _logger = logger;
        _outputBidPacketAPiFetcher = new OutputBidPacketAPiFetcher(outputLogger);
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Fetch bid results asynchronously from a remote API.
        var result = await _outputBidPacketAPiFetcher.FetchBidResultsAsync();

        
        // Process each series item in the fetched result.
        List<OutputBidApiModel> series = new List<OutputBidApiModel>();
        foreach (var serie in result.Series)
        {
            List<Position> positions = new List<Position>();
            foreach (var position in serie.Positions)
            {
                // Create a new position object by parsing the quantity.
                Position pos = new Position(double.Parse(position.Quantity.ToString()));
               
                // Add each position to the list.
                positions.Add(pos);
            }
            // Create and configure a new serie model S from the fetched data.
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
            // Add the series to the list.
            series.Add(S);
        }
        
        // Process update history similarly.
        List<BidPacketHistoryApiModel> histories = new List<BidPacketHistoryApiModel>();
        
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
        
        // Compile all fetched and processed data into one model.
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

        // Add the complete model to the database context and save changes.
        _context.Bidresults.Add(outputBidPacketApiModel);
        await _context.SaveChangesAsync();

        
        // Store data in ViewBag for use in views.
        ViewBag.SeriesList = _context.Series.ToList();
        ViewBag.BidresultList = _context.Bidresults.ToList();
        ViewBag.PositionList = _context.Positions.ToList();
        ViewBag.UpdateHistoryList = _context.UpdateHistories.ToList();
        return View();
    }

    // Action method to increment the quantity of a position, handled via POST request.
    [HttpPost]
    public async Task<IActionResult> IncrementQuantity(int positionId)
    {
        // Attempt to find the position by ID.
        var position = await _context.Positions.FirstOrDefaultAsync(p => p.Id == positionId);
        _logger.LogInformation($"Attempting to fetch position with ID: {positionId}");
        
        if (position == null)
        {
            _logger.LogWarning($"No position found with ID: {positionId}");
            return NotFound();
        }
        
        if (position != null)
        {
            // Increment the position's quantity by one.
           position.Quantity += 1;
           
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