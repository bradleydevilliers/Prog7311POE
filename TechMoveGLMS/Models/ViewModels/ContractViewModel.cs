using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechMoveGLMS.Models.ViewModels
{
    public class ContractViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Client")]
        public int ClientId { get; set; }
        
        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;
        
        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Now.AddYears(1);
        
        [Required]
        [Display(Name = "Service Level")]
        public string ServiceLevel { get; set; } = "Basic";
        
        [Display(Name = "Signed Agreement (PDF only)")]
        public IFormFile? SignedAgreement { get; set; }
        
        public List<SelectListItem>? Clients { get; set; }
        public List<SelectListItem>? ServiceLevels { get; set; }
    }
}