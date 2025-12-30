using System.ComponentModel.DataAnnotations;

namespace GateSale.Core.Entities
{
    public class SchoolRequest
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(500)]
        public required string SchoolName { get; set; }
        
        [Required]
        [StringLength(255)]
        public required string City { get; set; }
        
        [Required]
        [EmailAddress]
        public required string RequesterEmail { get; set; }
        
        [StringLength(500)]
        public string? ContactPerson { get; set; }
        
        public SchoolRequestStatus Status { get; set; } = SchoolRequestStatus.Pending;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }

    public enum SchoolRequestStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
