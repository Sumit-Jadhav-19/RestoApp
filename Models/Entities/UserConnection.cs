namespace RestoApp.Models.Entities
{
    public class UserConnection
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ConnectionId { get; set; }
        public int DataEnteredBy { get; set; }
        public DateTime DataEnteredOn { get; set; } = DateTime.Now;
        public int? DataModifiedBy { get; set; }
        public DateTime? DataModifiedOn { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
