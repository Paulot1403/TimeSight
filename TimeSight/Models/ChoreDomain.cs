using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase.Gotrue;

namespace TimeSight.Models;

public class ChoreDomain
{
    public ChoreDomain()
    {

    }
    public ChoreDomain(Chore chore, Domain domain)
    {
        ArgumentNullException.ThrowIfNull(chore.Id, "chore doit avoir un id de fourni");
        ArgumentNullException.ThrowIfNull(domain.Id, "domain doit avoir un id de fourni");

        this.UserId = chore.UserId;
        this.ChoreId = chore.Id.Value;
        this.DomainId = domain.Id.Value;
    }
    public Guid UserId { get; set; }
    public Guid ChoreId { get; set; }
    public Guid DomainId { get; set; }
    public int LinkIntensity { get; set; } = 1;

    public bool IsMadeOf(Chore chore, Domain domain)
    {
        ArgumentNullException.ThrowIfNull(chore.Id, "chore doit avoir un id de fourni");
        ArgumentNullException.ThrowIfNull(domain.Id, "domain doit avoir un id de fourni");

        return this.ChoreId == chore.Id.Value && this.DomainId == domain.Id.Value;
    }
}
