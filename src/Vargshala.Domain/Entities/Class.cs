using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class Class : BaseEntity
{
    public Guid BranchId { get; set; }

    // Class Details
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation
    public Branch Branch { get; set; } = null!;
    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
}
