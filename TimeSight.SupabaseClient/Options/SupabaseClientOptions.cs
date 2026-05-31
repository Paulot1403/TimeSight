using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeSight.SupabaseClient.Options;

public class SupabaseClientOptions
{
    [Required]
    public string Url { get; set; } = null!;

    [Required]
    public string AnonKey { get; set; } = null!;
}
