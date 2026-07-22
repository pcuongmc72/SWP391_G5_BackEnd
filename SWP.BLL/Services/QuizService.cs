using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.Quizzes;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;

namespace SWP.BLL.Services;

public class QuizService : IQuizService
{
    private readonly FlippedClassroomContext _context;

    public QuizService(FlippedClassroomContext context)
    {
        _context = context;
    }



    public async Task<QuizDetailDto> CreateQuizAsync(string lecturerId, CreateQuizDto dto)
    {
        await EnsureClassAccessAsync(lecturerId, dto.ClassId);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var quiz = new Quiz
            {
                Id = Guid.NewGuid(),
                ClassId = dto.ClassId,
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                TimeLimit = dto.TimeLimit,
                MaxAttempts = dto.MaxAttempts,
                CreatedBy = lecturerId,
                CreatedAt = DateTime.UtcNow,
                IsDisabled = false
            };

            _context.Quizzes.Add(quiz);

            int totalQuestions = dto.Questions.Count;
            decimal pointsPerQuestion = totalQuestions > 0 ? Math.Round(10m / totalQuestions, 2) : 0;
            int order = 1;
            foreach (var qDto in dto.Questions)
            {
                var question = new QuizQuestion
                {
                    Id = Guid.NewGuid(),
                    QuizId = quiz.Id,
                    QuestionText = qDto.QuestionText.Trim(),
                    Points = pointsPerQuestion,
                    Order = order++
                };

                _context.QuizQuestions.Add(question);

                foreach (var oDto in qDto.Options)
                {
                    var option = new QuizOption
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = question.Id,
                        OptionText = oDto.OptionText.Trim(),
                        IsCorrect = oDto.IsCorrect
                    };
                    _context.QuizOptions.Add(option);
                }
            }

            // Đồng bộ tạo một LearningMaterial kiểu 'quiz'
            // Mô tả của học liệu sẽ lưu thông tin JSON mô phỏng cấu hình học liệu
            var materialDesc = System.Text.Json.JsonSerializer.Serialize(new
            {
                desc = dto.Description?.Trim() ?? "",
                publishDate = dto.PublishDate ?? DateTime.UtcNow.ToString("yyyy-MM-dd"),
                deadline = dto.Deadline ?? "",
                distributeMode = "all",
                groups = new List<string>(),
                comments = new List<string>()
            });

            var material = new LearningMaterial
            {
                Id = Guid.NewGuid(),
                ClassId = dto.ClassId,
                Title = dto.Title.Trim(),
                Description = materialDesc,
                Chapter = dto.Chapter.Trim(),
                Lesson = dto.Lesson?.Trim(),
                MaterialType = "quiz",
                FileUrl = quiz.Id.ToString(), // ID của Quiz làm FileUrl
                FileSize = "Quiz",
                UploadedAt = DateOnly.FromDateTime(DateTime.Today),
                CreatedAt = DateTime.UtcNow,
                IsDisabled = false
            };

            _context.LearningMaterials.Add(material);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetQuizDetailsForLecturerAsync(lecturerId, quiz.Id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<QuizDetailDto> UpdateQuizAsync(string lecturerId, Guid quizId, UpdateQuizDto dto)
    {
        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null)
            throw new KeyNotFoundException("Không tìm thấy bài trắc nghiệm.");

        await EnsureClassAccessAsync(lecturerId, quiz.ClassId);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            quiz.Title = dto.Title.Trim();
            quiz.Description = dto.Description?.Trim();
            quiz.TimeLimit = dto.TimeLimit;
            quiz.MaxAttempts = dto.MaxAttempts;

            // Load existing questions and their options
            var existingQuestions = await _context.QuizQuestions
                .Include(q => q.QuizOptions)
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.Order)
                .ToListAsync();

            int totalQuestions = dto.Questions.Count;
            decimal pointsPerQuestion = totalQuestions > 0 ? Math.Round(10m / totalQuestions, 2) : 0;

