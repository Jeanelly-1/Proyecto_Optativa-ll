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
				// El middleware ya validó la autenticación.
				// Aquí solo verificamos permisos si el atributo los requiere.

				// Si no hay permisos requeridos, continuar directamente
				if (PermissionsList.Length == 0)
				{
					await next();
					return;
				}

				// Obtener los permisos cargados por el middleware desde HttpContext.Items
				var cachedPermissions = context.HttpContext.Items["UserPermissions"] as List<string>;

				if (cachedPermissions == null || cachedPermissions.Count == 0)
				{
					context.Result = new ObjectResult(new Authenticate
					{
						AuthVal = false,
						Message = "No se encontraron permisos para el usuario"
					})
					{ StatusCode = 403 };
					return;
				}

				// Verificar si el usuario tiene al menos uno de los permisos requeridos
				bool hasPermission = PermissionsList.Any(rp => cachedPermissions.Contains(rp.ToString()));

				if (!hasPermission)
				{
					context.Result = new ObjectResult(new Authenticate
					{
						AuthVal = false,
						Message = "Recurso inaccesible: permisos insuficientes"
					})
					{ StatusCode = 403 };
					return;
				}

				try
				{
					// Logging de la acción
					var httpContext = context.HttpContext;
					int? userId = httpContext.Items["UserId"] as int?;

					string method = httpContext.Request.Method;
					string path = httpContext.Request.Path;
					string time = DateTime.Now.ToString("HH:mm:ss");
					string message = $"{time} - {method} {path} - UserId: {userId}";
					// Aquí se podría agregar logging real
					// LoggerServices.AddAction(message, userId ?? -1);
				}
				catch (System.Exception)
				{
					// Error en logging no debe bloquear la acción
				}

				await next();
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
