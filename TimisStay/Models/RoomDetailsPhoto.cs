using System;
using System.Collections.Generic;

namespace TimisStay.Models;

public partial class RoomDetailsPhoto
{
    public int PhotoId { get; set; }

    public int RoomId { get; set; }

    public string? Category { get; set; }

    public string PhotoPath { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;
}
