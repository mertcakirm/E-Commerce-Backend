namespace eCommerce.Core.Entities;

public class AuditLog : BaseEntity
{
    public int? UserId { get; set; }
    public string Action { get; set; }
    public string EntityName { get; set; }
    public int? EntityId { get; set; }
    public string Details { get; set; }
    public bool IsSeen { get; set; } = false;
}