using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;

namespace TimeSight.Models;

public class Chore
{
    public const int MAX_DURATION = 4;
    public required Guid UserId { get; set; }
    public required string Name { get; set; }
    public required Guid Id { get; set; }
    public bool IsDone { get; set; } = false;

    /// <summary>
    /// De 1 à <see cref="Chore.MAX_DURATION"/> ?
    /// </summary>
    public int Duration { get; set; } = 2;

    public string? Description { get; set; }
    public Guid? ParentChoreId { get; set; }

    public List<ChoreDomain> ChoreDomains { get; set; } = [];

    public DateTime? DoneAt { get; set; }

    public Chore? ParentChore
    {
        get; set
        {
            ParentChoreId = value?.Id;
            field = value;
        }
    }

    public ICollection<Chore> Children { get; set; } = [];

    public bool IsSubtask => ParentChoreId != null;

    public Chore GetRootOfThis()
    {
        if (ParentChoreId == null)
        { return this; }

        if (ParentChore == null)
        {
            throw new ArgumentException("chores ne contient pas le parent");
        }

        return ParentChore.GetRootOfThis();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="domain"></param>
    /// <returns>Le score que donne cette tâche au domaine</returns>
    public int GetScoreForDomain(Domain domain)
    {
        ChoreDomain? cd = GetRootOfThis().ChoreDomains.FirstOrDefault(c => c.IsMadeOf(this, domain));
        if (cd == null)
            return 0;

        return cd.LinkIntensity;
    }
}
