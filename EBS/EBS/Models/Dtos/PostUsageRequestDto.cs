using System;
namespace EBS.Models.Dtos
{
	public class PostUsageRequestDto
	{
        public int UsageId { get; set; }
        public DateTime? OnDate { get; set; }
        public double ElectricityDue { get; set; }
        public bool IsPaid { get; set; }
        public int AddressId { get; set; }
        public double AmountDue { get; set; }
        public double PaidTariff { get; set; }
    }
}

