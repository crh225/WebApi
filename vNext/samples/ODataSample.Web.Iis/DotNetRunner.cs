using System;
using System.Collections.Generic;
using System.Threading;

namespace ODataSample.Web
{
	public static class DotNetRunner
	{
		public static string[] AppStart(params string[] args)
		{
			var newArgs = new List<string>();
			if (args.Length > 0)
			{
				var prefix = "wait:";
				foreach (var arg in args)
				{
					if (arg.StartsWith(prefix))
					{
						var wait = Int32.Parse(arg.Substring(prefix.Length));
						Thread.Sleep(wait);
					}
					else
					{
						newArgs.Add(arg);
					}
				}
			}
			return newArgs.ToArray();
		}
	}
}