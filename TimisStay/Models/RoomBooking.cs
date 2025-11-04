using System;
using System.Collections.Generic;

namespace TimisStay.Models;

public partial class RoomBooking
{
    public int RoomBookingId { get; set; }

    public int BookingId { get; set; }

    public int RoomId { get; set; }

    public decimal Price { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;
}
