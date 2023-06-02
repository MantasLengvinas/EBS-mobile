using System;
namespace EBS.Models.Dtos
{
	public class GetUserResponseDto
	{
        public int UserId { get; set; }
        public string FullName { get; set; }
        public double Balance { get; set; }
        public bool Business { get; set; }
        public int ClientId { get; set; }
    }
}

