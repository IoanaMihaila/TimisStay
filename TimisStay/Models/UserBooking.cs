using System;
using System.Collections.Generic;

namespace TimisStay.Models;

public partial class UserBooking
{
    public int UserBookingId { get; set; }

    public int UserId { get; set; }

    public int BookingId { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
