# Scripts SQL — FlippedClassroom

Chạy trực tiếp trên **SQL Server Management Studio (SSMS)**, không cần backend tự tạo bảng.

## Cách chạy

1. Mở SSMS → kết nối server → chọn database **FlippedClassroom**
2. Chạy lần lượt:
   - `01_FlippedClassroom_Lecturer.sql` (ClassSessions + dữ liệu lớp mẫu + bảng workspace)
   - `02_MaterialCompletions.sql` (bảng hoàn thành học liệu + phản hồi mẫu)
3. Nhấn **Execute (F5)** từng file

## Nội dung script

| Phần | Mô tả |
|------|--------|
| 1 | Tạo bảng `ClassSessions` (lịch + chi tiết buổi học) |
| 2 | Dữ liệu mẫu: học kỳ, môn SWP391, lớp `SWP391-CLC01` gán **GV123456** |
| 3 | 3 buổi học mẫu |
| 4 | *(Tùy chọn)* Bảng `LearningMaterials`, `Assignments`, `Submissions`, `SupportFeedbacks`, `DiscussionThreads` |
| 5 | Query kiểm tra kết quả |

## Đăng nhập test

| Role | Id | Email |
|------|-----|-------|
| Giảng viên | GV123456 | giangvien@system.com |
| Sinh viên | HE187159 | sinhvien@system.com |

Mật khẩu: theo cột `PasswordHash` trong bảng `Users` (hiện tại `123456`).

## Lưu ý

- Script dùng `IF NOT EXISTS` — có thể chạy lại nhiều lần an toàn.
- Nếu đã có lớp khác, chỉnh `@ClassId` / `LecturerId` trong script cho phù hợp.
