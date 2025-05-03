using System;
using System.Text;
using BepInEx.Logging;

namespace FearMe
{
	public static class Utils
	{
		public static void LogException(Exception e, string note = null)
		{
			var messageBuilder = new StringBuilder();
			if (!string.IsNullOrWhiteSpace(note))
				messageBuilder.AppendLine(note);

			var innerException = e;
			while (innerException != null)
			{
				if (!string.IsNullOrWhiteSpace(note))
					messageBuilder.Append("\t");
				messageBuilder.AppendLine(innerException.Message ?? string.Empty);

				innerException = innerException.InnerException;
			}

			Jotunn.Logger.LogError(messageBuilder.ToString());
		}
	}
}
