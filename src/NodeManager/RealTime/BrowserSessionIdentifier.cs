namespace NodeManager.RealTime;

public static class BrowserSessionIdentifier
{
	public const string CookieName = "gd92-browser-session";

	private static readonly object ContextItemKey = new();

	public static string Get(HttpContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		return TryGet(context, out var identifier)
			? identifier
			: throw new InvalidOperationException(
				"A browser session identifier was not established for the request.");
	}

	public static bool TryGet(HttpContext context, out string identifier)
	{
		ArgumentNullException.ThrowIfNull(context);

		if (context.Items.TryGetValue(ContextItemKey, out var value) && value is string valueIdentifier)
		{
			identifier = valueIdentifier;
			return true;
		}

		identifier = string.Empty;
		return false;
	}

	public static string GroupName(string identifier)
	{
		ArgumentException.ThrowIfNullOrEmpty(identifier);
		return $"browser-session:{identifier}";
	}

	internal static void Set(HttpContext context, string identifier)
	{
		context.Items[ContextItemKey] = identifier;
	}
}

public sealed class BrowserSessionMiddleware(RequestDelegate next)
{
	private readonly RequestDelegate next = next ?? throw new ArgumentNullException(nameof(next));

	public async Task InvokeAsync(HttpContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		if (!context.Request.Cookies.TryGetValue(
			BrowserSessionIdentifier.CookieName,
			out var identifier) ||
			!Guid.TryParseExact(identifier, "N", out _))
		{
			identifier = Guid.NewGuid().ToString("N");
			context.Response.Cookies.Append(
				BrowserSessionIdentifier.CookieName,
				identifier,
				new CookieOptions
				{
					HttpOnly = true,
					IsEssential = true,
					Path = "/",
					SameSite = SameSiteMode.Strict,
					Secure = true
				});
		}

		BrowserSessionIdentifier.Set(context, identifier);
		await this.next(context);
	}
}
