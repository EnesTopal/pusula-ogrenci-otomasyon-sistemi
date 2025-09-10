using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Pusula.UI.Services
{
	public class TokenStorage
	{
		private string? _token;
		public void SetToken(string? token) => _token = token;
		public string? GetToken() => _token;
	}

	public class ApiClient
	{
		private readonly HttpClient _httpClient;
		private readonly TokenStorage _tokenStorage;

		public ApiClient(HttpClient httpClient, TokenStorage tokenStorage)
		{
			_httpClient = httpClient;
			_tokenStorage = tokenStorage;
		}

		private void AttachAuth()
		{
			var token = _tokenStorage.GetToken();
			_httpClient.DefaultRequestHeaders.Authorization = token != null
				? new AuthenticationHeaderValue("Bearer", token)
				: null;
		}

		public async Task<T?> GetAsync<T>(string url)
		{
			AttachAuth();
			return await _httpClient.GetFromJsonAsync<T>(url);
		}

		public async Task<HttpResponseMessage> PostAsync<T>(string url, T body)
		{
			AttachAuth();
			return await _httpClient.PostAsJsonAsync(url, body);
		}

		public async Task<HttpResponseMessage> PutAsync<T>(string url, T body)
		{
			AttachAuth();
			return await _httpClient.PutAsJsonAsync(url, body);
		}

		public async Task<HttpResponseMessage> DeleteAsync(string url)
		{
			AttachAuth();
			return await _httpClient.DeleteAsync(url);
		}
	}
}


