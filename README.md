# QuanLyKhoHang_UNETI01_TI17A3HN

## Phân công module

- Module 1: Trần Xuân Hải
- Module 2: Nguyễn Thị Cúc
- Module 3: Lê Văn Hùng
- Module 4: Nguyễn Việt Dũng
- Module 5: Nguyễn Văn Cường

### Thủ tục mỗi khi làm

- Fetch để lấy thông tin thay đổi
- Pull để lấy code từ Github về

### Add package kiểm thử

- dotnet add package xunit
- dotnet add package xunit.runner.visualstudio
- dotnet add package Microsoft.NET.Test.Sdk
- dotnet add package Microsoft.AspNetCore.Mvc.Testing
- Chạy test: dotnet test
- Chạy từng testcase:

# Lọc theo tên hàm C#

dotnet test --filter "FullyQualifiedName~Test_Login_Success - hàm có trong code"

# Lọc chạy toàn bộ testcase thuộc 1 Class

dotnet test --filter "FullyQualifiedName~AuthenticationIntegrationTests lớp có trong code, có thể tự tìm (t lười)"

# Lọc theo Mã Testcase trong DisplayName

dotnet test --filter "DisplayName~TC_M1_001" - đăng nhập admin

# Lọc theo từ khóa mô tả (ví dụ đăng nhập)

dotnet test --filter "DisplayName~Đăng nhập Admin đúng thông tin"

# chạy thành công: succeeded, testcase không thành công: failed, testcase chạy sai mã (ví dụ chạy Display lấy cột Module thay vì lấy cột Tiêu đề):total: 0 và cảnh báo "No test matches the given testcase filter" là do xUnit --filter không thể lọc trực tiếp theo cột Module

Lấy ID testcase,tiêu đề mô tả ở file testcase_summary.txt
Nếu lười có thể chạy dotnet test -t để hiển thị các testcase mà xunit đã ghi nhận
