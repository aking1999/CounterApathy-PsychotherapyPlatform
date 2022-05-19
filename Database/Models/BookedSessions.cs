using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class BookedSessions
    {
        public string Id { get; set; }
        public string SessionId { get; set; }
        public string TherapistId { get; set; }
        public string TherapistFirstName { get; set; }
        public string TherapistLastName { get; set; }
        public string TherapistEmail { get; set; }
        public string TherapistPhoneNumber { get; set; }
        public string TherapistStreet { get; set; }
        public string TherapistHouseNumber { get; set; }
        public string TherapistCity { get; set; }
        public string TherapistCountry { get; set; }
        public string TherapistPostalCode { get; set; }
        public string ClientId { get; set; }
        public string ClientFirstName { get; set; }
        public string ClientLastName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhoneNumber { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Type { get; set; }
        public DateTime SessionDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime BookingDate { get; set; }
        public string ContactMethodName { get; set; }
        public string ContactInfo { get; set; }
        public int Status { get; set; }
        public bool? TherapistIsPaid { get; set; }
    }
}
