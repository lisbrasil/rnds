using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace GrupoSymExemploTokenCSharp
{
	public class TokenRnds
	{
		[JsonPropertyName("access_token")]
		public string AccessToken { get; set; }
		[JsonPropertyName("scope")]
		public string Scope { get; set; }
		[JsonPropertyName("token_type")]
		public string TokenType { get; set; }
		[JsonPropertyName("expires_in")]
		public long ExpiresIn { get; set; }
	}
}
