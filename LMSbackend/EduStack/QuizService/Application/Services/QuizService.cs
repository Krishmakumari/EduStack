// QuizService — Core business logic for taking and managing quizzes.
// ARCHITECTURE NOTE: Uses DbContext directly (no Repository layer).
// • This is a valid design choice ("DbContext is the Unit of Work / Repository").
// • Allows direct querying and INCLUDE statements without abstraction overhead.

using Microsoft.EntityFrameworkCore;
using QuizService.Application.DTOs;
using QuizService.Application.Interfaces;
using QuizService.Domain.Entities;
using QuizService.Domain.Enums;
using QuizService.Domain.Exceptions;
using QuizService.Infrastructure.Persistence;
using QuizService.Infrastructure.Messaging;
using QuizService.Domain.Events;

namespace QuizService.Application.Services;

public class QuizService : IQuizService
{
    private readonly QuizDbContext _context;
    private readonly RabbitMqPublisher _publisher;

    // Directly injects the DbContext and Publisher.
    public QuizService(QuizDbContext context, RabbitMqPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    // ─── Admin / Instructor Operations ────────────────────────────────────────

    public async Task<Guid> CreateQuizAsync(CreateQuizDto dto)
    {
        var quiz = new Quiz
        {
            QuizId = Guid.NewGuid(),
            CourseId = dto.CourseId,
            Title = dto.Title,
            PassingScore = dto.PassingScore,
            CreatedAt = DateTime.UtcNow
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        return quiz.QuizId;
    }

    public async Task UpdateQuizAsync(Guid quizId, UpdateQuizDto dto)
    {
        var quiz = await _context.Quizzes.FindAsync(quizId)
            ?? throw new QuizNotFoundException();

        quiz.Title = dto.Title;
        quiz.PassingScore = dto.PassingScore;

        await _context.SaveChangesAsync();
    }

    public async Task AddQuestionAsync(Guid quizId, CreateQuestionDto dto)
    {
        var quiz = await _context.Quizzes.FindAsync(quizId) 
            ?? throw new QuizNotFoundException();

        if (!Enum.TryParse(dto.Type, true, out QuestionType type))
            throw new ArgumentException("Invalid question type.");

        var question = new Question
        {
            QuestionId = Guid.NewGuid(),
            QuizId = quizId,
            Text = dto.Text,
            Type = type,
            CorrectAnswer = dto.CorrectAnswer,
            Options = dto.Options
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateQuestionAsync(Guid questionId, UpdateQuestionDto dto)
    {
        var question = await _context.Questions.FindAsync(questionId)
            ?? throw new QuestionNotFoundException(questionId);

        if (!Enum.TryParse(dto.Type, true, out QuestionType type))
            throw new ArgumentException("Invalid question type.");

        question.Text = dto.Text;
        question.Type = type;
        question.CorrectAnswer = dto.CorrectAnswer;
        question.Options = dto.Options;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteQuestionAsync(Guid questionId)
    {
        var question = await _context.Questions.FindAsync(questionId)
            ?? throw new QuestionNotFoundException(questionId);

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();
    }

    public async Task<Quiz> GetQuizDetailsAsync(Guid quizId)
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.QuizId == quizId)
            ?? throw new QuizNotFoundException();
    }

    public async Task<Quiz?> GetQuizByCourseIdAsync(Guid courseId)
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.CourseId == courseId);
    }

    // ─── Student Operations ──────────────────────────────────────────────────

    // Begins an attempt tracking state for a user.
    // Enrolment check is bypassed here (assumes upstream API Gateway / Client filtering).
    public async Task<Guid> StartQuizAsync(Guid quizId, Guid studentId)
    {
        var quiz = await _context.Quizzes.FindAsync(quizId)
            ?? throw new QuizNotFoundException();

        var attempt = new QuizAttempt
        {
            AttemptId = Guid.NewGuid(),
            QuizId = quizId,
            StudentId = studentId,
            Score = 0,
            Status = AttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };

        _context.QuizAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        return attempt.AttemptId;
    }

    // Handles grading: calculates score, checks passing condition, saves history.
    public async Task<QuizResultDto> SubmitQuizAsync(Guid attemptId, SubmitQuizDto dto, Guid studentId, string studentEmail)
    {
        // Must Include Questions to calculate score.
        var attempt = await _context.QuizAttempts
            .Include(a => a.Quiz)
            .ThenInclude(q => q.Questions)
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId)
            ?? throw new QuizNotFoundException();

        // Security check: cannot submit someone else's attempt.
        if (attempt.StudentId != studentId)
            throw new UnauthorizedQuizAccessException();

        // Idempotency / State validation.
        if (attempt.Status != AttemptStatus.InProgress)
            throw new QuizSubmissionException("Quiz attempt is already completed.");

        var questions = attempt.Quiz.Questions;
        int correctCount = 0;
        
        foreach (var answerDto in dto.Answers)
        {
            var question = questions.FirstOrDefault(q => q.QuestionId == answerDto.QuestionId)
                ?? throw new QuestionNotFoundException(answerDto.QuestionId);

            // Resilient matching: ignoring case and trimming accidental whitespace.
            bool isCorrect = string.Equals(question.CorrectAnswer.Trim(), answerDto.SelectedAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
            
            if (isCorrect) correctCount++;

            _context.UserAnswers.Add(new UserAnswer
            {
                AnswerId = Guid.NewGuid(),
                AttemptId = attemptId,
                QuestionId = question.QuestionId,
                SelectedAnswer = answerDto.SelectedAnswer,
                IsCorrect = isCorrect
            });
        }

        // Calculate score out of 100
        attempt.Score = questions.Count == 0 ? 0 : (correctCount * 100m) / questions.Count;
        
        // Finalize Attempt
        attempt.Status = attempt.Score >= attempt.Quiz.PassingScore 
            ? AttemptStatus.Passed 
            : AttemptStatus.Failed;
            
        attempt.CompletedAt = DateTime.UtcNow;

        Console.WriteLine($"[QuizService] Saving {dto.Answers.Count} answers for attempt {attemptId}...");
        await _context.SaveChangesAsync();
        Console.WriteLine($"[QuizService] Submission completed for attempt {attemptId}.");

        // 🔥 Publish "QuizResultEvent" to RabbitMQ.
        // NotificationService will pick this up and send an email.
        await _publisher.PublishAsync("quiz_queue", new QuizResultEvent
        {
            UserId = studentId,
            Email = studentEmail,
            Passed = attempt.Status == AttemptStatus.Passed
        });

        return new QuizResultDto
        {
            AttemptId = attempt.AttemptId,
            Score = attempt.Score,
            Passed = attempt.Status == AttemptStatus.Passed,
            CorrectAnswers = correctCount,
            TotalQuestions = questions.Count
        };
    }
}