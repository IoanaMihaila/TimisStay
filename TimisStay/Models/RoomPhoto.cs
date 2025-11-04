using System;
using System.Collections.Generic;

namespace TimisStay.Models;

public partial class RoomPhoto
{
    public int PhotoId { get; set; }

    public string PhotoPath { get; set; } = null!;
}
