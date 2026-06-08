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

    /// <summary>
    /// Id from database
    /// </summary>
    public Guid? Id { get; set; }
    public bool IsDone { get; set; } = false;

    /// <summary>
    /// De 1 à <see cref="Chore.MAX_DURATION"/> ?
    /// </summary>
    public int Duration { get; set; } = 2;

    public string? Description { get; set; }
    public Guid? ParentChoreId { get; set; }

    public List<ChoreDomain> ChoreDomains { get; set; } = [];

    public DateTime? DoneAt { get; set; }

    // public DateTime? ScheduledStartDate { get; set; }
    // public DateTime? ScheduledEndDate { get; set; }


    public bool IsSubtask => ParentChoreId != null;

    public Chore? GetRootOfThis(ICollection<Chore> chores)
    {
        if (ParentChoreId == null)
        { return this; }

        Chore parentChore = chores.FirstOrDefault(c => c.Id == ParentChoreId);

        if (parentChore == null)
        {
            throw new ArgumentException("chores ne contient pas le parent");
        }

        return parentChore.GetRootOfThis(chores);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="domain"></param>
    /// <returns>Le score que donne cette tâche au domaine</returns>
    public int GetScoreForDomain(Domain domain)
    {
        ChoreDomain? cd = ChoreDomains.FirstOrDefault(c => c.IsMadeOf(this, domain));
        if (cd == null)
            return 0;

        return cd.LinkIntensity;
    }
}