            for (int i = 0; i < totalQuestions; i++)
            {
                var qDto = dto.Questions[i];

                if (i < existingQuestions.Count)
                {
                    // Update existing question
                    var question = existingQuestions[i];
                    question.QuestionText = qDto.QuestionText.Trim();
                    question.Points = pointsPerQuestion;
                    question.Order = i + 1;

                    // Update or add options
                    var existingOptions = question.QuizOptions.ToList();
                    for (int j = 0; j < qDto.Options.Count; j++)
                    {
                        var oDto = qDto.Options[j];

                        if (j < existingOptions.Count)
                        {
                            var option = existingOptions[j];
                            option.OptionText = oDto.OptionText.Trim();
                            option.IsCorrect = oDto.IsCorrect;
                        }
                        else
                        {
                            var option = new QuizOption
                            {
                                Id = Guid.NewGuid(),
                                QuestionId = question.Id,
                                OptionText = oDto.OptionText.Trim(),
                                IsCorrect = oDto.IsCorrect
                            };
                            _context.QuizOptions.Add(option);
                        }
                    }

                    // Remove extra options
                    if (existingOptions.Count > qDto.Options.Count)
                    {
                        var extraOptions = existingOptions.Skip(qDto.Options.Count).ToList();
                        var extraOptionIds = extraOptions.Select(o => o.Id).ToList();

                        // Manually delete answers to these options to prevent FK constraint failure
                        var answersToOptions = await _context.QuizAnswers
                            .Where(ans => extraOptionIds.Contains(ans.SelectedOptionId))
                            .ToListAsync();
                        _context.QuizAnswers.RemoveRange(answersToOptions);

                        _context.QuizOptions.RemoveRange(extraOptions);
                    }
                }
                else
                {
                    // Add new question
                    var question = new QuizQuestion
                    {
                        Id = Guid.NewGuid(),
                        QuizId = quiz.Id,
                        QuestionText = qDto.QuestionText.Trim(),
                        Points = pointsPerQuestion,
                        Order = i + 1
                    };
                    _context.QuizQuestions.Add(question);

                    foreach (var oDto in qDto.Options)
                    {
                        var option = new QuizOption
                        {
                            Id = Guid.NewGuid(),
                            QuestionId = question.Id,
                            OptionText = oDto.OptionText.Trim(),
                            IsCorrect = oDto.IsCorrect
                        };
                        _context.QuizOptions.Add(option);
                    }
                }
            }

            // Remove extra questions
            if (existingQuestions.Count > totalQuestions)
            {
                var extraQuestions = existingQuestions.Skip(totalQuestions).ToList();
                var extraQuestionIds = extraQuestions.Select(q => q.Id).ToList();

                // Manually delete answers to these questions to prevent FK constraint failure
                var answersToQuestions = await _context.QuizAnswers
                    .Where(ans => extraQuestionIds.Contains(ans.QuestionId))
                    .ToListAsync();
                _context.QuizAnswers.RemoveRange(answersToQuestions);

                _context.QuizQuestions.RemoveRange(extraQuestions);
            }

            // Save the quiz changes first to ensure options are persisted/updated before recalculating
            await _context.SaveChangesAsync();

            // Recalculate scores for all student attempts of this quiz
            var attempts = await _context.QuizAttempts
                .Include(a => a.QuizAnswers)
                .Where(a => a.QuizId == quizId)
                .ToListAsync();

            var updatedQuestions = await _context.QuizQuestions
                .Include(q => q.QuizOptions)
                .Where(q => q.QuizId == quizId)
                .ToListAsync();

            foreach (var attempt in attempts)
            {
                int correctCount = 0;
                foreach (var question in updatedQuestions)
                {
                    var correctOptionIds = question.QuizOptions
                        .Where(o => o.IsCorrect)
                        .Select(o => o.Id)
                        .ToHashSet();

                    var selectedOptionIds = attempt.QuizAnswers
                        .Where(ans => ans.QuestionId == question.Id)
                        .Select(ans => ans.SelectedOptionId)
                        .ToHashSet();

                    if (selectedOptionIds.Count > 0 && selectedOptionIds.SetEquals(correctOptionIds))
                    {
                        correctCount++;
                    }
                }

                // Update the score dynamically
                attempt.TotalScore = updatedQuestions.Count > 0
                    ? Math.Round((decimal)correctCount * 10m / updatedQuestions.Count, 2)
                    : 0;
            }

