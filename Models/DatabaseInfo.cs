namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    public class DatabaseInfo
    {
        public bool IsRemote { get; set; }
        public string ServerName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;

        public string DisplayName => IsRemote ? "Remote" : "Local";
        public string Description => $"{DisplayName} DB ({ServerName} / {DatabaseName})";
    }
}
