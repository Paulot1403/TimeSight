using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeSight.Models;

public class Domain
{
    public const int MAX_IMPORTANCE = 10;
    public required Guid UserId { get; set; }
    public required Guid WorkspaceId { get; set; }
    public required string Name { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
    public int Importance { get; set; } = 3;
    public Guid? Id { get; set; }
    public List<ChoreDomain> ChoreDomains { get; set; } = [];
    /// <summary>
    /// Score fait pour le domaine. Vient de la durée des tâches faites pour le domaine.
    /// </summary>
    public double DoneScore { get; set; } = 1;

    public static float ComputePriority(double domainScore, int domainImportance)
    {
        return domainImportance / (float)domainScore;
    }

}
