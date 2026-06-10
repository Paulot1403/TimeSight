using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeSight.Models;

public class Domain
{
    public required Guid UserId { get; set; }
    public required Guid WorkspaceId { get; set; }
    public required string Name { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
    public int Importance { get; set; } = 3;
    public Guid? Id { get; set; }
    public List<ChoreDomain> ChoreDomains { get; set; } = [];

    public int CurrentDoneScore { get; set; }
}
