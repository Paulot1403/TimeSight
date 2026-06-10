namespace TimeSight.Models;

public class ChoreDomain
{
    public const int MAXIMUM_LINK_INTENSITY = 6;
    public const int DEFAULT_LINK_INTENSITY = 3;

    public ChoreDomain()
    {

    }
    public ChoreDomain(Chore chore, Domain domain)
    {
        ArgumentNullException.ThrowIfNull(chore.Id, "chore doit avoir un id de fourni");
        ArgumentNullException.ThrowIfNull(domain.Id, "domain doit avoir un id de fourni");

        this.UserId = chore.UserId;
        this.ChoreId = chore.Id;
        this.DomainId = domain.Id.Value;
    }
    public Guid UserId { get; set; }

    public Guid ChoreId { get; set; }
    public Guid DomainId { get; set; }
    public int LinkIntensity { get; set; } = DEFAULT_LINK_INTENSITY;

    public bool IsMadeOf(Chore chore, Domain domain)
    {
        ArgumentNullException.ThrowIfNull(chore.Id, "chore doit avoir un id de fourni");
        ArgumentNullException.ThrowIfNull(domain.Id, "domain doit avoir un id de fourni");

        return this.ChoreId == chore.Id && this.DomainId == domain.Id.Value;
    }
}
