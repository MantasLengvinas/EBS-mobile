using System;
using System.Collections.Generic;

namespace EBS.Models.Dtos
{
	public class GetUnpaidUsagesResponseDto
	{
		public List<GetUsageResponseDto> Usages { get; set; }
		public decimal PaymentSum { get; set; }
	}
}

