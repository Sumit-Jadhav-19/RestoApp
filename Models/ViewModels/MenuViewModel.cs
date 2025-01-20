using Microsoft.AspNetCore.Mvc.Rendering;
using RestoApp.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace RestoApp.Models.ViewModels
{
    public class MenuViewModel
    {
        public int MenuId { get; set; }
        [Required]
        [MaxLength(100, ErrorMessage = "Munu name must be less then 100 characters")]
        [Display(Name = "Menu Name")]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = " Please select a category.")]
        [Range(1, int.MaxValue, ErrorMessage = " Please select a valid category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        // Navigation Property
        public string? CategoryName { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Display(Name ="Half menu Available")]
        public bool IsHalfAvailable { get; set; } 
        [Display(Name ="Half menu price")]
        public decimal? HalfPrice { get; set; }
        public decimal? Discount { get; set; }
        public string? ImagePath { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int? PreparationTime { get; set; }
        public bool IsSpecial { get; set; } = false;
        public int? SpicyLevel { get; set; }
        public string? AllergenInfo { get; set; }
        public int? StockQuantity { get; set; }
        public string? MenuType { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
        public string? CreatedUser { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public string? ModifiedUser { get; set; }
        public bool IsActive { get; set; } = true;

        public IFormFile? ImageFile { get; set; } // For image upload
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }
}
