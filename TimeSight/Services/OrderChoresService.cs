using TimeSight.Models;

namespace TimeSight.Services;

public class OrderChoresService
{
    public List<Guid> OrderChores(IDictionary<Guid, Chore> choresDic)
    {
        return [.. choresDic.Values.OrderByDescending(c => GetPriorityScore(c)).Select(c => c.Id)];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="chore"></param>
    /// <param name="domain"></param>
    /// <returns>La priorité de la tâche. Plus la valeur est elevé plus tâche n'est pas prioritaire</returns>
    private static int GetPriorityScore(Chore chore)
    {
        int durationScore = chore.Duration.HasValue ? chore.Duration.Value : 0;
        int importance = chore.Importance;
        return importance - durationScore;
    }

}
