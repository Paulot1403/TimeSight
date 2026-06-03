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
        this.UserId = chore.UserId;
        this.ChoreId = chore.Id.Value;
        this.DomainId = domain.Id.Value;
        this.Chore = chore;
        this.Domain = domain;
    }
    public Guid UserId { get; set; }
    public Guid ChoreId { get; set; }
    public Guid DomainId { get; set; }
    public int Intensity { get; set; } = 2;

    public Chore? Chore { get; set; } = null!;
    public Domain? Domain { get; set; } = null!;
}
