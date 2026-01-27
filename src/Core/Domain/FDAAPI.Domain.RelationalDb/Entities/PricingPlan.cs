using FDAAPI.Domain.RelationalDb.Entities.Base;
using FDAAPI.Domain.RelationalDb.Enums;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class PricingPlan : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
    {
        public string Code { get; set; } = string.Empty; // FREE, PREMIUM, MONITOR
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal PriceMonth { get; set; }
        public decimal PriceYear { get; set; }
        public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free;
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }

        // Audit
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        [JsonIgnore]
        public virtual ICollection<UserSubscription>? UserSubscriptions { get; set; }
    }
}