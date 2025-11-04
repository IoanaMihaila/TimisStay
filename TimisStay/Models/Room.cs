using System;
using System.Collections.Generic;

namespace TimisStay.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string RoomType { get; set; } = null!;

    public string? Description { get; set; }

    public decimal PricePerNight { get; set; }

    public int MaxAdults { get; set; }

    public int MaxChildren { get; set; }

    public string? Photo { get; set; }

    public string RoomNumber { get; set; } = null!;

    public bool IsAvailable { get; set; }

    public virtual ICollection<RoomBooking> RoomBookings { get; set; } = new List<RoomBooking>();

    public virtual ICollection<RoomDetailsPhoto> RoomDetailsPhotos { get; set; } = new List<RoomDetailsPhoto>();
}
