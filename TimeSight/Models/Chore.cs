using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeSight.Models;

public class Chore
{
    public required Guid UserId { get; set; }
    public required string Name { get; set; }

    /// <summary>
    /// Id from database
    /// </summary>
    public Guid? Id { get; set; }
    public bool IsDone { get; set; } = false;

    /// <summary>
    /// De 1 à 4 ?
    /// </summary>
    public int? Significance { get; set; }
    /// <summary>
    /// De 1 à 4 ?
    /// </summary>
    public int? Duration { get; set; }

    public List<ChoreDomain> ChoreDomains { get; set; } = [];

    public DateTime? DoneAt { get; set; }

    // public DateTime? ScheduledStartDate { get; set; }
    // public DateTime? ScheduledEndDate { get; set; }

}