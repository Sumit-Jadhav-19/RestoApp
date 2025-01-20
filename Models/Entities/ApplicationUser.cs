using Microsoft.AspNetCore.Identity;

namespace RestoApp.Models.Entities
{
    public class ApplicationUser : IdentityUser<int> // or use string, GUID, etc.
    {
        // Additional properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string ProfilePictureUrl { get; set; }
        public int Role { get; set; }
        public int DataEnteredBy { get; set; }
        public DateTime DataEnteredOn { get; set; } = DateTime.Now;
        public int? DataModifiedBy { get; set; }
        public DateTime? DataModifiedOn { get; set; }
        public bool IsActive { get; set; } = true;
    }
    public class ApplicationRole : IdentityRole<int>
    {
        public bool Is_Active { get; set; } = true;
    }
}
