using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SWP.BLL.DTOs.Quizzes;

namespace SWP.BLL.Interfaces;

public interface IQuizService
{
    // Lecturer operations
    Task<QuizDetailDto> CreateQuizAsync(string lecturerId, CreateQuizDto dto);
    Task<QuizDetailDto> UpdateQuizAsync(string lecturerId, Guid quizId, UpdateQuizDto dto);
    Task DeleteQuizAsync(string lecturerId, Guid quizId);
    Task<List<QuizResponseDto>> GetQuizzesByClassAsync(string classId);
    Task<QuizDetailDto> GetQuizDetailsForLecturerAsync(string lecturerId, Guid quizId);
    Task<List<QuizAttemptDto>> GetClassAttemptsAsync(string lecturerId, Guid quizId);

    // Student operations
    Task<QuizDetailDto> GetQuizDetailsForStudentAsync(string studentId, Guid quizId);
    Task<QuizAttemptDto> StartAttemptAsync(string studentId, Guid quizId);
    Task<QuizResultDto> SubmitAttemptAsync(string studentId, Guid attemptId, SubmitQuizDto dto);
    Task<List<QuizAttemptDto>> GetMyAttemptsAsync(string studentId, Guid quizId);
    Task<AttemptDetailDto> GetAttemptDetailAsync(string studentId, Guid attemptId);
}
