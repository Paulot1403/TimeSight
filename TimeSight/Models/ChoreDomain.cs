using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeSight.Models;

public class ChoreDomain
{
    public required Guid UserId { get; set; }
    public required int ChoreId { get; set; }
    public required int DomainId { get; set; }
    public int Intensity { get; set; } = 2;

    public Chore? Chore { get; set; } = null!;
    public Domain? Domain { get; set; } = null!;
}
