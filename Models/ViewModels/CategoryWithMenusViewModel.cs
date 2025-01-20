using RestoApp.Models.Entities;

namespace RestoApp.Models.ViewModels
{
    public class CategoryWithMenusViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<CategoryWithMenuViewModel> Menus { get; set; }
    }
    public class CategoryWithMenuViewModel
    {
        public int MenuId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public bool IsHalfAvailable { get; set; }
        public decimal HalfPrice { get; set; }
        public decimal Discount { get; set; }
        public string ImagePath { get; set; }
        public bool IsAvailable { get; set; }
        public int PreparationTime { get; set; }
        public bool IsSpecial { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public bool IsActive { get; set; }
    }
}
