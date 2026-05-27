/*
================================================================================
  FlippedClassroom - Script SQL cho Giảng viên (Lecturer)
  Chạy trên SQL Server Management Studio (SSMS)
  Database: FlippedClassroom

  Thứ tự: mở file này -> F5 (Execute)

  Tài khoản có sẵn (theo bảng Users):
    - Giảng viên: GV123456  (giangvien@system.com)
    - Sinh viên:  HE187159  (sinhvien@system.com)
    - Admin:      AD000001
================================================================================
*/

USE [FlippedClassroom];
GO

SET NOCOUNT ON;

/* ============================================================================
   PHẦN 1: BẢNG ClassSessions (Lịch buổi học + chi tiết flipped classroom)
   ============================================================================ */
IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = N'dbo' AND t.name = N'ClassSessions'
)
BEGIN
    PRINT N'[1] Đang tạo bảng dbo.ClassSessions...';

    CREATE TABLE dbo.ClassSessions (
        Id          UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT DF_ClassSessions_Id DEFAULT (NEWSEQUENTIALID()),
        ClassId     VARCHAR(20)      NOT NULL,
        SessionDate DATE             NOT NULL,
        StartTime   TIME(0)          NULL,
        EndTime     TIME(0)          NULL,
        Title       NVARCHAR(255)    NOT NULL,
        Detail      NVARCHAR(MAX)    NULL,   -- Nội dung chi tiết buổi học (video trước lớp, tài liệu...)
        Room        NVARCHAR(100)    NULL,
        CreatedAt   DATETIME2(0)     NOT NULL
            CONSTRAINT DF_ClassSessions_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt   DATETIME2(0)     NOT NULL
            CONSTRAINT DF_ClassSessions_UpdatedAt DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_ClassSessions PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_ClassSessions_Class
            FOREIGN KEY (ClassId) REFERENCES dbo.Classes(Id) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_ClassSessions_Class_Date
        ON dbo.ClassSessions (ClassId, SessionDate);

    PRINT N'[1] Đã tạo bảng ClassSessions.';
END
ELSE
    PRINT N'[1] Bảng ClassSessions đã tồn tại — bỏ qua.';
GO

/* ============================================================================
   PHẦN 2: DỮ LIỆU MẪU — Học kỳ, Môn học, Lớp gán cho GV123456
   (Chỉ INSERT nếu chưa có — an toàn chạy lại nhiều lần)
   ============================================================================ */

DECLARE @TermId     UNIQUEIDENTIFIER;
DECLARE @CourseId   UNIQUEIDENTIFIER;
DECLARE @ClassId    VARCHAR(20) = 'SWP391-CLC01';
DECLARE @LecturerId VARCHAR(20) = 'GV123456';
DECLARE @StudentId  VARCHAR(20) = 'HE187159';

-- 2.1 Học kỳ
IF NOT EXISTS (SELECT 1 FROM dbo.AcademicTerms WHERE Name = N'Summer 2026')
BEGIN
    SET @TermId = NEWID();
    INSERT INTO dbo.AcademicTerms (Id, Name, StartDate, EndDate, CreatedAt)
    VALUES (@TermId, N'Summer 2026', '2026-05-01', '2026-08-31', SYSDATETIME());
    PRINT N'[2.1] Đã thêm học kỳ Summer 2026.';
END
ELSE
BEGIN
    SELECT @TermId = Id FROM dbo.AcademicTerms WHERE Name = N'Summer 2026';
    PRINT N'[2.1] Học kỳ Summer 2026 đã có.';
END

-- 2.2 Môn học
IF NOT EXISTS (SELECT 1 FROM dbo.Courses WHERE Code = 'SWP391')
BEGIN
    SET @CourseId = NEWID();
    INSERT INTO dbo.Courses (Id, Name, Code, Description, CreatedAt)
    VALUES (
        @CourseId,
        N'Software Project',
        'SWP391',
        N'Môn đồ án phần mềm — mô hình Flipped Classroom',
        SYSDATETIME()
    );
    PRINT N'[2.2] Đã thêm môn SWP391.';
END
ELSE
BEGIN
    SELECT @CourseId = Id FROM dbo.Courses WHERE Code = 'SWP391';
    PRINT N'[2.2] Môn SWP391 đã có.';
END

-- 2.3 Lớp học phần (gán giảng viên Nguyễn Văn Học)
IF NOT EXISTS (SELECT 1 FROM dbo.Classes WHERE Id = @ClassId)
BEGIN
    INSERT INTO dbo.Classes (Id, CourseId, AcademicTermId, LecturerId, Name, AllowReviewAfterEnd, CreatedAt)
    VALUES (
        @ClassId,
        @CourseId,
        @TermId,
        @LecturerId,
        N'Lớp Công nghệ phần mềm - CLC',
        1,
        SYSDATETIME()
    );
    PRINT N'[2.3] Đã thêm lớp SWP391-CLC01 cho giảng viên GV123456.';
END
ELSE
BEGIN
    -- Cập nhật lecturer nếu lớp đã tồn tại nhưng chưa gán đúng GV
    UPDATE dbo.Classes
    SET LecturerId = @LecturerId,
        CourseId = @CourseId,
        AcademicTermId = @TermId,
        Name = N'Lớp Công nghệ phần mềm - CLC'
    WHERE Id = @ClassId;
    PRINT N'[2.3] Lớp SWP391-CLC01 đã có — đã đồng bộ LecturerId = GV123456.';
END

-- 2.4 Ghi danh sinh viên vào lớp
IF NOT EXISTS (
    SELECT 1 FROM dbo.ClassStudents
    WHERE ClassId = @ClassId AND StudentId = @StudentId
)
BEGIN
    INSERT INTO dbo.ClassStudents (ClassId, StudentId, EnrolledAt)
    VALUES (@ClassId, @StudentId, SYSDATETIME());
    PRINT N'[2.4] Đã ghi danh sinh viên HE187159 vào lớp.';
END
ELSE
    PRINT N'[2.4] Sinh viên đã có trong lớp.';
GO

/* ============================================================================
   PHẦN 3: Buổi học mẫu (ClassSessions)
   ============================================================================ */
DECLARE @ClassId2 VARCHAR(20) = 'SWP391-CLC01';

IF NOT EXISTS (
    SELECT 1 FROM dbo.ClassSessions
    WHERE ClassId = @ClassId2 AND Title = N'Buổi 1 - Giới thiệu Flipped Classroom'
)
BEGIN
    INSERT INTO dbo.ClassSessions (ClassId, SessionDate, StartTime, EndTime, Title, Detail, Room)
    VALUES
    (
        @ClassId2,
        '2026-05-26',
        '07:30',
        '09:30',
        N'Buổi 1 - Giới thiệu Flipped Classroom',
        N'Video trước lớp: Agile/Scrum. Trên lớp: thảo luận nhóm quy trình đồ án.',
        N'P301'
    ),
    (
        @ClassId2,
        '2026-06-02',
        '07:30',
        '09:30',
        N'Buổi 2 - Phân tích yêu cầu & Use Case',
        N'Xem slide PDF trước buổi. Thực hành vẽ Use Case Diagram trên lớp.',
        N'P301'
    ),
    (
        @ClassId2,
        '2026-06-09',
        '07:30',
        '09:30',
        N'Buổi 3 - Thiết kế hệ thống',
        N'Chuẩn bị: đọc chương thiết kế MVC. Buổi lab: phân tích kiến trúc API.',
        N'P302'
    );

    PRINT N'[3] Đã thêm 3 buổi học mẫu.';
END
ELSE
    PRINT N'[3] Buổi học mẫu đã có — bỏ qua.';
GO

/* ============================================================================
   PHẦN 4 (TÙY CHỌN): Bảng hỗ trợ Dashboard đầy đủ (học liệu, bài tập...)
   Chạy khi team triển khai API — hiện frontend đang dùng localStorage demo
   ============================================================================ */

-- 4.1 Học liệu
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LearningMaterials')
BEGIN
    CREATE TABLE dbo.LearningMaterials (
        Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        ClassId     VARCHAR(20)      NOT NULL,
        Title       NVARCHAR(255)    NOT NULL,
        Description NVARCHAR(MAX)    NULL,
        MaterialType VARCHAR(20)     NOT NULL,  -- video | pdf | document | quiz
        FileUrl     NVARCHAR(500)    NULL,
        FileSize    NVARCHAR(50)     NULL,
        UploadedAt  DATE             NOT NULL DEFAULT CAST(SYSDATETIME() AS DATE),
        CreatedAt   DATETIME2(0)     NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT PK_LearningMaterials PRIMARY KEY (Id),
        CONSTRAINT FK_LearningMaterials_Class
            FOREIGN KEY (ClassId) REFERENCES dbo.Classes(Id) ON DELETE CASCADE,
        CONSTRAINT CK_LearningMaterials_Type
            CHECK (MaterialType IN ('video', 'pdf', 'document', 'quiz'))
    );
    PRINT N'[4.1] Đã tạo bảng LearningMaterials.';
END

-- 4.2 Bài tập
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Assignments')
BEGIN
    CREATE TABLE dbo.Assignments (
        Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        ClassId     VARCHAR(20)      NOT NULL,
        Title       NVARCHAR(255)    NOT NULL,
        Description NVARCHAR(MAX)    NULL,
        DueDate     DATE             NOT NULL,
        MaxPoints   DECIMAL(5,2)     NOT NULL DEFAULT 10,
        CreatedAt   DATETIME2(0)     NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT PK_Assignments PRIMARY KEY (Id),
        CONSTRAINT FK_Assignments_Class
            FOREIGN KEY (ClassId) REFERENCES dbo.Classes(Id) ON DELETE CASCADE
    );
    PRINT N'[4.2] Đã tạo bảng Assignments.';
END

-- 4.3 Bài nộp
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Submissions')
BEGIN
    CREATE TABLE dbo.Submissions (
        Id           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        AssignmentId UNIQUEIDENTIFIER NOT NULL,
        StudentId    VARCHAR(20)      NOT NULL,
        FileName     NVARCHAR(255)    NULL,
        StudentNotes NVARCHAR(MAX)    NULL,
        Status       VARCHAR(20)      NOT NULL DEFAULT 'SUBMITTED', -- SUBMITTED | GRADED
        Grade        DECIMAL(5,2)     NULL,
        Feedback     NVARCHAR(MAX)    NULL,
        SubmittedAt  DATETIME2(0)     NOT NULL DEFAULT SYSDATETIME(),
        GradedAt     DATETIME2(0)     NULL,
        CONSTRAINT PK_Submissions PRIMARY KEY (Id),
        CONSTRAINT FK_Submissions_Assignment
            FOREIGN KEY (AssignmentId) REFERENCES dbo.Assignments(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Submissions_Student
            FOREIGN KEY (StudentId) REFERENCES dbo.Users(Id),
        CONSTRAINT CK_Submissions_Status
            CHECK (Status IN ('SUBMITTED', 'GRADED'))
    );
    PRINT N'[4.3] Đã tạo bảng Submissions.';
END

-- 4.4 Phản hồi hỗ trợ
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SupportFeedbacks')
BEGIN
    CREATE TABLE dbo.SupportFeedbacks (
        Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        ClassId     VARCHAR(20)      NULL,
        SenderId    VARCHAR(20)      NOT NULL,
        Title       NVARCHAR(255)    NOT NULL,
        Message     NVARCHAR(MAX)    NOT NULL,
        Status      VARCHAR(20)      NOT NULL DEFAULT 'OPEN',  -- OPEN | RESPONDED
        Response    NVARCHAR(MAX)    NULL,
        CreatedAt   DATETIME2(0)     NOT NULL DEFAULT SYSDATETIME(),
        RespondedAt DATETIME2(0)     NULL,
        CONSTRAINT PK_SupportFeedbacks PRIMARY KEY (Id),
        CONSTRAINT FK_SupportFeedbacks_Sender
            FOREIGN KEY (SenderId) REFERENCES dbo.Users(Id),
        CONSTRAINT CK_SupportFeedbacks_Status
            CHECK (Status IN ('OPEN', 'RESPONDED'))
    );
    PRINT N'[4.4] Đã tạo bảng SupportFeedbacks.';
END

-- 4.5 Blog thảo luận
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DiscussionThreads')
BEGIN
    CREATE TABLE dbo.DiscussionThreads (
        Id        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        ClassId   VARCHAR(20)      NULL,
        AuthorId  VARCHAR(20)      NOT NULL,
        Title     NVARCHAR(255)    NOT NULL,
        Content   NVARCHAR(MAX)    NOT NULL,
        CreatedAt DATETIME2(0)     NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT PK_DiscussionThreads PRIMARY KEY (Id),
        CONSTRAINT FK_DiscussionThreads_Author
            FOREIGN KEY (AuthorId) REFERENCES dbo.Users(Id)
    );

    CREATE TABLE dbo.DiscussionReplies (
        Id        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        ThreadId  UNIQUEIDENTIFIER NOT NULL,
        AuthorId  VARCHAR(20)      NOT NULL,
        Content   NVARCHAR(MAX)    NOT NULL,
        CreatedAt DATETIME2(0)     NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT PK_DiscussionReplies PRIMARY KEY (Id),
        CONSTRAINT FK_DiscussionReplies_Thread
            FOREIGN KEY (ThreadId) REFERENCES dbo.DiscussionThreads(Id) ON DELETE CASCADE,
        CONSTRAINT FK_DiscussionReplies_Author
            FOREIGN KEY (AuthorId) REFERENCES dbo.Users(Id)
    );
    PRINT N'[4.5] Đã tạo bảng DiscussionThreads & DiscussionReplies.';
END
GO

/* ============================================================================
   PHẦN 5: KIỂM TRA SAU KHI CHẠY
   ============================================================================ */
PRINT N'';
PRINT N'========== KẾT QUẢ KIỂM TRA ==========';

SELECT N'Users (giảng viên)' AS [Check], Id, FullName, Email, Role
FROM dbo.Users WHERE Role = 'lecturer';

SELECT N'Lớp của GV123456' AS [Check], c.Id, c.Name, co.Code AS CourseCode, u.FullName AS Lecturer
FROM dbo.Classes c
INNER JOIN dbo.Courses co ON co.Id = c.CourseId
INNER JOIN dbo.Users u ON u.Id = c.LecturerId
WHERE c.LecturerId = 'GV123456';

SELECT N'Sinh viên trong lớp' AS [Check], cs.ClassId, cs.StudentId, u.FullName
FROM dbo.ClassStudents cs
INNER JOIN dbo.Users u ON u.Id = cs.StudentId
WHERE cs.ClassId = 'SWP391-CLC01';

SELECT N'Buổi học (ClassSessions)' AS [Check], ClassId, SessionDate, Title, Room
FROM dbo.ClassSessions
WHERE ClassId = 'SWP391-CLC01'
ORDER BY SessionDate;

PRINT N'Hoàn tất script.';
GO
