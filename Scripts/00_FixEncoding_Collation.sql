/*
================================================================================
  FIX ENCODING / COLLATION — FlippedClassroom Database
  -------------------------------------------------------
  Chạy script này để sửa lỗi chữ tiếng Việt bị mã hóa sai (garbled text)
  Ví dụ: "MÃ n Ã¡»±..." thay vì "Môn đồ án..."

  NGUYÊN NHÂN:
    - Database collation không phải Vietnamese_CI_AS hoặc SQL_Latin1_General_CP1_CI_AS
    - Dữ liệu cũ được INSERT mà không có prefix N'' (non-Unicode string)

  CÁCH SỬA:
    Bước 1: Kiểm tra và sửa collation của database
    Bước 2: Cập nhật lại dữ liệu bị lỗi encoding

  QUAN TRỌNG: Chạy script này trước tất cả các script khác nếu database mới tạo
================================================================================
*/

USE master;
GO

/* ============================================================================
   BƯỚC 1: Kiểm tra Collation hiện tại
   ============================================================================ */
SELECT
    name AS DatabaseName,
    collation_name AS CurrentCollation,
    CASE 
        WHEN collation_name LIKE '%Vietnamese%' THEN N'✓ OK - Hỗ trợ tiếng Việt'
        WHEN collation_name = 'SQL_Latin1_General_CP1_CI_AS' THEN N'✓ OK - Hỗ trợ Unicode (NVARCHAR)'
        ELSE N'⚠ Cần kiểm tra — có thể gây lỗi encoding'
    END AS Status
FROM sys.databases
WHERE name = 'FlippedClassroom';
GO

/* ============================================================================
   BƯỚC 2: Đổi Collation của Database (nếu cần)
   Nếu collation hiện tại OK → bỏ qua bước này
   ============================================================================ */
-- Đóng tất cả kết nối khác vào database
ALTER DATABASE [FlippedClassroom]
SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Đổi sang collation hỗ trợ Unicode đầy đủ
ALTER DATABASE [FlippedClassroom]
COLLATE SQL_Latin1_General_CP1_CI_AS;
GO

-- Mở lại multi-user mode
ALTER DATABASE [FlippedClassroom]
SET MULTI_USER;
GO

USE [FlippedClassroom];
GO

/* ============================================================================
   BƯỚC 3: Cập nhật lại dữ liệu bị lỗi encoding trong bảng Courses
   ============================================================================ */
-- Xóa dữ liệu cũ bị lỗi và INSERT lại đúng với prefix N''
IF EXISTS (SELECT 1 FROM dbo.Courses WHERE Code = 'SWP391')
BEGIN
    UPDATE dbo.Courses
    SET
        Name        = N'Software Project',
        Description = N'Môn đồ án phần mềm — mô hình Flipped Classroom'
    WHERE Code = 'SWP391';

    PRINT N'[Fix] Đã cập nhật lại dữ liệu Courses.SWP391 với encoding đúng.';
END
GO

/* ============================================================================
   BƯỚC 4: Cập nhật lại dữ liệu bảng Users (nếu FullName bị lỗi)
   ============================================================================ */
-- Cập nhật tên giảng viên
IF EXISTS (SELECT 1 FROM dbo.Users WHERE Id = 'GV123456')
BEGIN
    UPDATE dbo.Users
    SET FullName = N'Nguyễn Văn Học'
    WHERE Id = 'GV123456';
    PRINT N'[Fix] Đã cập nhật FullName cho GV123456.';
END

-- Cập nhật tên sinh viên
IF EXISTS (SELECT 1 FROM dbo.Users WHERE Id = 'HE187159')
BEGIN
    UPDATE dbo.Users
    SET FullName = N'Trần Văn Sinh'  -- Thay bằng tên thật nếu cần
    WHERE Id = 'HE187159';
    PRINT N'[Fix] Đã cập nhật FullName cho HE187159.';
END
GO

/* ============================================================================
   BƯỚC 5: Cập nhật lại tên lớp học bị lỗi
   ============================================================================ */
IF EXISTS (SELECT 1 FROM dbo.Classes WHERE Id = 'SWP391-CLC01')
BEGIN
    UPDATE dbo.Classes
    SET Name = N'Lớp Công nghệ phần mềm - CLC'
    WHERE Id = 'SWP391-CLC01';
    PRINT N'[Fix] Đã cập nhật tên lớp SWP391-CLC01.';
END
GO

/* ============================================================================
   BƯỚC 6: Kiểm tra kết quả sau khi sửa
   ============================================================================ */
PRINT N'';
PRINT N'========== KIỂM TRA KẾT QUẢ ==========';

SELECT 
    'Courses' AS [Bảng],
    Code,
    Name,
    Description
FROM dbo.Courses
WHERE Code = 'SWP391';

SELECT 
    'Users' AS [Bảng],
    Id,
    FullName,
    Email,
    Role
FROM dbo.Users;

SELECT 
    'Classes' AS [Bảng],
    Id,
    Name
FROM dbo.Classes;

PRINT N'Hoàn tất sửa encoding. Chữ tiếng Việt đã được khôi phục đúng.';
GO
