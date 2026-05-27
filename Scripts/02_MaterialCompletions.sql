USE [FlippedClassroom];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MaterialCompletions')
BEGIN
    CREATE TABLE dbo.MaterialCompletions (
        MaterialId  UNIQUEIDENTIFIER NOT NULL,
        StudentId   VARCHAR(20)      NOT NULL,
        CompletedAt DATETIME2(0)     NOT NULL CONSTRAINT DF_MaterialCompletions_CompletedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT PK_MaterialCompletions PRIMARY KEY (MaterialId, StudentId),
        CONSTRAINT FK_MaterialCompletions_Material FOREIGN KEY (MaterialId) REFERENCES dbo.LearningMaterials(Id) ON DELETE CASCADE,
        CONSTRAINT FK_MaterialCompletions_Student FOREIGN KEY (StudentId) REFERENCES dbo.Users(Id)
    );
    PRINT N'Da tao bang MaterialCompletions.';
END
GO

-- Phan hoi mau cho lop SWP391-CLC01
IF NOT EXISTS (SELECT 1 FROM dbo.SupportFeedbacks WHERE Title LIKE N'%video tuần 3%')
BEGIN
    INSERT INTO dbo.SupportFeedbacks (ClassId, SenderId, Title, Message, Status, CreatedAt)
    VALUES ('SWP391-CLC01', 'HE187159', N'Khong truy cap duoc video tuan 3',
            N'Em bam link video bao loi 403, nhờ thay/co kiem tra giup a.', 'OPEN', SYSDATETIME());
END
GO
