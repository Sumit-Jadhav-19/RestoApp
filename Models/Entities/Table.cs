namespace RestoApp.Models.Entities
{
    public class Table
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int HallId { get; set; }
        public Hall Hall { get; set; }
        public int DataEnteredBy { get; set; }
        public DateTime DataEnteredOn { get; set; } = DateTime.Now;
        public int? DataModifiedBy { get; set; }
        public DateTime? DataModifiedOn { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsOccupied { get; set; } = false;
    }
}
