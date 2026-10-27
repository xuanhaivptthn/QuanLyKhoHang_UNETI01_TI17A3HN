// ==========================================
// XỬ LÝ AJAX VÀ JQUERY CHO DỰ ÁN QUẢN LÝ KHO
// ==========================================

$(document).ready(function () {

    // 1. Tự động gửi kèm Token bảo mật CSRF cho các yêu cầu POST
    $.ajaxSetup({
        beforeSend: function (xhr, settings) {
            if (settings.type === 'POST') {
                var token = $('input[name="__RequestVerificationToken"]').first().val();
                if (token) {
                    xhr.setRequestHeader('RequestVerificationToken', token);
                }
            }
        }
    });

    // 2. Hàm dùng chung: Tải lại dữ liệu bảng và phân trang bằng AJAX
    function taiDuLieuAjax(url, thamSo) {
        var form = $('form[method="get"]').first();

        // Làm mờ bảng trong lúc đang tải dữ liệu
        form.nextAll().css('opacity', '0.5');

        $.ajax({
            url: url,
            type: 'GET',
            data: thamSo,
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (html) {
                var trangMoi = $(html);
                var formMoi = trangMoi.find('form[method="get"]').first();

                // Cập nhật lại nội dung bảng và thanh phân trang mới
                if (formMoi.length > 0) {
                    form.nextAll().remove();
                    form.after(formMoi.nextAll());
                }

                // Cập nhật URL trên thanh địa chỉ trình duyệt
                if (window.history && window.history.replaceState) {
                    var urlMoi = url;
                    if (thamSo) {
                        urlMoi += (url.indexOf('?') === -1 ? '?' : '&') + thamSo;
                    }
                    window.history.replaceState(null, '', urlMoi);
                }
            },
            error: function () {
                form.nextAll().css('opacity', '1');
                alert('Không thể tải dữ liệu, vui lòng thử lại.');
            }
        });
    }

    // 3. Xử lý Tìm kiếm và Lọc khi bấm nút Submit (hoặc nhấn Enter)
    $('form[method="get"]').submit(function (e) {
        if ($('table').length === 0) return; // Nếu trang không có bảng thì bỏ qua

        e.preventDefault();
        var form = $(this);

        // Khi tìm kiếm mới thì luôn đưa về trang 1
        form.find('input[name="page"], input[name="pageNumber"]').val('1');

        var url = form.attr('action') || window.location.pathname;
        var duLieu = form.serialize();
        taiDuLieuAjax(url, duLieu);
    });

    // 4. Xử lý Phân trang khi bấm vào các số trang
    $(document).on('click', '.pagination a', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        if (url && url !== '#' && url !== 'javascript:void(0)') {
            taiDuLieuAjax(url, null);
        }
    });

    // 5. Xử lý nút "Tất cả" để xóa bộ lọc tìm kiếm
    $(document).on('click', 'form[method="get"] a:contains("Tất cả")', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        var form = $(this).closest('form');

        // Xóa trắng các ô nhập tìm kiếm
        form.find('input[type="text"], input[type="search"], input[type="number"]').val('');
        form.find('select').val('');

        taiDuLieuAjax(url, null);
    });

    // 6. Kiểm tra trùng Tên đăng nhập khi tạo tài khoản (sự kiện blur)
    $('#TenDangNhap, input[name="TenDangNhap"]').blur(function () {
        var input = $(this);
        var tenDangNhap = input.val().trim();
        var thongBaoLoi = $('span[data-valmsg-for="TenDangNhap"]');

        if (tenDangNhap !== '') {
            $.get('/TaiKhoans/CheckTenDangNhap', { username: tenDangNhap }, function (res) {
                if (res && res.exists) {
                    thongBaoLoi.text('Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác.');
                } else {
                    if (thongBaoLoi.text().indexOf('đã tồn tại') !== -1) {
                        thongBaoLoi.text('');
                    }
                }
            });
        }
    });

    // 7. Xử lý Đăng nhập bằng AJAX
    $('form[action*="DangNhap"], form[action*="dangnhap"]').submit(function (e) {
        e.preventDefault();
        var form = $(this);
        var url = form.attr('action') || window.location.href;
        var duLieu = form.serialize();

        $.post(url, duLieu, function (res) {
            if (res && res.success) {
                window.location.href = res.redirectUrl || '/';
            } else {
                var loi = (res && res.message) ? res.message : 'Tài khoản hoặc mật khẩu không chính xác.';
                var hopLoi = $('.text-danger').first();
                if (hopLoi.length) {
                    hopLoi.html('<ul><li>' + loi + '</li></ul>');
                } else {
                    alert(loi);
                }
            }
        });
    });

    // 8. Tự động tính Thành tiền = Số lượng * Đơn giá khi nhập số
    $(document).on('input', 'input[name*="SoLuong"], input[name*="DonGia"]', function () {
        var form = $(this).closest('form');
        var soLuong = parseFloat(form.find('input[name*="SoLuong"]').val()) || 0;
        var donGia = parseFloat(form.find('input[name*="DonGia"]').val()) || 0;
        var thanhTien = soLuong * donGia;

        form.find('input[name*="ThanhTien"]').val(thanhTien.toLocaleString('vi-VN'));
    });

    // 9. Kiểm tra số lượng tồn kho khi chọn hàng hóa ở phiếu xuất
    $(document).on('change', '#MaHang, select[name="MaHang"]', function () {
        var select = $(this);
        var maHang = select.val();
        var maKho = $('#MaKho, select[name="MaKho"]').val();

        if (maHang && maKho) {
            $.get('/ChiTietPhieuXuats/GetTonKhoHienTai', { maKho: maKho, maHang: maHang }, function (res) {
                if (res && res.success) {
                    $('#thongBaoTonKho').remove();
                    var thongBao = res.tonKho > 0
                        ? '<div id="thongBaoTonKho" class="text-success small mt-1">Tồn kho khả dụng: ' + res.tonKho + '</div>'
                        : '<div id="thongBaoTonKho" class="text-danger small mt-1">Đã hết hàng trong kho!</div>';
                    select.after(thongBao);
                }
            });
        }
    });

    // 10. Tự động cập nhật số liệu Dashboard sau mỗi 30 giây
    if (window.location.pathname.toLowerCase().indexOf('/thongke') !== -1) {
        setInterval(function () {
            $.get('/ThongKe/GetDashboardData', function (res) {
                if (res && res.success) {
                    $('.card:contains("Tổng Hàng Hóa") .card-title').text(res.tongSoHangHoa);
                    $('.card:contains("Kho Đang Hoạt Động") .card-title').text(res.tongSoKho);
                    $('.card:contains("Phiếu Nhập") .card-title').text(res.soPhieuNhapTrongNgay);
                    $('.card:contains("Phiếu Xuất") .card-title').text(res.soPhieuXuatTrongNgay);
                    $('.alert-danger strong').text(res.soHangHet);
                    $('.alert-warning strong').text(res.soHangSapHet);
                }
            });
        }, 30000);
    }

});
