using System;

namespace GateSale.Models
{
    public class UserLockerDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid LockerId { get; set; }
        public LockerDto Locker { get; set; } = null!;
        public bool IsFavorite { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSellerDropoff { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }

    public class AddLockerRequest
    {
        public Guid? LockerId { get; set; }
        public string? LockerCode { get; set; }
    }
}
