using System.ComponentModel.DataAnnotations;
namespace VolueEnergyTrader.Models
{
    public class OutputBidPacketApiModel
    {
        public OutputBidPacketApiModel()
        {
            
        }
        // Primary key
        public int Id { get; set; }
        
        [Required]
        public string ExternalId { get; set;  }
        
        [Required]
        public DateTime Day { get; set; }
        
        [Required]
        public DateTime DateOfLastChange { get; set; }
        
        [Required]
        public string Market { get; set; }
        
        [Required]
        public string Status { get; set; }
        
        [Required]
        public string Country { get; set; }

        [Required]
        public List<OutputBidApiModel> Series { get; set; }
        
        [Required]
        public List <BidPacketHistoryApiModel> Updatehistory { get; set; }
    }
    public class OutputBidApiModel
    {
        // Primary key
        public int Id { get; set; }
        public string ExternalId { get; set; }
        public string CustomerId { get; set; }
        public string Status { get; set; }
        public string Direction { get; set; }
        public string Currency { get; set; }
        public string PriceArea { get; set; }
        public string AssetId { get; set; }
        public decimal Price { get; set; }
        public DateTime StartInterval { get; set; }
        public DateTime EndInterval { get; set; }
        public string Resolution { get; set; }
        
        // Foreign key linking back to OutputBidPacketApiModel
        public int OutputBidPacketApiModelID { get; set; }
        
        public List<Position> Positions { get; set; }
    }
    public class Position
    {
        public Position(double quantity)
        {
            Quantity = quantity;
        }
        
        // Primary key
        public int Id { get; set; }
        public double Quantity { get; set; }
        
        // Foreign key to the OutputBidApiModel
        public int OutputBidApiModelID { get; set; }
        

    }
    public class BidPacketHistoryApiModel
    {
        // primary key
        public int Id { get; set; }
        public DateTime UpdateTime { get; set; }
        public string FromStatus { get; set; }
        public string ToStatus { get; set; }
        
        // Foreign Key OutputBidPacketApiModel
        public int OutputBidPacketApiModelID { get; set; }


    }
}