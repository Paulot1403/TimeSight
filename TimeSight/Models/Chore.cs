using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeSight.Models;

public class Chore
{
    public const int MAX_DURATION = 4;

    public static string DurationLabel(int? duration) => duration switch
    {
        null => "—",
        1 => "5 min",
        2 => "15 min",
        3 => "45 min",
        4 => "2 h",
        _ => $"{duration}"
    };
    public required Guid UserId { get; set; }
    public required Guid WorkspaceId { get; set; }
    public required string Name { get; set; }
    public required Guid Id { get; set; }
    public bool IsDone
    {
        get; set
        {
            bool isDone = value;
            DoneAt = isDone ? DateTime.UtcNow : null;
            field = isDone;
        }
    } = false;

    /// <summary>
    /// De 1 à <see cref="Chore.MAX_DURATION"/>, ou null si pas d'estimation.
    /// </summary>
    public int? Duration { get; set; } = null;

    public string? Description { get; set; }
    public Guid? ParentChoreId { get; set; }

    public List<ChoreDomain> ChoreDomains { get; set; } = [];

    public DateTime? DoneAt { get; set; }

    public int? RecurrenceIntervalDays { get; set; }

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
        if (ParentChoreId == null) return this;
        if (ParentChore == null) throw new ArgumentException("chores ne contient pas le parent");
        return ParentChore.GetRootOfThis();
    }

    public int CountAllDescendants() =>
        Children.Sum(c => 1 + c.CountAllDescendants());

    public int CountAllDoneDescendants() =>
        Children.Sum(c => (c.IsDone ? 1 : 0) + c.CountAllDoneDescendants());

    /// <summary>
    /// Score ajouté au domaine quand CETTE tâche est complétée.
    /// Sous-tâche : rootScore / totalDescendants. Racine sans enfants : score brut.
    /// </summary>
    public int GetScoreForDomain(Domain domain)
    {
        var root = GetRootOfThis();
        ChoreDomain? cd = root.ChoreDomains.FirstOrDefault(c => c.DomainId == domain.Id!.Value);
        if (cd == null) return 0;
        if (!IsSubtask) return cd.LinkIntensity;
        return cd.LinkIntensity / root.CountAllDescendants();
    }
}
