using System.ComponentModel.DataAnnotations;

namespace TechMoveGLMS.Models.Entities
{
    public class Client
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string ContactDetails { get; set; }
        
        [Required]
        public string Region { get; set; }
        
        // Navigation property - One client has many contracts
        public List<Contract> Contracts { get; set; } = new();
        public List<User> Users { get; set; } = new();
    }
}