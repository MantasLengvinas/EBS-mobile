using System;
namespace EBS.Models.Dtos
{
	public class GetUsageResponseDto
	{
        public int UsageId { get; set; }
        public DateTime? OnDate { get; set; }
        public decimal? ElectricityDue { get; set; }
        public bool IsPaid { get; set; }
        public int AddressId { get; set; }
        public decimal? AmountDue { get; set; }
        public decimal? PaidTariff { get; set; }
        public string NoData { get; set; } = "Nėra neapmokėtų sąskaitų";
        public bool ShowNoDataMessage { get; set; }
        public bool Checked { get; set; }
    }
}

