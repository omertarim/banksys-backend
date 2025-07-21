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
        public int TaxOfficeId { get; set; } 

        [Required]
        public int PersonTypeId { get; set; }

        [Required]
        public int CitizenshipId { get; set; }

        [Required]
        public int AccomodationId { get; set; }

        [Required]
        public int LanguageId { get; set; }

        [Required]
        public string RecordingChannel { get; set; } = string.Empty;

        [Range(100, 999, ErrorMessage = "CitizenshipCountryId 3 haneli olmalı.")]
        public int CitizenshipCountryId { get; set; }
        public int AccomodationCountryId { get; set; }
    }
}
