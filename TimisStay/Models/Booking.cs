using System;
using System.Collections.Generic;

namespace TimisStay.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public int NrAdults { get; set; }

    public int NrChildren { get; set; }

    public string RoomType { get; set; } = null!;

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<RoomBooking> RoomBookings { get; set; } = new List<RoomBooking>();

    public virtual ICollection<UserBooking> UserBookings { get; set; } = new List<UserBooking>();
}
