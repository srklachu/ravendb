﻿using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Raven.Abstractions.Data;
using Raven.Imports.Newtonsoft.Json;
using Raven.Abstractions;
using Raven.Json.Linq;
using System.Linq;
using Raven.Abstractions.Extensions;

namespace Raven.Database.Server.Security.OAuth
{
	public class AccessToken
	{
		public string Body { get; set; }
		public string Signature { get; set; }

		private bool MatchesSignature(byte[] key)
		{
			var signatureData = Convert.FromBase64String(Signature);
			
			using (var rsa = new RSACryptoServiceProvider())
			{
				rsa.ImportCspBlob(key);
			
				var bodyData = Encoding.Unicode.GetBytes(Body);

				using (var hasher = new SHA1Managed())
				{
					var hash = hasher.ComputeHash(bodyData);

					return rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), signatureData);
				}
			}
		}

		public static bool TryParseBody(byte[] key, string token, out AccessTokenBody body)
		{
			AccessToken accessToken;
			if (TryParse(token, out accessToken) == false)
			{
				body = null;
				return false;
			}

			if (accessToken.MatchesSignature(key) == false)
			{
				body = null;
				return false;
			}

			try
			{
				body = JsonConvert.DeserializeObject<AccessTokenBody>(accessToken.Body);
				return true;
			}
			catch
			{
				body = null;
				return false;
			}
		}

		private static bool TryParse(string token, out AccessToken accessToken)
		{
			try
			{
				accessToken = JsonConvert.DeserializeObject<AccessToken>(token);
				return true;
			}
			catch
			{
				accessToken = null;
				return false;
			}
		}

		public static AccessToken Create(byte[] key, string userId, string[] databases)
		{
			var authorizedDatabases = (databases ?? new string[0]).Select(tenantId => new DatabaseAccess { TenantId = tenantId, ReadOnly = false }).ToList();

			return Create(key, new AccessTokenBody { UserId = userId, AuthorizedDatabases = authorizedDatabases });
		}


		public static AccessToken Create(byte[] key, AccessTokenBody tokenBody)
		{
			tokenBody.Issued = (SystemTime.UtcNow - DateTime.MinValue).TotalMilliseconds;

			var body = RavenJObject.FromObject(tokenBody)
					.ToString(Formatting.None);

			var signature = Sign(body, key);

			return new AccessToken { Body = body, Signature = signature };
		}

		public static string Sign(string body, byte[] key)
		{
			var data = Encoding.Unicode.GetBytes(body);
			using (var hasher = new SHA1Managed())
			using(var rsa = new RSACryptoServiceProvider())
			{
				var hash = hasher.ComputeHash(data);

				rsa.ImportCspBlob(key);

				return Convert.ToBase64String(rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1")));
			}
		}

		public string Serialize()
		{
			return RavenJObject.FromObject(this).ToString(Formatting.None);
		}

	}
}