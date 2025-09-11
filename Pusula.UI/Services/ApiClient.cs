using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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
			try
			{
				var response = await _httpClient.GetAsync(url);
				if (!response.IsSuccessStatusCode)
				{
					Console.WriteLine($"API Error: {response.StatusCode} - {response.ReasonPhrase}");
					var content = await response.Content.ReadAsStringAsync();
					Console.WriteLine($"Response content: {content}");
					return default(T);
				}
				return await response.Content.ReadFromJsonAsync<T>();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"API Exception: {ex.Message}");
				return default(T);
			}
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

		public async Task<HttpResponseMessage> PatchAsync<T>(string url, T body)
		{
			AttachAuth();
			var json = JsonSerializer.Serialize(body);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var request = new HttpRequestMessage(HttpMethod.Patch, url) { Content = content };
			return await _httpClient.SendAsync(request);
		}

		public async Task<HttpResponseMessage> DeleteAsync(string url)
		{
			AttachAuth();
			return await _httpClient.DeleteAsync(url);
		}
	}
}


