using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Họ và tên: Trần Xuân Hải
// Mã sinh viên: 23103100135
// Phụ trách Module 1: Tài khoản, Đăng nhập, Phân quyền, Loại hàng & Đơn vị tính

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Services
{
    /// <summary>
    /// Background service định kỳ kiểm tra kết nối tới RemoteDb.
    /// Nếu đang chạy LocalDb mà RemoteDb khả dụng -> tự động chuyển sang RemoteDb.
    /// </summary>
    public class DatabaseHealthCheckService : BackgroundService
    {
        private readonly DatabaseConnectionManager _dbManager;
        private readonly ILogger<DatabaseHealthCheckService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(15);

        public DatabaseHealthCheckService(
            DatabaseConnectionManager dbManager,
            ILogger<DatabaseHealthCheckService> logger)
        {
            _dbManager = dbManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);

                    // Nếu hiện tại đang phải dùng CSDL local, tự động thử kết nối lại RemoteDb
                    if (!_dbManager.IsRemoteActive)
                    {
                        _dbManager.TrySwitchToRemote();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogTrace(ex, "[DatabaseHealthCheck] Ngoại lệ khi kiểm tra kết nối định kỳ.");
                }
            }
        }
    }
}
