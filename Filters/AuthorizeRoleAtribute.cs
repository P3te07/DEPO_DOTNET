using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace Proiect_ASPDOTNET.Filters
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly UserRole[] _allowedRoles;

        public AuthorizeRoleAttribute(params UserRole[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            if (!AuthHelper.IsAuthenticated(session))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            var userRole = AuthHelper.GetCurrentUserRole(session);
            if (userRole == null || !_allowedRoles.Contains(userRole.Value))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}