namespace BankSysAPI.Models
{
    public class CustomerUpdateRequest
    {
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

        public List<string>? Emails { get; set; }
    }
}
