using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeSight.Models;

public class Chore
{
    public const int MAX_DURATION = 4;
    public const int MAX_EMERGENCY = 5;
    public const int DAYS_BEFORE_EMERGENCY_START = 30;

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

    /// <summary>
    /// Minutes depuis minuit (heure locale). Ex: 480 = 8h00.
    /// </summary>
    public int? RecurrenceResetTime { get; set; }

    /// <summary>
    /// Bitmask : Lun=1, Mar=2, Mer=4, Jeu=8, Ven=16, Sam=32, Dim=64. null = tous les jours.
    /// </summary>
    public int? RecurrenceDaysOfWeek { get; set; }

    public DateOnly? Deadline { get; set; }

    public int Emergency
    {
        get
        {
            if (Deadline is null) return 0;
            int daysUntil = Deadline.Value.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
            if (daysUntil > DAYS_BEFORE_EMERGENCY_START) return 0;
            return Math.Max(0, MAX_EMERGENCY * (DAYS_BEFORE_EMERGENCY_START - daysUntil) / DAYS_BEFORE_EMERGENCY_START);
        }
    }

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
