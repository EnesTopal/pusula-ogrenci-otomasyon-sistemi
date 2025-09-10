using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Pusula.UI.Services
{
	public class AuthStateProvider : AuthenticationStateProvider
	{
		private readonly TokenStorage _tokenStorage;

		public AuthStateProvider(TokenStorage tokenStorage)
		{
			_tokenStorage = tokenStorage;
		}

		public void SetToken(string? token)
		{
			_tokenStorage.SetToken(token);
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

		public override Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			var token = _tokenStorage.GetToken();
			if (string.IsNullOrWhiteSpace(token))
			{
				var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
				return Task.FromResult(new AuthenticationState(anonymous));
			}

			var claims = ParseClaimsFromJwt(token);
			var identity = new ClaimsIdentity(claims, "jwt");
			var user = new ClaimsPrincipal(identity);
			return Task.FromResult(new AuthenticationState(user));
		}

		private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
		{
			var payload = jwt.Split('.')[1];
			var jsonBytes = ParseBase64WithoutPadding(payload);
			var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
			var claims = new List<Claim>();
			if (keyValuePairs != null)
			{
				foreach (var kvp in keyValuePairs)
				{
					if (kvp.Value is JsonElement e)
					{
						if (e.ValueKind == JsonValueKind.Array)
						{
							foreach (var item in e.EnumerateArray())
							{
								var value = item.GetString() ?? string.Empty;
								claims.Add(new Claim(kvp.Key, value));
								if (string.Equals(kvp.Key, "role", StringComparison.OrdinalIgnoreCase) || string.Equals(kvp.Key, "roles", StringComparison.OrdinalIgnoreCase))
								{
									claims.Add(new Claim(ClaimTypes.Role, value));
								}
							}
						}
						else
						{
							var value = e.ToString();
							claims.Add(new Claim(kvp.Key, value));
							if (string.Equals(kvp.Key, "role", StringComparison.OrdinalIgnoreCase) || string.Equals(kvp.Key, "roles", StringComparison.OrdinalIgnoreCase))
							{
								claims.Add(new Claim(ClaimTypes.Role, value));
							}
						}
					}
					else if (kvp.Value != null)
					{
						var value = kvp.Value.ToString()!;
						claims.Add(new Claim(kvp.Key, value));
						if (string.Equals(kvp.Key, "role", StringComparison.OrdinalIgnoreCase) || string.Equals(kvp.Key, "roles", StringComparison.OrdinalIgnoreCase))
						{
							claims.Add(new Claim(ClaimTypes.Role, value));
						}
					}
				}
			}
			return claims;
		}

		private static byte[] ParseBase64WithoutPadding(string base64)
		{
			base64 = base64.Replace('-', '+').Replace('_', '/');
			switch (base64.Length % 4)
			{
				case 2: base64 += "=="; break;
				case 3: base64 += "="; break;
			}
			return Convert.FromBase64String(base64);
		}
	}
}


