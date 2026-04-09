using QuizService.Application.DTOs;

namespace QuizService.Application.Interfaces;

public interface IQuizService
{
    Task<Guid> CreateQuizAsync(CreateQuizDto dto);
    Task AddQuestionAsync(CreateQuestionDto dto);
    Task<QuizResultDto> SubmitQuizAsync(SubmitQuizDto dto);
}