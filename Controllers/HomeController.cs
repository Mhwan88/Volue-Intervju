using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VolueEnergyTrader.Models;
using VolueEnergyTrading.Controller;
using VolueEnergyTrading.Models;

namespace VolueEnergyTrader.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly BidResultController _bidResultController;
    private ApplicationDbContext _context;

    
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _bidResultController = new BidResultController();
        _context = context;
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();    

    }

    public async Task<IActionResult> Index()
    {
        
        // Fetching results from API
        var result = await _bidResultController.FetchBidResultsAsync();

        // Loops through series and Posistions and add to list
        List<Serie> series = new List<Serie>();
        foreach (var serie in result["series"])
        {
            List<Position> positions = new List<Position>();
            foreach (var position in serie["positions"])
            {
                Position pos = new Position(int.Parse(position["quantity"].ToString()));
                positions.Add(pos);
                //_context.Positions.Add(pos);
                //await _context.SaveChangesAsync();
            }
                
            Serie S = new Serie()
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
            series.Add(S);
            //_context.Series.Add(S);
            //await _context.SaveChangesAsync();
        }
        
        
        List<UpdateHistory> histories = new List<UpdateHistory>();

        foreach (var history in result["updateHistory"])
        {
            UpdateHistory hist = new UpdateHistory()
            {
                UpdateTime = DateTime.Parse(history["updateTime"].ToString()),
                FromStatus = history["fromStatus"].ToString(),
                ToStatus = history["toStatus"].ToString()
            };
            histories.Add(hist);
        }
        
        
        BidResult bidResult = new BidResult()
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

        
        _context.Bidresults.Add(bidResult);
        _context.SaveChangesAsync();


        return View();
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