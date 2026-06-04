-- ============================================================
-- Migration Script: Add IsDisabled + Fix ClassRole NULL values
-- Run this against the FlippedClassroom database
-- ============================================================

-- 1. Add IsDisabled column to LearningMaterials table
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('LearningMaterials') 
    AND name = 'IsDisabled'
)
BEGIN
    ALTER TABLE LearningMaterials
    ADD IsDisabled BIT NOT NULL CONSTRAINT DF_LearningMaterials_IsDisabled DEFAULT 0;

    PRINT 'Column IsDisabled added to LearningMaterials successfully.';
END
ELSE
BEGIN
    PRINT 'Column IsDisabled already exists in LearningMaterials. Skipping.';
END

-- 2. Add ClassRole column to ClassStudents table (if not exists)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('ClassStudents') 
    AND name = 'ClassRole'
)
BEGIN
    ALTER TABLE ClassStudents
    ADD ClassRole NVARCHAR(20) NOT NULL CONSTRAINT DF_ClassStudents_ClassRole DEFAULT 'student';

    PRINT 'Column ClassRole added to ClassStudents successfully.';
END
ELSE
BEGIN
    PRINT 'Column ClassRole already exists in ClassStudents. Skipping ADD.';
END

-- 3. Fix any existing NULL values in ClassRole (handles rows inserted before default was set)
UPDATE ClassStudents
SET ClassRole = 'student'
WHERE ClassRole IS NULL;

PRINT 'NULL ClassRole values updated to student.';
