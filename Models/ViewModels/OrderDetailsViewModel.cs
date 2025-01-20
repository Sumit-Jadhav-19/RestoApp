using System.ComponentModel.DataAnnotations.Schema;

namespace RestoApp.Models.ViewModels
{
    public class OrderDetailsViewModel
    {
        public long DetailsId { get; set; }
        public long OrderId { get; set; }
        public int CategoryId { get; set; }
        public int MenuId { get; set; }
        public int Quantity { get; set; }
        public string Size { get; set; } = "F";
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
        public int DataEnteredBy { get; set; }
        public DateTime DataEnteredOn { get; set; } = DateTime.Now;
        public int? DataModifiedBy { get; set; }
        public DateTime? DataModifiedOn { get; set; }
        public bool IsActive { get; set; } = true;
        public int? OrderCount { get; set; }

    }
}
