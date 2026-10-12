using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

// Họ và tên: Trần Xuân Hải
// Mã sinh viên: 23103100135
// Phụ trách Module 1: Tài khoản, Đăng nhập, Phân quyền, Loại hàng & Đơn vị tính

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles ?? Array.Empty<string>();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 1. Cho phép bỏ qua nếu Action hoặc Controller có [AllowAnonymous]
            var hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any()
                || context.Filters.Any(f => f is IAllowAnonymousFilter);

            if (hasAllowAnonymous)
            {
                base.OnActionExecuting(context);
                return;
            }

            // Nếu Action có định nghĩa AuthorizeRoleAttribute riêng có cấu hình vai trò,
            // cho phép Action-level attribute nắm quyền kiểm tra (thay thế Class-level)
            var actionSpecificRoles = context.ActionDescriptor.EndpointMetadata
                .OfType<AuthorizeRoleAttribute>()
                .Where(f => f._roles.Length > 0)
                .ToList();

            if (actionSpecificRoles.Count > 0 && this._roles.Length > 0 && !actionSpecificRoles.Contains(this))
            {
                base.OnActionExecuting(context);
                return;
            }

            var httpContext = context.HttpContext;
            var session = httpContext.Session;
            var maTaiKhoan = session.GetInt32("MaTaiKhoan");
            var isAuthenticated = (httpContext.User.Identity?.IsAuthenticated == true) || (maTaiKhoan != null);

            // 2. Chưa đăng nhập -> Chuyển hướng về trang Đăng nhập kèm returnUrl
            if (!isAuthenticated)
            {
                var returnUrl = httpContext.Request.Path + httpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("DangNhap", "TaiKhoans", new { returnUrl });
                return;
            }

            // 3. Đã đăng nhập nhưng kiểm tra vai trò (Role) nếu có yêu cầu
            if (_roles.Length > 0)
            {
                var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value
                    ?? session.GetString("VaiTro")
                    ?? string.Empty;

                if (!_roles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
                {
                    // Không đủ quyền -> Chuyển sang trang AccessDenied
                    context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
