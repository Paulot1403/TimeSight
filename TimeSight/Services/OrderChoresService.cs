using TimeSight.Models;
using TimeSight.Models.Order;

namespace TimeSight.Services;

public class OrderChoresService
{
    public List<Guid> OrderChores(IDictionary<Guid, Chore> choresDic, List<Domain> domains)
    {
        Dictionary<Guid, Domain> domainsDic = domains.ToDictionary(
             d => (Guid)d.Id,
             d => d);

        List<ChorePriorityForDomain> prioritiesForDomains = [ .. choresDic.Values.SelectMany(originalChore =>
        {
            var choreDomainsAssociated = originalChore.GetRootOfThis().ChoreDomains;

            return choreDomainsAssociated.Select(cd=>{
                Domain domain = domainsDic.GetValueOrDefault(cd.DomainId);
                int doneScore = originalChore.GetScoreForDomain(domain);
                int priorityScore = GetPriorityScore(originalChore,domain,doneScore);

                return new ChorePriorityForDomain()
                {
                    ChoreId = originalChore.Id,
                    DomainId = cd.DomainId,
                    Priority = priorityScore,
                    DoneScore = doneScore
                };
            });

        })];

        List<DomainPriorityComputation> domainsScore = [.. domains.Select(d =>
        {
            return new DomainPriorityComputation(){
                DomainId=(Guid)d.Id,
                CurrentScore= d.DoneScore,
                DomainImportance=d.Importance
            };
        })];

        List<Guid> sortedChores = [];


        while (prioritiesForDomains.Count != 0 && domainsScore.Count != 0)
        {
            DomainPriorityComputation prioritizedDomain = domainsScore.MaxBy(d => d.Priority)!;

            ICollection<ChorePriorityForDomain> prioritizedChoresForThisDomain = [.. prioritiesForDomains
                .Where(s => s.DomainId == prioritizedDomain.DomainId)];
            if (prioritizedChoresForThisDomain.Count == 0)
            {
                domainsScore.Remove(prioritizedDomain);
                continue;
            }

            ChorePriorityForDomain maximumPriorityForThisDomain = prioritizedChoresForThisDomain.MaxBy(s => s.Priority);
            Guid choreIdToDo = maximumPriorityForThisDomain.ChoreId;
            sortedChores.Add(choreIdToDo);
            ICollection<ChorePriorityForDomain> prioritiesForThisChore = [.. prioritiesForDomains.Where(s => s.ChoreId == choreIdToDo)];
            foreach (var priority in prioritiesForThisChore)
            {
                //update score of domain
                domainsScore.First(d => d.DomainId == priority.DomainId).CurrentScore += priority.DoneScore;
                prioritiesForDomains.Remove(priority);
            }
        }

        List<Guid> missingChoresId = [.. choresDic.Keys.Where(id => !sortedChores.Contains(id))];
        sortedChores.AddRange(missingChoresId);

        return sortedChores;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="chore"></param>
    /// <param name="domain"></param>
    /// <param name="doneScore">score que rapporte chore</param>
    /// <returns>La priorité de la tâche. Plus la valeur est elevé plus tâche n'est pas prioritaire</returns>
    private static int GetPriorityScore(Chore chore, Domain domain, int doneScore)
    {
        int durationScore = chore.Duration.HasValue ? chore.Duration.Value : 0;
        int emergency = chore.Emergency;
        return doneScore - durationScore + emergency;
    }

}
