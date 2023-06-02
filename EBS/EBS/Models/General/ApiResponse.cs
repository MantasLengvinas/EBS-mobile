using System;
namespace EBS.Models.General
{
	public class ApiResponse<T> where T : class
	{
		public T Data { get; set; }
		public Error Error { get; set; }
		public bool IsSuccess { get; set; }

		public ApiResponse() {}

		public ApiResponse(Error error)
		{
			Error = error;
			IsSuccess = false;
		}

		public ApiResponse(T data)
		{
			Data = data;
			IsSuccess = true;
		}
	}

	public class Error
	{
		public string Message { get; set; }
		public int StatusCode { get; set; }
	}
}

