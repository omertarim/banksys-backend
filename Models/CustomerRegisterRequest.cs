using System.ComponentModel.DataAnnotations;

namespace BankSysAPI.Models
{
    public class CustomerRegisterRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string TaxNumber { get; set; } = string.Empty;

        [Required]
        public string TaxOffice { get; set; } = string.Empty;

        [Required]
        public string PersonType { get; set; } = string.Empty;

        [Required]
        public string Citizenship { get; set; } = string.Empty;

        [Required]
        public string Accomodation { get; set; } = string.Empty;

        [Required]
        public string Language { get; set; } = string.Empty;

        [Required]
        public string RecordingChannel { get; set; } = string.Empty;

        public int? CitizenshipCountryId { get; set; }
        public int? AccomodationCountryId { get; set; }
    }
}
