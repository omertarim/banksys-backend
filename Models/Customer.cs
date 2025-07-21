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
        public int? TaxOfficeId { get; set; }
        public TaxOffice? TaxOffice { get; set; }
        public int PersonTypeId { get; set; }
        public PersonType? PersonType { get; set; }
        public int CitizenshipId { get; set; }
        public Citizenship? Citizenship { get; set; }
        public int? AccomodationId { get; set; }
        public Accomodation? Accomodation { get; set; }

        public int? LanguageId { get; set; }
        public Language? Language { get; set; }
        public string? RecordingChannel { get; set; }

        [Range(100, 999, ErrorMessage = "CitizenshipCountryId 3 haneli olmalı.")]
        public int CitizenshipCountryId { get; set; }
        public int AccomodationCountryId { get; set; }
        
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
