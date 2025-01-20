using System.ComponentModel.DataAnnotations;

namespace RestoApp.Models.ViewModels
{
    public class RoleViewModel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(15, ErrorMessage = "Role name should be less then 15 characters")]
        public string Name { get; set; }
        public bool Is_Active { get; set; }
    }
}
