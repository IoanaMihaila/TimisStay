using System;
using System.Collections.Generic;

namespace TimisStay.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public virtual User User { get; set; } = null!;
}
