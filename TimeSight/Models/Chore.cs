using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeSight.Models;

public class Chore
{
    public const int MAX_IMPORTANCE = 10;
    public const int DEFAULT_IMPORTANCE = 3;
    public const int MAX_DURATION = 4;
    public const int MAX_EMERGENCY = 5;


    /// <summary>
    /// Seuils proposés à l'utilisateur pour "dans combien de temps une tâche devient urgente".
    /// </summary>
    public static readonly int[] EmergencyThresholdOptionsDays = [7, 14, 30, 60, 90];

    /// <summary>
    /// Valeur par défaut de <see cref="EmergencyThresholdDays"/> quand la tâche n'en définit pas.
    /// </summary>
    public const int DefaultEmergencyThresholdDays = 30;

    public static string EmergencyThresholdLabel(int days) => days switch
    {
        7 => "1 week",
        14 => "2 weeks",
        30 => "1 month",
        60 => "2 months",
        90 => "3 months",
        _ => $"{days} days"
    };

    public static string DurationLabel(int? duration) => duration switch
    {
        null => "—",
        1 => "5 min",
        2 => "15 min",
        3 => "45 min",
        4 => "2 h",
        _ => $"{duration}"
    };

    /// <summary>
    /// Choix proposés à l'utilisateur pour la récurrence "After interval", en heures.
    /// </summary>
    public static readonly int[] RecurrenceIntervalOptionsHours = [1, 4, 8, 12, 24, 72, 168, 336, 720, 1440];

    public static string RecurrenceIntervalLabel(int hours) => hours switch
    {
        1 => "hour",
        24 => "day",
        72 => "3 days",
        168 => "week",
        336 => "2 weeks",
        720 => "month",
        1440 => "2 months",
        _ when hours % 24 == 0 => $"{hours / 24} days",
        _ => $"{hours} hours"
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

    public int Importance { get; set; } = DEFAULT_IMPORTANCE;

    /// <summary>
    /// De 1 à <see cref="Chore.MAX_DURATION"/>, ou null si pas d'estimation.
    /// </summary>
    public int? Duration { get; set; } = null;

    public string? Description { get; set; }
    public Guid? ParentChoreId { get; set; }

    public List<ChoreDomain> ChoreDomains { get; set; } = [];

    public DateTime? DoneAt { get; set; }

    /// <summary>
    /// Intervalle de la récurrence "After interval", en heures (ex: 1, 4, 24, 168...).
    /// </summary>
    public int? RecurrenceIntervalHours { get; set; }

    /// <summary>
    /// Minutes depuis minuit (heure locale). Ex: 480 = 8h00.
    /// </summary>
    public int? RecurrenceResetTime { get; set; }

    /// <summary>
    /// Bitmask : Lun=1, Mar=2, Mer=4, Jeu=8, Ven=16, Sam=32, Dim=64. null = tous les jours.
    /// </summary>
    public int? RecurrenceDaysOfWeek { get; set; }

    public DateOnly? Deadline { get; set; }

    /// <summary>
    /// Nombre de jours avant la deadline à partir duquel CETTE tâche commence à devenir urgente.
    /// null = utilise <see cref="DefaultEmergencyThresholdDays"/>.
    /// </summary>
    public int? EmergencyThresholdDays { get; set; }

    /// <summary>
    /// Date à laquelle on prévoit de commencer cette tâche.
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Heure de début prévue, en minutes depuis minuit (ex: 540 = 9h00). null = pas d'heure définie.
    /// </summary>
    public int? StartTime { get; set; }

    public int Emergency
    {
        get
        {
            if (Deadline is null) return 0;
            int threshold = EmergencyThresholdDays ?? DefaultEmergencyThresholdDays;
            int daysUntil = Deadline.Value.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
            if (daysUntil > threshold) return 0;
            return Math.Max(0, MAX_EMERGENCY * (threshold - daysUntil) / threshold);
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

}
