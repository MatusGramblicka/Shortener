using LiteDB;
using System;
using System.IO;
using System.Linq;

namespace Shortener
{
	public class Shortener
    {
		public string Token { get; set; }
		private readonly ShortUrl biturl;

		private string GenerateToken()
		{
			string urlsafe = string.Empty;

			Enumerable.Range(48, 75).
				Where(i => i < 58 || i > 64 && i < 91 || i > 96).
				OrderBy(o => new Random().Next())
				.ToList()
				.ForEach(i => urlsafe += Convert.ToChar(i));

			Token = urlsafe.Substring(new Random().Next(0, urlsafe.Length), new Random().Next(2, 6));

			return Token;
		}

		public Shortener(string url)
		{
			using (var db = new LiteDatabase("Data/Urls.db"))
			{
				var urls = db.GetCollection<ShortUrl>();

				while (urls.Exists(u => u.Token == GenerateToken())) ;
				biturl = new ShortUrl()
				{
					Token = Token,
					URL = url,
					ShortenedURL = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseUrl>(File.ReadAllText("App/Config.json")).baseUrl + Token
				};

				if (urls.Exists(u => u.URL == url))
					throw new Exception("URL already exists");

				urls.Insert(biturl);
			}
		}
	}
}
