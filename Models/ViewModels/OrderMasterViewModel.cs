namespace RestoApp.Models.ViewModels
{
    public class OrderMasterViewModel
    {
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
        public List<OrderDetailsViewModel> OrderDetails { get; set; }
    }
}
