namespace RestoApp.Models.Entities
{
    public class Menu
    {
        public int MenuId { get; set; } 
        public string Name { get; set; } 
        public string? Description { get; set; }
        public int CategoryId { get; set; } 
        public Categories Category { get; set; } 
        public decimal Price { get; set; } 
        public bool IsHalfAvailable { get; set; } = false;
        public decimal HalfPrice { get; set; } = 0;
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
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }

}
