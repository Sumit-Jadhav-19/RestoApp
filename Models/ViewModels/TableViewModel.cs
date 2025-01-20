using Microsoft.AspNetCore.Mvc.Rendering;
using RestoApp.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace RestoApp.Models.ViewModels
{
    public class TableViewModel
    {
        public int Id { get; set; }
        [Required]
        [Display(Name ="Table Name / No")]
        public string Name { get; set; }
        [Required]
        [Display(Name ="Hall")]
        public int HallId { get; set; }
        public Hall? Hall { get; set; }
        public int DataEnteredBy { get; set; }
        public DateTime DataEnteredOn { get; set; } = DateTime.Now;
        public int? DataModifiedBy { get; set; }
        public DateTime? DataModifiedOn { get; set; }
        public bool IsActive { get; set; } = true;

        public List<SelectListItem> Halls { get; set; } = new List<SelectListItem>();
    }
}
