using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSysAPI.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string? Email { get; set; }  // Opsiyonel, geçici olarak kullanılabilir.

        public string? Name { get; set; }
        public string? TaxNumber { get; set; }
        public string? TaxOffice { get; set; }
        public string? PersonType { get; set; }
        public string? Citizenship { get; set; }
        public string? Accomodation { get; set; }

        public string? Language { get; set; }
        public string? RecordingChannel { get; set; }

        public int? CitizenshipCountryId { get; set; }
        public int? AccomodationCountryId { get; set; }

        public string? Status { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? LastUpdateUser { get; set; }
        public string? HostIp { get; set; }
        public string? CustomerNumber { get; set; }

        public ICollection<CustomerEmail> Emails { get; set; } = new List<CustomerEmail>();
    }

}
