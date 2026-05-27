-- Chạy script này trên database FlippedClassroom trước khi dùng tính năng lịch/buổi học
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ClassSessions')
BEGIN
    CREATE TABLE dbo.ClassSessions (
        Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ClassSessions_Id DEFAULT (NEWSEQUENTIALID()),
        ClassId     VARCHAR(20)      NOT NULL,
        SessionDate DATE             NOT NULL,
        StartTime   TIME(0)          NULL,
        EndTime     TIME(0)          NULL,
        Title       NVARCHAR(255)    NOT NULL,
        Detail      NVARCHAR(MAX)    NULL,
        Room        NVARCHAR(100)    NULL,
        CreatedAt   DATETIME2(0)     NOT NULL CONSTRAINT DF_ClassSessions_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt   DATETIME2(0)     NOT NULL CONSTRAINT DF_ClassSessions_UpdatedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT PK_ClassSessions PRIMARY KEY (Id),
        CONSTRAINT FK_ClassSessions_Class FOREIGN KEY (ClassId) REFERENCES dbo.Classes(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_ClassSessions_Class_Date ON dbo.ClassSessions (ClassId, SessionDate);
END
GO