            // Đồng bộ cập nhật LearningMaterial
            var material = await _context.LearningMaterials
                .FirstOrDefaultAsync(m => m.ClassId == quiz.ClassId && m.MaterialType == "quiz" && m.FileUrl == quizId.ToString());

            if (material != null)
            {
                material.Title = quiz.Title;

                var existingMeta = new { desc = "", publishDate = "", deadline = "", distributeMode = "all", groups = new List<string>(), comments = new List<string>() };
                try
                {
                    if (!string.IsNullOrEmpty(material.Description))
                    {
                        var parsed = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(material.Description);
                        existingMeta = new
                        {
                            desc = parsed.TryGetProperty("desc", out var d) ? d.GetString() ?? "" : "",
                            publishDate = parsed.TryGetProperty("publishDate", out var pd) ? pd.GetString() ?? "" : "",
                            deadline = parsed.TryGetProperty("deadline", out var dl) ? dl.GetString() ?? "" : "",
                            distributeMode = parsed.TryGetProperty("distributeMode", out var dm) ? dm.GetString() ?? "all" : "all",
                            groups = parsed.TryGetProperty("groups", out var g) ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(g.GetRawText()) ?? new List<string>() : new List<string>(),
                            comments = parsed.TryGetProperty("comments", out var c) ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(c.GetRawText()) ?? new List<string>() : new List<string>()
                        };
                    }
                }
                catch {}

                var parsedDesc = new { 
                    desc = quiz.Description ?? "", 
                    publishDate = dto.PublishDate ?? existingMeta.publishDate, 
                    deadline = dto.Deadline ?? "", 
                    distributeMode = existingMeta.distributeMode, 
                    groups = existingMeta.groups, 
                    comments = existingMeta.comments 
                };
                material.Description = System.Text.Json.JsonSerializer.Serialize(parsedDesc);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetQuizDetailsForLecturerAsync(lecturerId, quiz.Id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteQuizAsync(string lecturerId, Guid quizId)
    {
        var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId);
        if (quiz == null) throw new KeyNotFoundException("Không tìm thấy bài trắc nghiệm.");

        await EnsureClassAccessAsync(lecturerId, quiz.ClassId);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Soft delete/ẩn học liệu tương ứng
            var material = await _context.LearningMaterials
                .FirstOrDefaultAsync(m => m.ClassId == quiz.ClassId && m.MaterialType == "quiz" && m.FileUrl == quizId.ToString());

            if (material != null)
            {
                material.IsDisabled = true;
            }

            quiz.IsDisabled = true;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<QuizResponseDto>> GetQuizzesByClassAsync(string classId)
    {
        return await _context.Quizzes
            .AsNoTracking()
            .Where(q => q.ClassId == classId && !q.IsDisabled)
            .Select(q => MapToResponseDto(q))
            .ToListAsync();
    }

    public async Task<QuizDetailDto> GetQuizDetailsForLecturerAsync(string lecturerId, Guid quizId)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.QuizOptions)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null) throw new KeyNotFoundException("Không tìm thấy bài trắc nghiệm.");

        await EnsureClassAccessAsync(lecturerId, quiz.ClassId);

