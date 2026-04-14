// IQuizService — Interface hiding business logic for quizzes.

using QuizService.Application.DTOs;
using QuizService.Domain.Entities;

namespace QuizService.Application.Interfaces;

public interface IQuizService
{
    Task<Guid> CreateQuizAsync(CreateQuizDto dto);
    Task AddQuestionAsync(Guid quizId, CreateQuestionDto dto);
    Task<Quiz> GetQuizDetailsAsync(Guid quizId);
    
    // Start an attempt for a given quiz & student
    Task<Guid> StartQuizAsync(Guid quizId, Guid studentId);
    
    // Submit final answers, calculates score, and saves records
    Task<QuizResultDto> SubmitQuizAsync(Guid attemptId, SubmitQuizDto dto, Guid studentId);
}