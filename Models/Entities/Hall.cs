namespace RestoApp.Models.Entities
{
    public class Hall
    {
        public int HallId { get; set; }
        public string HallName { get; set; }
        public string? HallDescription { get; set; }
        public int DataEnteredBy { get; set; }  
        public DateTime DataEnteredOn { get; set; } = DateTime.Now;
        public int? DataModifiedBy { get; set; }
        public DateTime? DataModifiedOn { get;set; }
        public bool IsActive { get; set; } = true;
    }
}
