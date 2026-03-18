1. TONG QUAN
Xây dựng hệ thống Web App quản lý đặt sân Pickleball (9 sân), cho phép khách hàng đặt sân riêng theo giờ hoặc sân chung theo slot (tối đa 8 slot/khung giờ), thanh toán online qua VNPay hoặc chuyển khoản QR ngân hàng, sinh mà check-in tự động, quản lý chính sách hủy linh hoạt và tích điểm thành viên.
II. CHỨC NĂNG CHI TIẾT
1. MODULE TÀI KHOẢN NGƯỜI DÙNG
• Đăng ký tài khoản (Email + mật khẩu).
• Đăng nhập bằng User & Password.
• Quên mật khẩu (gửi link reset có thời hạn).
• Quản lý hồ sơ cá nhân.
• Xem lịch sử đặt sẵn và tải lại mã.
2. MODULE DAT SAN
2.1 Sân riêng theo giờ:
• Đặt toàn bộ sân theo khung giờ
• Chọn ngày và khung giờ qua giao diện lịch.
• Không giới hạn số người chơi.
2.2 Sân chung theo slot:
• Tối đa 8 slot / khung giờ.
• Hiển thị số slot còn trắng (ví dụ: 5/8).
• Cho phép đặt nhiều slot trong một lần thanh toán.
• Mỗi slot được cấp 1 mã riêng.
2.3 Quy tắc hệ thống
• Không cho đặt trùng sân – trùng giờ.
• Không vượt quá 8 slot sân chung.
• Thanh toán thành công mới tạo mà checkin.
• Cho phép thanh toán gộp nhiều booking trong 1 giao dịch.
3. MODULE CẤU HÌNH GIẢI
• Cấu hình giá giờ thường và giờ cao điểm.
• Cấu hình phụ thu cuối tuần (% tăng giá).
• Có thể cấu hình giá riêng cho sân riêng và sân chung
• Thay đổi giá trực tiếp từ Dashboard Admin.
4. MODULE THANH TOÁN
• Tích hợp cổng thanh toán VNPay (Redirect + Webhook xác nhận).
• Hỗ trợ chuyển khoản ngân hàng bằng QR động theo số tiền.
• Trạng thái thanh toán: Pending / Paid / Failed.
• Hệ thống kiểm tra webhook để đảm bảo xác nhận giao dịch thành công.
5. MODULE CHECK-IN
• Mã checkin dạng ký tự random (ví dụ: PB-9X4K2) 
• Lễ tân kiểm tra & xác nhận thủ công trên hệ thống 
• Hệ thống tự vô hiệu hóa mã sau khi check-in
• Reset toàn bộ mã cuối ngày
6. MODULE CHÍNH SÁCH HỦY 8 HOÀN TIỀN 
• Hủy trước ≥ 2 tiếng đủ điều kiện hoàn tiền. 
• Hủy < 2 tiếng: không hoàn tiền.
7. Phân tích phương án hoàn tiền:
• Hoàn 100% qua VNPay: minh bạch nhưng chủ đầu tư chịu phí thanh toán hai lần. 
• Hoàn 95% qua VNPay có phí hủy để giảm rủi ro tài chính.
• Hoàn về vì nội bộ (khuyến nghị): không mất phi cổng thanh toán, giữ dòng tiền trong hệ thống.
8. Đề xuất vận hành:
• Hủy ≥ 2 tiếng: hoàn tự động về ví nội bộ.
• Trường hợp đặc biệt: Admin duyệt hoàn thủ công.
• Có thể áp dụng phí hủy 5–10% để tránh lỗ chi phí thanh toán.
7. MODULE TÍCH ĐIỂM & PHÂN HẠNG THÀNH VIÊN
• Quy đổi: 10.000₫ = 1 điểm.
• Chỉ cộng điểm khi booking hoàn thành.
• Phân hạng: Silver / Gold / Platinum.
• Hạng cao được hưởng ưu đãi giảm giá tương ứng
8. MODULE DASHBOARD QUẢN TRỊ 
• Quản lý sân (bật/tắt, bảo trì).
• Cấu hình giá theo khung giờ.
• Quản lý booking theo ngày tháng.
• Quản lý hoàn tiền và duyệt thủ công. 
• Báo cáo doanh thu theo ngày/ tháng. 
• Xuất báo cáo Excel.