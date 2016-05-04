using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.PlatformAbstractions;

namespace ODataSample.Web
{
	public class ReflectionTypeLoadExceptionMiddleware
	{
		private readonly RequestDelegate _next;

		public ReflectionTypeLoadExceptionMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await this._next(context);
			}
			catch (ReflectionTypeLoadException ex)
			{
			}
		}
	}
}