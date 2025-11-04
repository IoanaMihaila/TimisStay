using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TimisStay.Models;

namespace TimisStay.Data;

public partial class TimisStayDbContext : DbContext
{
    public TimisStayDbContext()
    {
    }

    public TimisStayDbContext(DbContextOptions<TimisStayDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomBooking> RoomBookings { get; set; }

    public virtual DbSet<RoomDetailsPhoto> RoomDetailsPhotos { get; set; }

    public virtual DbSet<RoomPhoto> RoomPhotos { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserBooking> UserBookings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Bookings__C6D03BCDC1D25F09");

            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.CheckInDate).HasColumnName("checkInDate");
            entity.Property(e => e.CheckOutDate).HasColumnName("checkOutDate");
            entity.Property(e => e.NrAdults).HasColumnName("nrAdults");
            entity.Property(e => e.NrChildren).HasColumnName("nrChildren");
            entity.Property(e => e.RoomType)
                .HasMaxLength(100)
                .HasColumnName("roomType");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .HasColumnName("status");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("totalPrice");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__2ECD6E046E4B4711");

            entity.Property(e => e.ReviewId).HasColumnName("reviewId");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Reviews_Users");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Rooms__6C3BF5BE9463F6CE");

            entity.HasIndex(e => e.RoomNumber, "UQ__Rooms__AE53101C9411AECE").IsUnique();

            entity.Property(e => e.RoomId).HasColumnName("roomId");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsAvailable)
                .HasDefaultValue(true)
                .HasColumnName("isAvailable");
            entity.Property(e => e.MaxAdults).HasColumnName("maxAdults");
            entity.Property(e => e.MaxChildren).HasColumnName("maxChildren");
            entity.Property(e => e.Photo)
                .HasMaxLength(255)
                .HasColumnName("photo");
            entity.Property(e => e.PricePerNight)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("pricePerNight");
            entity.Property(e => e.RoomNumber)
                .HasMaxLength(20)
                .HasColumnName("roomNumber");
            entity.Property(e => e.RoomType)
                .HasMaxLength(100)
                .HasColumnName("roomType");
        });

        modelBuilder.Entity<RoomBooking>(entity =>
        {
            entity.HasKey(e => e.RoomBookingId).HasName("PK__RoomBook__0FB382E1F5FCC525");

            entity.Property(e => e.RoomBookingId).HasColumnName("roomBookingId");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.RoomId).HasColumnName("roomId");

            entity.HasOne(d => d.Booking).WithMany(p => p.RoomBookings)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_RoomBookings_Bookings");

            entity.HasOne(d => d.Room).WithMany(p => p.RoomBookings)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_RoomBookings_Rooms");
        });

        modelBuilder.Entity<RoomDetailsPhoto>(entity =>
        {
            entity.HasKey(e => e.PhotoId).HasName("PK__RoomDeta__547C322D776D8578");

            entity.Property(e => e.PhotoId).HasColumnName("photoId");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasColumnName("category");
            entity.Property(e => e.PhotoPath)
                .HasMaxLength(255)
                .HasColumnName("photoPath");
            entity.Property(e => e.RoomId).HasColumnName("roomId");

            entity.HasOne(d => d.Room).WithMany(p => p.RoomDetailsPhotos)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_RoomDetailsPhotos_Rooms");
        });

        modelBuilder.Entity<RoomPhoto>(entity =>
        {
            entity.HasKey(e => e.PhotoId).HasName("PK__room_pho__547C322DD6FCF17B");

            entity.ToTable("room_photos");

            entity.Property(e => e.PhotoId).HasColumnName("photoId");
            entity.Property(e => e.PhotoPath)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("photoPath");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__CB9A1CFF2F6F8B6C");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E616477DEAD9A").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("firstName");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("lastName");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("passwordHash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValue("User")
                .HasColumnName("role");
        });

        modelBuilder.Entity<UserBooking>(entity =>
        {
            entity.HasKey(e => e.UserBookingId).HasName("PK__UserBook__5B3F2EE447BEF506");

            entity.Property(e => e.UserBookingId).HasColumnName("userBookingId");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Booking).WithMany(p => p.UserBookings)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_UserBookings_Bookings");

            entity.HasOne(d => d.User).WithMany(p => p.UserBookings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserBookings_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
