using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

namespace GrupoSymExemploTokenCSharp
{
	class Program
	{
		public static X509Certificate2 RetornaCertificado(string caminho, string senha)
		{
			try
			{
				return new X509Certificate2(caminho, senha);
			}
			catch { }

			using (X509Store store = new X509Store("MY", StoreLocation.CurrentUser))
			{
				store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

				var scollection = X509Certificate2UI.SelectFromCollection(store.Certificates, "Teste RNDS", "Selecione um certificado", X509SelectionFlag.SingleSelection);

				foreach (var cert in scollection)
				{
					return cert;
				}
			}

			Console.WriteLine("Nenhum certificado selecionado");

			return null;
		}

		static async Task<int> Main(string[] args)
		{
			// Caso queira informar o certificado, use as duas variáveis abaixo. Caso contrário, coloque null ou dados incorretos para selecionar um certificado instalado
			string _caminhoCertificado = @"e:\teste\CertificadoParaTeste.pfx";
			string _senhaCertificado = "123456";

			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls12;
			ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

			var request = (HttpWebRequest)WebRequest.Create("https://ehr-auth-hmg.saude.gov.br/api/token");

			request.Method = "GET";
			request.KeepAlive = true;

			request.AuthenticationLevel = AuthenticationLevel.MutualAuthRequired;

			// Adiciona o certificado do cliente para autenticação mútua
			request.ClientCertificates.Add(RetornaCertificado(_caminhoCertificado, _senhaCertificado));

			var response = (HttpWebResponse)(await request.GetResponseAsync());

			if (response.StatusCode == HttpStatusCode.OK)
			{
				using (var sr = new StreamReader(response.GetResponseStream()))
				{
					var content = await sr.ReadToEndAsync();
					var token = JsonSerializer.Deserialize<TokenRnds>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

					var expira = DateTime.Now.AddMilliseconds(token.ExpiresIn);
					Console.WriteLine("Token retornado com sucesso!");
					Console.WriteLine($"Expira em: {expira}");

					Console.WriteLine();
					Console.WriteLine();
					Console.WriteLine(token.AccessToken);
					Console.WriteLine();
					Console.WriteLine();
				}
			}
			else
			{
				Console.WriteLine($"Falha ao chamar API. Status: {response.StatusCode} - {response.StatusDescription}");
			}

			Console.WriteLine("Pressione qualquer tecla para continuar");
			Console.ReadKey();

			return 0;
		}
	}
}
