namespace RestoApp.Models.Entities
{
    public class Categories
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime DataEnteredOn { get; set; } = DateTime.Now;
        public int DataEnteredBy { get; set; }
        public DateTime? DataModifiedOn { get; set; }
        public int? DataModifiedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
