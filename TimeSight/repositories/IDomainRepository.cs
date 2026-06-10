using TimeSight.Models;

namespace TimeSight.repositories;

public interface IDomainRepository
{
    Task<List<Domain>> GetDomainsAsync(Guid workspaceId);
    Task<Domain> GetDomainAsync(Guid domainId);

    Task<Domain> CreateDomainAsync(Domain domain);
    Task<Domain> UpdateDomainAsync(Domain domain);

    Task DeleteDomainAsync(Guid domainId);

}
