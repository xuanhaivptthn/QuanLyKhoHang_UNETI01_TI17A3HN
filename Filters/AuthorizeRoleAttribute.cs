using System;
using System.Linq;
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

            var session = context.HttpContext.Session;
            var maTaiKhoan = session.GetInt32("MaTaiKhoan");

            // 2. Chưa đăng nhập -> Chuyển hướng về trang Đăng nhập kèm returnUrl
            if (maTaiKhoan == null)
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("DangNhap", "TaiKhoans", new { returnUrl });
                return;
            }

            // 3. Đã đăng nhập nhưng kiểm tra vai trò (Role) nếu có yêu cầu
            if (_roles.Length > 0)
            {
                var userRole = session.GetString("VaiTro") ?? string.Empty;
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
