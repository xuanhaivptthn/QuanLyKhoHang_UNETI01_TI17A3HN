using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;

// Họ và tên: Trần Xuân Hải
// Mã sinh viên: 23103100135
// Phụ trách Module 1: Tài khoản, Đăng nhập, Phân quyền, Loại hàng & Đơn vị tính

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Services
{
    /// <summary>
    /// Bắt các lỗi kết nối mở CSDL từ EF Core để tự động chuyển sang CSDL Local.
    /// </summary>
    public class DatabaseFailoverConnectionInterceptor : DbConnectionInterceptor
    {
        private readonly DatabaseConnectionManager _dbManager;

        public DatabaseFailoverConnectionInterceptor(DatabaseConnectionManager dbManager)
        {
            _dbManager = dbManager;
        }

        public override void ConnectionFailed(DbConnection connection, ConnectionErrorEventData eventData)
        {
            if (_dbManager.IsRemoteActive && eventData.Exception is SqlException)
            {
                _dbManager.SwitchToLocal(eventData.Exception);
            }
            base.ConnectionFailed(connection, eventData);
        }

        public override Task ConnectionFailedAsync(DbConnection connection, ConnectionErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            if (_dbManager.IsRemoteActive && eventData.Exception is SqlException)
            {
                _dbManager.SwitchToLocal(eventData.Exception);
            }
            return base.ConnectionFailedAsync(connection, eventData, cancellationToken);
        }
    }

    /// <summary>
    /// Bắt các lỗi thực thi câu lệnh CSDL từ EF Core khi mạng chập chờn / bắt tay SSL thất bại.
    /// </summary>
    public class DatabaseFailoverCommandInterceptor : DbCommandInterceptor
    {
        private readonly DatabaseConnectionManager _dbManager;

        public DatabaseFailoverCommandInterceptor(DatabaseConnectionManager dbManager)
        {
            _dbManager = dbManager;
        }

        public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
        {
            if (_dbManager.IsRemoteActive && eventData.Exception is SqlException)
            {
                _dbManager.SwitchToLocal(eventData.Exception);
            }
            base.CommandFailed(command, eventData);
        }

        public override Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            if (_dbManager.IsRemoteActive && eventData.Exception is SqlException)
            {
                _dbManager.SwitchToLocal(eventData.Exception);
            }
            return base.CommandFailedAsync(command, eventData, cancellationToken);
        }
    }
}
