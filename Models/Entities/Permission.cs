namespace RestoApp.Models.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "ViewReports"
        public string Description { get; set; } // Optional
    }
}
