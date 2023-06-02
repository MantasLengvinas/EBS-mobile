using System;
namespace EBS.Models.Dtos
{
	public class GetTariffResponseDto
	{
        public int TariffId { get; set; }
        public DateTime? RegistryDate { get; set; }
        public double Rate { get; set; }
        public int ProviderId { get; set; }
        public bool IsBusiness { get; set; }
    }
}

