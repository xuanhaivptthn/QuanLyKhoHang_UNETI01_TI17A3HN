namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    // Họ và tên: Trần Xuân Hải
    // Mã sinh viên: 23103100135
    // Phụ trách Module 1: Tài khoản, Đăng nhập, Phân quyền, Loại hàng & Đơn vị tính
    public class DatabaseInfo
    {
        private readonly object _lock = new();
        private bool _isRemote;
        private string _serverName = string.Empty;
        private string _databaseName = string.Empty;

        public bool IsRemote
        {
            get { lock (_lock) return _isRemote; }
            set { lock (_lock) _isRemote = value; }
        }

        public string ServerName
        {
            get { lock (_lock) return _serverName; }
            set { lock (_lock) _serverName = value; }
        }

        public string DatabaseName
        {
            get { lock (_lock) return _databaseName; }
            set { lock (_lock) _databaseName = value; }
        }

        public string DisplayName => IsRemote ? "Remote" : "Local";
        public string Description => $"{DisplayName} DB ({ServerName} / {DatabaseName})";

        public void Update(bool isRemote, string serverName, string databaseName)
        {
            lock (_lock)
            {
                _isRemote = isRemote;
                _serverName = serverName;
                _databaseName = databaseName;
            }
        }
    }
}
