using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RestoApp.Models.Entities
{
    public class OrderMaster
    {
        [Key]
        [Column(TypeName = "numeric(15, 0)")]
        public long OrderId { get; set; }
        public int HallId { get; set; }
        public int TableId { get; set; }
        public int CaptainId { get; set; }
        public DateTime OrderDateTime { get; set; } = DateTime.Now;
        public int? ChefId { get; set; }
        public string? ChefStatus { get; set; }
        public bool OrderStatus { get; set; } = false;
        public int DataEnteredBy { get; set; }
        public DateTime DataEnteredOn { get; set; } = DateTime.Now;
        public int? DataModifiedBy { get; set; }
        public DateTime? DataModifiedOn { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsBillOrder { get; set; } = false;
        public bool IsBillPayed { get; set; } = false;
        public string? PaymentType { get; set; } 
    }
}
