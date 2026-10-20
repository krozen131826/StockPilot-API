
namespace Stockpilot.Domain.Common;

public class AuditableEntity
{
    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }


    // Soft Delete
    public bool IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public string? DeletedBy { get; set; }
}
