using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeSight.Models.Order;

public class DomainPriorityComputation()
{
    public Guid DomainId { get; set; }

    public int DomainImportance { get; set; }

    /// <summary>
    /// Score anticipé après chaque réalisation de tâches
    /// </summary>
    public int CurrentScore { get; set; }

    public float Priority => DomainImportance / (float)CurrentScore;

}
