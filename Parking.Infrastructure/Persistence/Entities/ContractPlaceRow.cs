namespace Parking.Infrastructure.Persistence.Entities;

public sealed class ContractPlaceRow
{
    public long ContractId { get; set; }
    public long PlaceId { get; set; }

    public string Status { get; set; } = "Reserved"; // Reserved/BorrowedOut/BorrowedIn
    public long? LinkedContractId { get; set; }
    public DateTimeOffset? LinkedSince { get; set; }

    // nav
    public ContractRow Contract { get; set; } = null!;
    public PlaceRow Place { get; set; } = null!;

    public ContractRow? LinkedContract { get; set; }
}