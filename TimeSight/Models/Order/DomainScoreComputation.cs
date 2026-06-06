using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeSight.Models.Order;

public class DomainScoreComputation()
{
    public Guid DomainId { get; set; }
    public int CurrentScore { get; set; }
}
