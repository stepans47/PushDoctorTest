using Microsoft.EntityFrameworkCore;
using PDR.PatientBooking.Data.Models;

namespace PDR.PatientBooking.Data
{
    /* It's better to encapsulate Context using Repository pattern and not expose it to any other layers */
    public class PatientBookingContext : DbContext
    {
        public PatientBookingContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Order> Order { get; set; }
        public DbSet<Patient> Patient { get; set; }
        public DbSet<Doctor> Doctor { get; set; }
        public DbSet<Clinic> Clinic { get; set; }
    }
}
