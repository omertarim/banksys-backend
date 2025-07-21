using System.ComponentModel.DataAnnotations;

namespace BankSysAPI.Models
{
    public class CustomerUpdateRequest
    {
        public string? Name { get; set; }
        public string? TaxNumber { get; set; }
        public int? TaxOfficeId { get; set; }
        public int? PersonTypeId { get; set; }
        public int? CitizenshipId { get; set; }
        public int? AccomodationId { get; set; }
        public int? LanguageId { get; set; }
        public string? RecordingChannel { get; set; }

        [Range(100, 999, ErrorMessage = "CitizenshipCountryId 3 haneli olmalı.")]
        public int CitizenshipCountryId { get; set; }
        public int AccomodationCountryId { get; set; }

        public List<string>? Emails { get; set; }
    }
}
