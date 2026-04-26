using ETLService.Security.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ETLService.Security
{
	public class AuthControllerAttribute : ActionFilterAttribute
	{
		private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1000, 1000); // Permite solo una operación concurrente
		public Permissions[] PermissionsList { get; set; }
		public AuthControllerAttribute()
		{
			PermissionsList = [];
		}
		public AuthControllerAttribute(params Permissions[] permissionsList)
		{
			PermissionsList = permissionsList ?? [];
		}
		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			// Intentar entrar al semáforo
			await _semaphore.WaitAsync();
			try
			{
				string? token = context.HttpContext.Session.GetString("sessionKey");
				
				// Autenticación
				if (!AuthNetCore.Authenticate(token))
				{
					context.Result = new ObjectResult(new Authenticate
					{
						AuthVal = false
					});
					return;
				}
				// Permisos
				if (PermissionsList.Length > 0 && !AuthNetCore.HavePermission(token, PermissionsList))
				{
					context.Result = new ObjectResult(new Authenticate
					{
						AuthVal = false,
						Message = "Inaccessible resource"
					})
					{ StatusCode = 401 };

					return;
				}
				try
				{
					var user = AuthNetCore.User(token);
					var httpContext = context.HttpContext;

					string method = httpContext.Request.Method;
					string path = httpContext.Request.Path;
					string time = DateTime.Now.ToString("HH:mm:ss");
					string message = $"{time} - {method} {path}";
					// Aquí, 'user' es un objeto anónimo o UserModel. Se necesitaría un casting o una propiedad UserID en UserModel.
					// Por ahora, se comenta la línea que usa UserId.
					//LoggerServices.AddAction(message, user.UserId ?? -1);
				}
				catch (System.Exception)
				{
					context.Result = new ObjectResult(new Authenticate
					{
						AuthVal = false,
						Message = "Inaccessible resource"
					})
					{ StatusCode = 403 };
					return;
				}
			}
			finally
			{
				_semaphore.Release();
			}
		}
	}

	class Authenticate
	{
		public bool AuthVal { get; set; }
		public string? Message { get; set; }
	}
}
