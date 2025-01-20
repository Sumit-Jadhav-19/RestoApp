using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RestoApp.Models.Entities
{
    public class OrderDetails
    {
        [Key]
        [Column(TypeName = "numeric(15, 0)")]
        public long DetailsId { get; set; }
        [Column(TypeName = "numeric(15, 0)")]
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
        public bool IsAccept { get; set; } = false;
        public string? OrderStatus { get; set; }
        public int OrderCount { get; set; } = 0;
    }
}
