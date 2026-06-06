using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;
using TimeSight.Models.Order;

namespace TimeSight.Services;

public class OrderChoresService
{
    public List<Chore> OrderChores(List<Chore> chores, List<Domain> domains)
    {
        Dictionary<Guid, Chore> choresDic = chores.ToDictionary(
            c => (Guid)c.Id,
            c => c);
        Dictionary<Guid, Domain> domainsDic = domains.ToDictionary(
             d => (Guid)d.Id,
             d => d);
        List<ChoreDomain> choreDomains = [.. chores.SelectMany(c => c.ChoreDomains)];

        List<ChoreScoreForDomain> scoreForDomains = [ .. choreDomains.Select(cd =>
        {
            return new ChoreScoreForDomain()
            {
                ChoreId = cd.ChoreId,
                DomainId = cd.DomainId,
                Score = GetScoreThatGivesChoreForDomain(
                    choresDic.GetValueOrDefault(cd.ChoreId),
                    domainsDic.GetValueOrDefault(cd.DomainId))
            };
        })];

        List<DomainScoreComputation> domainsScore = [.. domains.Select(d =>
        {
            return new DomainScoreComputation(){
                DomainId=(Guid)d.Id,
                CurrentScore= d.CurrentDoneScore
            };
        })];

        List<Guid> sortedChores = [];


        while (scoreForDomains.Count != 0 && domainsScore.Count != 0)
        {
            DomainScoreComputation leastDoneDomain = domainsScore.MinBy(d => d.CurrentScore)!;
            // TODO  : manage case when no chores is done with this domain and add one to say that this domain needs tasks

            ICollection<ChoreScoreForDomain> scoresForThisDomain = [.. scoreForDomains
                .Where(s => s.DomainId == leastDoneDomain.DomainId)];
            if (scoresForThisDomain.Count == 0)
            {
                domainsScore.Remove(leastDoneDomain);
                continue;
            }

            ChoreScoreForDomain maximumScoreForThisDomain = scoresForThisDomain.MaxBy(s => s.Score);
            Guid choreIdToDo = maximumScoreForThisDomain.ChoreId;
            sortedChores.Add(choreIdToDo);
            ICollection<ChoreScoreForDomain> scoresForThisChore = [.. scoreForDomains.Where(s => s.ChoreId == choreIdToDo)];
            foreach (var score in scoresForThisChore)
            {
                //update score of domain
                domainsScore.First(d => d.DomainId == score.DomainId).CurrentScore += score.Score;
                scoreForDomains.Remove(score);
            }
        }

        List<Guid> missingChoresId = [.. choresDic.Keys.Where(id => !sortedChores.Contains(id))];
        sortedChores.AddRange(missingChoresId);

        return [.. sortedChores.Select(s => choresDic.GetValueOrDefault(s))];
    }
    private int GetScoreThatGivesChoreForDomain(Chore chore, Domain domain)
    {
        ChoreDomain? cd = chore.ChoreDomains.FirstOrDefault(c => c.IsMadeOf(chore, domain));
        if (cd == null)
            return 0;

        return cd.LinkIntensity + chore.Duration + chore.Significance;
    }
}