        var dto = MapToDetailDto(quiz, isLecturer: true);
        return dto;
    }

    public async Task<List<QuizAttemptDto>> GetClassAttemptsAsync(string lecturerId, Guid quizId)
    {
        var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId);
        if (quiz == null) throw new KeyNotFoundException("Không tìm thấy bài trắc nghiệm.");

        await EnsureClassAccessAsync(lecturerId, quiz.ClassId);

        return await _context.QuizAttempts
            .AsNoTracking()
            .Include(a => a.Student)
            .Where(a => a.QuizId == quizId)
            .OrderByDescending(a => a.SubmittedAt)
            .Select(a => MapToAttemptDto(a))
            .ToListAsync();
    }


    public async Task<QuizDetailDto> GetQuizDetailsForStudentAsync(string studentId, Guid quizId)
    {
        // Kiểm tra xem sinh viên có thuộc lớp chứa quiz này hay không
        var quiz = await _context.Quizzes
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.QuizOptions)
            .FirstOrDefaultAsync(q => q.Id == quizId && !q.IsDisabled);

        if (quiz == null) throw new KeyNotFoundException("Không tìm thấy bài trắc nghiệm.");

        var isEnrolled = await _context.ClassStudents
            .AnyAsync(cs => cs.ClassId == quiz.ClassId && cs.StudentId == studentId);

        if (!isEnrolled)
            throw new UnauthorizedAccessException("Bạn không thuộc lớp học chứa bài trắc nghiệm này.");

        // Sinh viên xem đề sẽ ẩn đáp án đúng
        return MapToDetailDto(quiz, isLecturer: false);
    }

    public async Task<QuizAttemptDto> StartAttemptAsync(string studentId, Guid quizId)
    {
        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.Id == quizId && !q.IsDisabled);

        if (quiz == null) throw new KeyNotFoundException("Không tìm thấy bài trắc nghiệm.");

        // Kiểm tra xem sinh viên có thuộc lớp chứa quiz này hay không
        var isEnrolled = await _context.ClassStudents
            .AnyAsync(cs => cs.ClassId == quiz.ClassId && cs.StudentId == studentId);

        if (!isEnrolled)
            throw new UnauthorizedAccessException("Bạn không thuộc lớp học chứa bài trắc nghiệm này.");

        // Đếm số lượt đã làm
        var attemptsCount = await _context.QuizAttempts
            .CountAsync(a => a.QuizId == quizId && a.StudentId == studentId);

        if (attemptsCount >= quiz.MaxAttempts)
        {
            throw new InvalidOperationException($"Bạn đã đạt tới số lần làm bài tối đa ({quiz.MaxAttempts}) cho bài trắc nghiệm này.");
        }

        var attempt = new QuizAttempt
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            StudentId = studentId,
            AttemptNumber = attemptsCount + 1,
            StartedAt = DateTime.UtcNow
        };

        _context.QuizAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        await _context.Entry(attempt).Reference(a => a.Student).LoadAsync();
        return MapToAttemptDto(attempt);
    }

    public async Task<QuizResultDto> SubmitAttemptAsync(string studentId, Guid attemptId, SubmitQuizDto dto)
    {
        var attempt = await _context.QuizAttempts
            .Include(a => a.Quiz)
                .ThenInclude(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuizOptions)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.StudentId == studentId);

        if (attempt == null)
            throw new KeyNotFoundException("Lượt làm bài không tồn tại.");

        if (attempt.SubmittedAt != null)
            throw new InvalidOperationException("Lượt làm bài này đã được nộp.");

        // Validate thời gian hết hạn (chống gian lận bypass timer)
        if (attempt.Quiz.TimeLimit.HasValue)
        {
            var deadline = attempt.StartedAt
                .AddMinutes(attempt.Quiz.TimeLimit.Value)
                .AddSeconds(30); // Buffer 30s cho độ trễ mạng

            if (DateTime.UtcNow > deadline)
            {
                throw new InvalidOperationException(
                    "Đã hết thời gian làm bài. Bài thi không thể nộp sau khi quá giờ.");
            }
        }

        // Tính điểm tự động
        int correctQuestionsCount = 0;
        int totalQuestions = attempt.Quiz.QuizQuestions.Count;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Group the submitted answers by QuestionId
            var submittedAnswersGrouped = dto.Answers
                .GroupBy(ans => ans.QuestionId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.SelectedOptionId).ToHashSet());

            foreach (var qGroup in submittedAnswersGrouped)
            {
                var questionId = qGroup.Key;
                var selectedOptionIds = qGroup.Value;

                var question = attempt.Quiz.QuizQuestions.FirstOrDefault(q => q.Id == questionId);
                if (question == null) continue;

                // Save all selected answers for this question
                foreach (var optId in selectedOptionIds)
                {
                    // Check if option actually belongs to this question
                    var optionExists = question.QuizOptions.Any(o => o.Id == optId);
                    if (!optionExists) continue;

                    var answer = new QuizAnswer
                    {
                        AttemptId = attemptId,
                        QuestionId = questionId,
                        SelectedOptionId = optId
                    };
                    _context.QuizAnswers.Add(answer);
                }

                // Check correctness: selected options must exactly match the options where IsCorrect == true
                var correctOptionIds = question.QuizOptions
                    .Where(o => o.IsCorrect)
                    .Select(o => o.Id)
                    .ToHashSet();

                if (selectedOptionIds.SetEquals(correctOptionIds))
                {
                    correctQuestionsCount++;
                }
            }

            // Calculate score based on total score of 10 points
            decimal totalScore = 0;
            if (totalQuestions > 0)
            {
                totalScore = Math.Round((decimal)correctQuestionsCount * 10m / totalQuestions, 2);
            }

            attempt.SubmittedAt = DateTime.UtcNow;
            attempt.TotalScore = totalScore;

            // Automatically check if student passed (>= 5.0 out of 10.0 points)
            bool isPassed = totalScore >= 5.0m;

            var material = await _context.LearningMaterials
                .FirstOrDefaultAsync(m => m.ClassId == attempt.Quiz.ClassId && m.MaterialType == "quiz" && m.FileUrl == attempt.QuizId.ToString());

            if (material != null && isPassed)
            {
                // Kiểm tra xem đã lưu hoàn thành học liệu chưa
                var isCompleted = await _context.MaterialCompletions
                    .AnyAsync(mc => mc.MaterialId == material.Id && mc.StudentId == studentId);

                if (!isCompleted)
                {
                    _context.MaterialCompletions.Add(new MaterialCompletion
                    {
                        MaterialId = material.Id,
                        StudentId = studentId,
                        CompletedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new QuizResultDto
            {
                AttemptId = attemptId,
                TotalScore = totalScore,
                CorrectAnswersCount = correctQuestionsCount,
                TotalQuestionsCount = totalQuestions,
                SubmittedAt = attempt.SubmittedAt.Value
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<QuizAttemptDto>> GetMyAttemptsAsync(string studentId, Guid quizId)
    {
        return await _context.QuizAttempts
            .AsNoTracking()
            .Include(a => a.Student)
            .Where(a => a.QuizId == quizId && a.StudentId == studentId && a.SubmittedAt != null)
            .OrderByDescending(a => a.SubmittedAt)
            .Select(a => MapToAttemptDto(a))
            .ToListAsync();
    }

    public async Task<AttemptDetailDto> GetAttemptDetailAsync(string studentId, Guid attemptId)
    {
        var attempt = await _context.QuizAttempts
            .AsNoTracking()
            .Include(a => a.QuizAnswers)
            .Include(a => a.Quiz)
                .ThenInclude(q => q.QuizQuestions.OrderBy(qq => qq.Order))
                    .ThenInclude(qq => qq.QuizOptions)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.StudentId == studentId);

        if (attempt == null)
            throw new KeyNotFoundException("Không tìm thấy lượt thi.");

        if (attempt.SubmittedAt == null)
            throw new InvalidOperationException("Lượt thi này chưa được nộp.");

        // Build the set of selected option IDs for O(1) lookup
        var selectedOptionIds = attempt.QuizAnswers
            .Select(ans => ans.SelectedOptionId)
            .ToHashSet();

        var questions = attempt.Quiz.QuizQuestions
            .OrderBy(q => q.Order)
            .Select(q =>
            {
                var correctOptionIds = q.QuizOptions
                    .Where(o => o.IsCorrect)
                    .Select(o => o.Id)
                    .ToHashSet();

                var studentSelectedForThisQ = attempt.QuizAnswers
                    .Where(ans => ans.QuestionId == q.Id)
                    .Select(ans => ans.SelectedOptionId)
                    .ToHashSet();

                bool isQuestionCorrect = studentSelectedForThisQ.Count > 0
                    && studentSelectedForThisQ.SetEquals(correctOptionIds);

                return new AttemptAnswerDetailDto
                {
                    QuestionId = q.Id,
                    QuestionOrder = q.Order,
                    QuestionText = q.QuestionText,
                    Points = q.Points,
                    IsCorrect = isQuestionCorrect,
                    Options = q.QuizOptions.Select(o => new AttemptOptionDetailDto
                    {
                        Id = o.Id,
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect,
                        WasSelected = studentSelectedForThisQ.Contains(o.Id)
                    }).ToList()
                };
            }).ToList();

        int correctCount = questions.Count(q => q.IsCorrect);

        return new AttemptDetailDto
        {
            AttemptId = attempt.Id,
            AttemptNumber = attempt.AttemptNumber,
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt,
            TotalScore = attempt.TotalScore,
            CorrectCount = correctCount,
            TotalCount = questions.Count,
            Questions = questions
        };
    }



    private async Task EnsureClassAccessAsync(string lecturerId, string classId)
    {
        var isLecturer = await _context.Classes
            .AnyAsync(c => c.Id == classId && c.LecturerId == lecturerId);

        if (isLecturer) return;

        var isAssistant = await _context.ClassStudents
            .AnyAsync(cs => cs.ClassId == classId && cs.StudentId == lecturerId && cs.ClassRole == "assistant");

        if (!isAssistant)
            throw new UnauthorizedAccessException("Bạn không được phân công giảng dạy lớp học này.");
    }

    private static QuizResponseDto MapToResponseDto(Quiz quiz) => new()
    {
        Id = quiz.Id,
        ClassId = quiz.ClassId,
        Title = quiz.Title,
        Description = quiz.Description,
        TimeLimit = quiz.TimeLimit,
        MaxAttempts = quiz.MaxAttempts,
        CreatedBy = quiz.CreatedBy,
        CreatedAt = quiz.CreatedAt,
        IsDisabled = quiz.IsDisabled
    };

    private static QuizDetailDto MapToDetailDto(Quiz quiz, bool isLecturer) => new()
    {
        Id = quiz.Id,
        ClassId = quiz.ClassId,
        Title = quiz.Title,
        Description = quiz.Description,
        TimeLimit = quiz.TimeLimit,
        MaxAttempts = quiz.MaxAttempts,
        CreatedBy = quiz.CreatedBy,
        CreatedAt = quiz.CreatedAt,
        IsDisabled = quiz.IsDisabled,
        Questions = quiz.QuizQuestions
            .OrderBy(q => q.Order)
            .Select(q => new QuizQuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                Points = q.Points,
                Order = q.Order,
                MaxSelections = q.QuizOptions.Count(o => o.IsCorrect),
                Options = q.QuizOptions.Select(o => new QuizOptionDto
                {
                    Id = o.Id,
                    OptionText = o.OptionText,
                    IsCorrect = isLecturer ? o.IsCorrect : null // Giấu đáp án đúng nếu là student
                }).ToList()
            }).ToList()
    };

    private static QuizAttemptDto MapToAttemptDto(QuizAttempt attempt) => new()
    {
        Id = attempt.Id,
        QuizId = attempt.QuizId,
        StudentId = attempt.StudentId,
        StudentFullName = attempt.Student?.FullName ?? "Học sinh",
        AttemptNumber = attempt.AttemptNumber,
        StartedAt = attempt.StartedAt,
        SubmittedAt = attempt.SubmittedAt,
        TotalScore = attempt.TotalScore
    };

    #endregion
}
