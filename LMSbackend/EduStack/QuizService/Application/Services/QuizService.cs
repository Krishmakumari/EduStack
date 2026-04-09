using Microsoft.EntityFrameworkCore;
using QuizService.Application.DTOs;
using QuizService.Application.Interfaces;
using QuizService.Domain.Entities;
using QuizService.Domain.Exceptions;
using QuizService.Infrastructure.Persistence;

namespace QuizService.Application.Services;

public class QuizService : IQuizService
{
    private readonly QuizDbContext _context;

    public QuizService(QuizDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateQuizAsync(CreateQuizDto dto)
    {
        var quiz = new Quiz
        {
            QuizId = Guid.NewGuid(),
            CourseId = dto.CourseId,
            Title = dto.Title,
            PassingScore = dto.PassingScore
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        return quiz.QuizId;
    }

    public async Task AddQuestionAsync(CreateQuestionDto dto)
    {
        var quiz = await _context.Quizzes.FindAsync(dto.QuizId);
        if (quiz == null)
            throw new QuizNotFoundException(dto.QuizId);

        var question = new Question
        {
            QuestionId = Guid.NewGuid(),
            QuizId = dto.QuizId,
            Text = dto.Text,
            OptionA = dto.OptionA,
            OptionB = dto.OptionB,
            OptionC = dto.OptionC,
            OptionD = dto.OptionD,
            CorrectAnswer = dto.CorrectAnswer
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
    }

    public async Task<QuizResultDto> SubmitQuizAsync(SubmitQuizDto dto)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.QuizId == dto.QuizId);

        if (quiz == null)
            throw new QuizNotFoundException(dto.QuizId);

        int score = 0;

        foreach (var answer in dto.Answers)
        {
            var question = quiz.Questions
                .FirstOrDefault(q => q.QuestionId == answer.QuestionId);

            if (question == null)
                throw new QuestionNotFoundException(answer.QuestionId);

            if (question.CorrectAnswer == answer.SelectedAnswer)
                score++;
        }

        bool isPassed = score >= quiz.PassingScore;

        var attempt = new QuizAttempt
        {
            AttemptId = Guid.NewGuid(),
            QuizId = dto.QuizId,
            UserId = dto.UserId,
            Score = score,
            IsPassed = isPassed,
            AttemptedAt = DateTime.UtcNow
        };

        _context.QuizAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        return new QuizResultDto
        {
            Score = score,
            IsPassed = isPassed
        };
    }
}