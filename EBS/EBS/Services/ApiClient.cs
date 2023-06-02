using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EBS.Models.General;
using Newtonsoft.Json;

namespace EBS.Services
{
	public class ApiClient
	{
		private HttpClient httpClient;

		public ApiClient()
		{
			httpClient = new HttpClient();
			//httpClient.BaseAddress = new Uri("http://localhost:50001/");
		}

		private async Task<HttpResponseMessage> GetHttpResponseMessage<T>(HttpMethod method, string url, T content = null) where T : class
		{
			HttpRequestMessage requestMessage = new HttpRequestMessage(method, url);

			if(method != HttpMethod.Get)
			{
				JsonContent jsonContent = JsonContent.Create(content);
				requestMessage.Content = jsonContent;
			}

			return await httpClient.SendAsync(requestMessage);
		}

		private async Task<ApiResponse<T>> SendAsync<T>(string url, HttpMethod method, T content = null) where T : class, new()
		{
			T data = new T();

			HttpStatusCode statusCode = HttpStatusCode.OK;

			try
			{
                HttpResponseMessage response = await GetHttpResponseMessage<T>(method, url, content);

				statusCode = response.StatusCode;

				response.EnsureSuccessStatusCode();

                string stringContent = await response.Content.ReadAsStringAsync();

                if (stringContent != null)
                {
                    ApiResponse<T> resContent = JsonConvert.DeserializeObject<ApiResponse<T>>(stringContent);

                    if (resContent != null)
                        data = resContent.Data;

                    return new ApiResponse<T>(data);
                }

            }
			catch(Exception e)
			{
				Error err = new Error()
				{
					Message = e.Message,
					StatusCode = (int)statusCode
				};

				return new ApiResponse<T>(err);
			}

			return new ApiResponse<T>(data);
        }

		private class NoContent { };

		public async Task<ApiResponse<T>> GetAsync<T>(string url) where T : class, new()
		{
			return await SendAsync<T>(url, HttpMethod.Get);
		}

		public async Task<ApiResponse<T>> PostAsync<T>(string url, T data) where T : class, new()
		{
			return await SendAsync(url, HttpMethod.Post, data);
		}

		public async Task PutAsync(string url)
		{
			await SendAsync<NoContent>(url, HttpMethod.Put);
		}

		public async Task DeleteAsync(string url)
		{
			await SendAsync<NoContent>(url, HttpMethod.Delete);
		}
	}
}

