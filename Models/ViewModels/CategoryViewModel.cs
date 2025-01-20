using System.ComponentModel.DataAnnotations;

namespace RestoApp.Models.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100, ErrorMessage = "Category Name should less then 100 characters")]
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime DataEnteredOn { get; set; } = DateTime.Now;
        public int DataEnteredBy { get; set; }
        public DateTime? DataModifiedOn { get; set; }
        public int? DataModifiedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
