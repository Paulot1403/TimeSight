using TimeSight.Models;

namespace TimeSight.repositories;

public interface IChoreDomainRepository
{
    Task CreateChoreDomainAsync(ChoreDomain choreDomain);
    Task DeleteChoreDomainAsync(Guid choreId, Guid domainId);
    Task UpdateChoreDomainAsync(ChoreDomain choreDomain);

}