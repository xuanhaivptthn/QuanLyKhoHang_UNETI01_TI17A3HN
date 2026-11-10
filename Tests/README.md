# Hệ Thống Kiểm Thử Tự Động (Data-Driven Automation Testing Framework)

Sub-project kiểm thử tự động toàn diện cho hệ thống **Quản Lý Kho Hàng UNETI (ASP.NET Core 10 MVC)**.

---

## 1. Công Nghệ & Kiến Trúc

- **Test Runner**: [NUnit 4.3](https://nunit.org/)
- **UI & E2E Engine**: [Microsoft Playwright](https://playwright.dev/dotnet/)
- **Data Reader**: [ClosedXML](https://github.com/ClosedXML/ClosedXML) đọc trực tiếp `TestCase_QuanLyKhoHang_ASPNetCore10.xlsx`
- **Assertions**: [FluentAssertions](https://fluentassertions.com/)
- **Báo cáo kiểm thử**: [Allure Framework](https://allurereport.org/) (`Allure.NUnit`)
- **CI/CD Pipeline**: GitHub Actions (`.github/workflows/test-pipeline.yml` & `discord-notify.yml`)

---

## 2. Cấu Trúc Dự Án (`Tests/`)

```
Tests/
├── TestData/
│   └── TestCase_QuanLyKhoHang_ASPNetCore10.xlsx   # Ma trận 204 testcase gốc
├── Core/
│   ├── TestCaseModel.cs                           # POCO Model mapping cột Excel
│   ├── ExcelDataReaderEngine.cs                   # Dynamic Data-Driven Provider
│   ├── BaseTest.cs                                # Quản lý Playwright Browser/Context/Page
│   └── TestReportListener.cs                      # Lắng nghe trạng thái test, chụp screenshot khi fail
├── Locators/                                      # Page Object Model (POM) Locators
│   ├── AuthLocators.cs                            # Selectors cho đăng nhập, đổi mật khẩu, phân quyền
│   └── InventoryLocators.cs                       # Selectors cho kho, hàng hóa, nhập/xuất kho
├── Modules/                                       # Test Suites phân chia theo Module Excel
│   ├── M1_TaiKhoanTests.cs                        # Module 1: Tài khoản & Phân quyền
│   ├── M2_KhoHangTests.cs                         # Module 2: Quản lý Kho & Hàng hóa
│   ├── M3_NhapKhoTests.cs                         # Module 3: Quản lý Nhập kho
│   ├── M4_XuatKhoTests.cs                         # Module 4: Quản lý Xuất kho
│   ├── M5_TonKhoTests.cs                          # Module 5: Tồn kho & Thống kê
│   └── M6_TichHopTests.cs                         # Module 6: Tích hợp & Phi chức năng
├── appsettings.test.json                          # Cấu hình môi trường (BaseUrl, Timeout, Headless)
├── QuanLyKhoHang.Automation.csproj                # Project file .NET 10
└── Run-Tests.ps1                                  # Script PowerShell tiện ích chạy test
```

---

## 3. Độ Bao Phủ Testcase (204 Testcases)

Hệ thống tự động nạp toàn bộ **204 testcases** từ sheet `Testcase`:
- **Module M1 (Tài khoản & Phân quyền)**: TC_M1_001 -> TC_M1_030
- **Module M2 (Kho & Hàng hóa)**: TC_M2_001 -> TC_M2_040
- **Module M3 (Nhập kho)**: TC_M3_001 -> TC_M3_035
- **Module M4 (Xuất kho)**: TC_M4_001 -> TC_M4_035
- **Module M5 (Tồn kho & Thống kê)**: TC_M5_001 -> TC_M5_030
- **Module M6 (Tích hợp & Phi chức năng)**: TC_M6_001 -> TC_M6_034

> [!NOTE]
> Các testcase được đánh dấu "Thủ công" hoặc "Skip" trong file Excel sẽ tự động được NUnit bỏ qua bằng `Assert.Ignore()`, hiển thị chi tiết lý do mà không gây fail build.

---

## 4. Hướng Dẫn Vận Hành

### Liệt kê danh sách tất cả 204 testcases:
```powershell
dotnet test Tests/QuanLyKhoHang.Automation.csproj --list-tests
```

### Chạy kiểm thử bằng PowerShell Script:
```powershell
# Chạy toàn bộ test
.\Tests\Run-Tests.ps1

# Chạy theo Module cụ thể (M1, M2, M3, M4, M5, M6)
.\Tests\Run-Tests.ps1 -Module "M1"

# Chạy 1 testcase cụ thể theo ID
.\Tests\Run-Tests.ps1 -TestCase "TC_M1_001"
```

### Chạy trực tiếp qua `dotnet test`:
```powershell
# Chạy module M1
dotnet test Tests/QuanLyKhoHang.Automation.csproj --filter "TestCategory=M1"

# Chạy 1 testcase cụ thể
dotnet test Tests/QuanLyKhoHang.Automation.csproj --filter "FullyQualifiedName~TC_M1_001"
```

### Xem báo cáo Allure Report:
Sau khi chạy test, kết quả Allure được lưu tại `Tests/allure-results`. Để render giao diện HTML:
```powershell
allure serve Tests/allure-results
```
