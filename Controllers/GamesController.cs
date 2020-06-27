using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shortener.Controllers
{
	[Route("shortener")]
    [ApiController]
    public class GamesController : Controller
    {
		[HttpGet]
		public IActionResult getRoot()
		{
			return Json("hello");
		}

		[HttpGet]
		[Route("all")]
		public IEnumerable<ShortUrl> getAllUrls()
		{
			using (var db = new LiteDB.LiteDatabase("Data/Urls.db"))
			{
				return db.GetCollection<ShortUrl>().FindAll()
					.OrderBy(url => url.Created)
					.ToList();				
			}
		}

		[HttpPost]
		public IActionResult PostURL([FromBody] string url)
		{
			try
			{
				if (!url.Contains("http"))
				{
					url = "http://" + url;
				}

				using(var db = new LiteDB.LiteDatabase("Data/Urls.db"))
                {
					if (db.GetCollection<ShortUrl>().Exists(u => u.ShortenedURL == url))
					{
						Response.StatusCode = 405;
						return Json(new URLResponse() 
						{ 
							url = url, 
							status = "already shortened", 
							token = null 
						});
					}
				}
				
				Shortener shortURL = new Shortener(url);
				return Json(shortURL.Token);
			}
			catch (Exception ex)
			{
				if (ex.Message == "URL already exists")
				{
					Response.StatusCode = 400;

					using (var db = new LiteDB.LiteDatabase("Data/Urls.db"))
					{
						return Json(new URLResponse()
						{
							url = url,
							status = "URL already exists",
							token = db.GetCollection<ShortUrl>().Find(u => u.URL == url).FirstOrDefault().Token
						});
					}
				}
				throw new Exception(ex.Message);
			}			
		}
		
		[HttpGet]
		[Route("{token}")]
		public IActionResult NixRedirect([FromRoute] string token)
		{
			using (var db = new LiteDB.LiteDatabase("Data/Urls.db"))
			{
				string url = db.GetCollection<ShortUrl>().FindOne(u => u.Token == token).URL;

				Redirect(url);

				return Ok(url);
			}			
		}
	}
}
