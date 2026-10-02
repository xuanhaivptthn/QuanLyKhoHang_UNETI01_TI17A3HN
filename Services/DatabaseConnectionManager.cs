using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

// Họ và tên: Trần Xuân Hải
// Mã sinh viên: 23103100135
// Phụ trách Module 1: Tài khoản, Đăng nhập, Phân quyền, Loại hàng & Đơn vị tính

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Services
{
    public class DatabaseConnectionManager
    {
        private readonly string? _remoteConnectionString;
        private readonly string? _localConnectionString;
        private readonly DatabaseInfo _databaseInfo;
        private readonly ILogger<DatabaseConnectionManager> _logger;

        private volatile bool _isRemoteActive;
        private readonly object _lock = new();

        public DatabaseConnectionManager(
            IConfiguration configuration,
            DatabaseInfo databaseInfo,
            ILogger<DatabaseConnectionManager> logger)
        {
            _databaseInfo = databaseInfo;
            _logger = logger;

            var rawRemote = configuration.GetConnectionString("RemoteDb");
            var rawLocal = configuration.GetConnectionString("LocalDb");

            if (!string.IsNullOrWhiteSpace(rawRemote))
            {
                try
                {
                    var builder = new SqlConnectionStringBuilder(rawRemote)
                    {
                        TrustServerCertificate = true, // Khắc phục lỗi SSL handshake trên Linux OpenSSL
                        ConnectTimeout = 15
                    };
                    _remoteConnectionString = builder.ConnectionString;
                }
                catch
                {
                    _remoteConnectionString = rawRemote;
                }
            }

            _localConnectionString = rawLocal;

            // Kiểm tra kết nối ban đầu một cách nhanh chóng (timeout 5s thay vì treo 60s)
            InitializeConnection();
        }

        private void InitializeConnection()
        {
            if (!string.IsNullOrWhiteSpace(_remoteConnectionString))
            {
                if (TestConnection(_remoteConnectionString, timeoutSeconds: 5))
                {
                    ActivateRemote();
                    _logger.LogInformation("[Database] Đã kết nối thành công tới RemoteDb ({Server}).", _databaseInfo.ServerName);
                    return;
                }
                _logger.LogWarning("[Database] Không thể kết nối tới RemoteDb khi khởi động. Tự động chuyển sang sử dụng LocalDb.");
            }

            ActivateLocal("Khởi động ban đầu");
        }

        public string GetActiveConnectionString()
        {
            return _isRemoteActive && !string.IsNullOrWhiteSpace(_remoteConnectionString)
                ? _remoteConnectionString
                : (_localConnectionString ?? string.Empty);
        }

        public bool IsRemoteActive => _isRemoteActive;

        public void SwitchToLocal(Exception? reason = null)
        {
            lock (_lock)
            {
                if (_isRemoteActive)
                {
                    _logger.LogWarning(reason, "[Database] Mất kết nối tới RemoteDb ({Message}). Đang tự động chuyển sang CSDL Local.", reason?.Message);
                    ActivateLocal(reason?.Message);
                }
            }
        }

        public bool TrySwitchToRemote()
        {
            if (string.IsNullOrWhiteSpace(_remoteConnectionString))
                return false;

            if (TestConnection(_remoteConnectionString, timeoutSeconds: 5))
            {
                lock (_lock)
                {
                    if (!_isRemoteActive)
                    {
                        ActivateRemote();
                        _logger.LogInformation("[Database] Đã kết nối lại được RemoteDb ({Server})! Tự động chuyển sang CSDL server chung.", _databaseInfo.ServerName);
                    }
                }
                return true;
            }
            return false;
        }

        private void ActivateRemote()
        {
            _isRemoteActive = true;
            string server = "", db = "";
            try
            {
                var csb = new SqlConnectionStringBuilder(_remoteConnectionString);
                server = csb.DataSource;
                db = csb.InitialCatalog;
            }
            catch { }

            _databaseInfo.Update(isRemote: true, serverName: server, databaseName: db);
        }

        private void ActivateLocal(string? reason)
        {
            _isRemoteActive = false;
            string server = "", db = "";
            try
            {
                var csb = new SqlConnectionStringBuilder(_localConnectionString);
                server = csb.DataSource;
                db = csb.InitialCatalog;
            }
            catch { }

            _databaseInfo.Update(isRemote: false, serverName: server, databaseName: db);
        }

        public static bool TestConnection(string connectionString, int timeoutSeconds = 5)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    ConnectTimeout = timeoutSeconds,
                    TrustServerCertificate = true
                };
                using var conn = new SqlConnection(builder.ConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.CommandTimeout = timeoutSeconds;
                cmd.ExecuteScalar();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
