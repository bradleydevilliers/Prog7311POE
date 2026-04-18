using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMoveGLMS.Models.Entities
{
    public class ServiceRequest
    {
        [Key]
        public int Id { get; set; }
        
        // Foreign Key to Contract
        public int ContractId { get; set; }
        
        [ForeignKey("ContractId")]
        public Contract? Contract { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        public string Status { get; set; } = "Draft"; // "Draft", "Active", "Completed", "Cancelled"
        
        [Required]
        public string ServiceLevel { get; set; }
        
        [DataType(DataType.Currency)]
        public decimal LocalCost { get; set; } // Stored in ZAR
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}