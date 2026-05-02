// QuizServiceTests — Unit tests for QuizService.Application.Services.QuizService
// Uses InMemory EF Core DbContext since QuizService works directly with DbContext.

using Microsoft.EntityFrameworkCore;
using Moq;
using QuizService.Application.DTOs;
using QuizService.Domain.Entities;
using QuizService.Domain.Enums;
using QuizService.Domain.Exceptions;
using QuizService.Infrastructure.Persistence;
using QuizService.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;

namespace NUnitTesting.QuizServiceTests;

[TestFixture]
public class QuizServiceTests
{
    private QuizDbContext _context;
    private Mock<RabbitMqPublisher> _publisherMock;
    private QuizService.Application.Services.QuizService _quizService;

    [SetUp]
    public void Setup()
    {
        // Use InMemory database for each test (unique name to avoid cross-test leaks)
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new QuizDbContext(options);

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["RabbitMQ:Host"]).Returns("localhost");
        _publisherMock = new Mock<RabbitMqPublisher>(configMock.Object);

        _quizService = new QuizService.Application.Services.QuizService(_context, _publisherMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    // ─── CreateQuiz Tests ─────────────────────────────────────────────────────

    [Test]
    public async Task CreateQuizAsync_ReturnsValidQuizId()
    {
        // Arrange
        var dto = new CreateQuizDto
        {
            CourseId = Guid.NewGuid(),
            Title = "C# Fundamentals Quiz",
            PassingScore = 70
        };

        // Act
        var quizId = await _quizService.CreateQuizAsync(dto);

        // Assert
        Assert.That(quizId, Is.Not.EqualTo(Guid.Empty));
        var quiz = await _context.Quizzes.FindAsync(quizId);
        Assert.That(quiz, Is.Not.Null);
        Assert.That(quiz!.Title, Is.EqualTo("C# Fundamentals Quiz"));
        Assert.That(quiz.PassingScore, Is.EqualTo(70));
    }

    // ─── UpdateQuiz Tests ─────────────────────────────────────────────────────

    [Test]
    public async Task UpdateQuizAsync_ExistingQuiz_UpdatesFields()
    {
        // Arrange
        var quiz = new Quiz
        {
            QuizId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            Title = "Old Title",
            PassingScore = 50,
            CreatedAt = DateTime.UtcNow
        };
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        var dto = new UpdateQuizDto { Title = "New Title", PassingScore = 80 };

        // Act
        await _quizService.UpdateQuizAsync(quiz.QuizId, dto);

        // Assert
        var updated = await _context.Quizzes.FindAsync(quiz.QuizId);
        Assert.That(updated!.Title, Is.EqualTo("New Title"));
        Assert.That(updated.PassingScore, Is.EqualTo(80));
    }

    [Test]
    public void UpdateQuizAsync_NonExistent_ThrowsQuizNotFound()
    {
        var dto = new UpdateQuizDto { Title = "Title", PassingScore = 50 };

        Assert.ThrowsAsync<QuizNotFoundException>(
            async () => await _quizService.UpdateQuizAsync(Guid.NewGuid(), dto));
    }

    // ─── AddQuestion Tests ────────────────────────────────────────────────────

    [Test]
    public async Task AddQuestionAsync_ValidInput_AddsToQuiz()
    {
        // Arrange
        var quiz = new Quiz
        {
            QuizId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            Title = "Quiz",
            PassingScore = 60,
            CreatedAt = DateTime.UtcNow
        };
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        var dto = new CreateQuestionDto
        {
            Text = "What is C#?",
            Type = "MultipleChoice",
            CorrectAnswer = "A programming language",
            Options = "A programming language,A fruit,A color,A country"
        };

        // Act
        await _quizService.AddQuestionAsync(quiz.QuizId, dto);

        // Assert
        var questions = await _context.Questions.Where(q => q.QuizId == quiz.QuizId).ToListAsync();
        Assert.That(questions, Has.Count.EqualTo(1));
        Assert.That(questions[0].Text, Is.EqualTo("What is C#?"));
        Assert.That(questions[0].Type, Is.EqualTo(QuestionType.MultipleChoice));
    }

    [Test]
    public void AddQuestionAsync_InvalidType_ThrowsArgumentException()
    {
        // Arrange
        var quiz = new Quiz
        {
            QuizId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            Title = "Quiz",
            PassingScore = 60,
            CreatedAt = DateTime.UtcNow
        };
        _context.Quizzes.Add(quiz);
        _context.SaveChanges();

        var dto = new CreateQuestionDto
        {
            Text = "Question",
            Type = "InvalidType",
            CorrectAnswer = "Answer",
            Options = "A,B"
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _quizService.AddQuestionAsync(quiz.QuizId, dto));
    }

    // ─── DeleteQuestion Tests ─────────────────────────────────────────────────

    [Test]
    public async Task DeleteQuestionAsync_ExistingQuestion_RemovesIt()
    {
        // Arrange
        var question = new Question
        {
            QuestionId = Guid.NewGuid(),
            QuizId = Guid.NewGuid(),
            Text = "Q",
            Type = QuestionType.MultipleChoice,
            CorrectAnswer = "A",
            Options = "A,B"
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act
        await _quizService.DeleteQuestionAsync(question.QuestionId);

        // Assert
        var deleted = await _context.Questions.FindAsync(question.QuestionId);
        Assert.That(deleted, Is.Null);
    }

    // ─── GetQuizDetails Tests ─────────────────────────────────────────────────

    [Test]
    public async Task GetQuizDetailsAsync_ExistingQuiz_ReturnsWithQuestions()
    {
        // Arrange
        var quiz = new Quiz
        {
            QuizId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            Title = "Quiz",
            PassingScore = 60,
            CreatedAt = DateTime.UtcNow
        };
        quiz.Questions.Add(new Question
        {
            QuestionId = Guid.NewGuid(),
            QuizId = quiz.QuizId,
            Text = "Q1",
            Type = QuestionType.MultipleChoice,
            CorrectAnswer = "A",
            Options = "A,B,C"
        });
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        // Act
        var result = await _quizService.GetQuizDetailsAsync(quiz.QuizId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Questions, Has.Count.EqualTo(1));
    }

    // ─── StartQuiz Tests ──────────────────────────────────────────────────────

    [Test]
    public async Task StartQuizAsync_CreatesAttemptInProgress()
    {
        // Arrange
        var quiz = new Quiz
        {
            QuizId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            Title = "Quiz",
            PassingScore = 60,
            CreatedAt = DateTime.UtcNow
        };
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        var studentId = Guid.NewGuid();

        // Act
        var attemptId = await _quizService.StartQuizAsync(quiz.QuizId, studentId);

        // Assert
        Assert.That(attemptId, Is.Not.EqualTo(Guid.Empty));
        var attempt = await _context.QuizAttempts.FindAsync(attemptId);
        Assert.That(attempt, Is.Not.Null);
        Assert.That(attempt!.Status, Is.EqualTo(AttemptStatus.InProgress));
        Assert.That(attempt.StudentId, Is.EqualTo(studentId));
    }

    // ─── SubmitQuiz Tests ─────────────────────────────────────────────────────
    // NOTE: Full scoring tests (AllCorrect / AllWrong) are integration tests because
    // QuizService.SubmitQuizAsync calls RabbitMqPublisher.PublishAsync (non-virtual concrete class)
    // at the end, which tries to connect to RabbitMQ. We verify scoring logic through
    // the DB state and focus unit tests on the guard-clause paths.

    [Test]
    public async Task SubmitQuizAsync_ScoringLogic_CorrectAnswer_SetsPassedInDb()
    {
        // Arrange — verify scoring logic by inspecting the DB state
        // after the service processes the answers (publisher will throw,
        // but the DB write happens before the publish call).
        var quiz = new Quiz
        {
            QuizId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            Title = "Quiz",
            PassingScore = 60,
            CreatedAt = DateTime.UtcNow
        };
        var q1 = new Question
        {
            QuestionId = Guid.NewGuid(),
            QuizId = quiz.QuizId,
            Text = "What is 1+1?",
            Type = QuestionType.MultipleChoice,
            CorrectAnswer = "2",
            Options = "1,2,3,4"
        };
        quiz.Questions.Add(q1);
        _context.Quizzes.Add(quiz);

        var studentId = Guid.NewGuid();
        var attempt = new QuizAttempt
        {
            AttemptId = Guid.NewGuid(),
            QuizId = quiz.QuizId,
            StudentId = studentId,
            Score = 0,
            Status = AttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };
        _context.QuizAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        var submitDto = new SubmitQuizDto
        {
            Answers = new List<AnswerDto>
            {
                new AnswerDto { QuestionId = q1.QuestionId, SelectedAnswer = "2" }
            }
        };

        // Act — the publisher will throw (no RabbitMQ), but SaveChangesAsync 
        // is called BEFORE publish, so the DB state reflects the scoring.
        try
        {
            await _quizService.SubmitQuizAsync(attempt.AttemptId, submitDto, studentId, "student@test.com");
        }
        catch { /* Expected: RabbitMQ connection failure */ }

        // Assert — check DB state directly
        var savedAttempt = await _context.QuizAttempts.FindAsync(attempt.AttemptId);
        Assert.That(savedAttempt!.Score, Is.EqualTo(100));
        Assert.That(savedAttempt.Status, Is.EqualTo(AttemptStatus.Passed));

        var userAnswers = await _context.UserAnswers
            .Where(ua => ua.AttemptId == attempt.AttemptId).ToListAsync();
        Assert.That(userAnswers, Has.Count.EqualTo(1));
        Assert.That(userAnswers[0].IsCorrect, Is.True);
    }

    [Test]
    public async Task SubmitQuizAsync_ScoringLogic_WrongAnswer_SetsFailedInDb()
    {
        // Arrange
        var quiz = new Quiz
        {
            QuizId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            Title = "Quiz",
            PassingScore = 60,
            CreatedAt = DateTime.UtcNow
        };
        var q1 = new Question
        {
            QuestionId = Guid.NewGuid(),
            QuizId = quiz.QuizId,
            Text = "What is 1+1?",
            Type = QuestionType.MultipleChoice,
            CorrectAnswer = "2",
            Options = "1,2,3,4"
        };
        quiz.Questions.Add(q1);
        _context.Quizzes.Add(quiz);

        var studentId = Guid.NewGuid();
        var attempt = new QuizAttempt
        {
            AttemptId = Guid.NewGuid(),
            QuizId = quiz.QuizId,
            StudentId = studentId,
            Score = 0,
            Status = AttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };
        _context.QuizAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        var submitDto = new SubmitQuizDto
        {
            Answers = new List<AnswerDto>
            {
                new AnswerDto { QuestionId = q1.QuestionId, SelectedAnswer = "999" }
            }
        };

        // Act
        try
        {
            await _quizService.SubmitQuizAsync(attempt.AttemptId, submitDto, studentId, "student@test.com");
        }
        catch { /* Expected: RabbitMQ connection failure */ }

        // Assert — check DB state directly
        var savedAttempt = await _context.QuizAttempts.FindAsync(attempt.AttemptId);
        Assert.That(savedAttempt!.Score, Is.EqualTo(0));
        Assert.That(savedAttempt.Status, Is.EqualTo(AttemptStatus.Failed));
    }

    [Test]
    public void SubmitQuizAsync_WrongStudent_ThrowsUnauthorized()
    {
        // Arrange
        var quiz = new Quiz
        {
            QuizId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            Title = "Quiz",
            PassingScore = 60,
            CreatedAt = DateTime.UtcNow
        };
        quiz.Questions.Add(new Question
        {
            QuestionId = Guid.NewGuid(), QuizId = quiz.QuizId,
            Text = "Q", Type = QuestionType.MultipleChoice,
            CorrectAnswer = "A", Options = "A,B"
        });
        _context.Quizzes.Add(quiz);

        var ownerId = Guid.NewGuid();
        var attempt = new QuizAttempt
        {
            AttemptId = Guid.NewGuid(),
            QuizId = quiz.QuizId,
            StudentId = ownerId,
            Score = 0,
            Status = AttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };
        _context.QuizAttempts.Add(attempt);
        _context.SaveChanges();

        var attackerId = Guid.NewGuid();
        var submitDto = new SubmitQuizDto
        {
            Answers = new List<AnswerDto> { new() { QuestionId = Guid.NewGuid(), SelectedAnswer = "A" } }
        };

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedQuizAccessException>(
            async () => await _quizService.SubmitQuizAsync(attempt.AttemptId, submitDto, attackerId, "a@b.com"));
    }

    [Test]
    public void SubmitQuizAsync_AlreadyCompleted_ThrowsQuizSubmissionException()
    {
        // Arrange
        var quiz = new Quiz
        {
            QuizId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            Title = "Quiz",
            PassingScore = 60,
            CreatedAt = DateTime.UtcNow
        };
        quiz.Questions.Add(new Question
        {
            QuestionId = Guid.NewGuid(), QuizId = quiz.QuizId,
            Text = "Q", Type = QuestionType.MultipleChoice,
            CorrectAnswer = "A", Options = "A,B"
        });
        _context.Quizzes.Add(quiz);

        var studentId = Guid.NewGuid();
        var attempt = new QuizAttempt
        {
            AttemptId = Guid.NewGuid(),
            QuizId = quiz.QuizId,
            StudentId = studentId,
            Score = 100,
            Status = AttemptStatus.Passed, // already completed
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };
        _context.QuizAttempts.Add(attempt);
        _context.SaveChanges();

        var submitDto = new SubmitQuizDto { Answers = new List<AnswerDto>() };

        // Act & Assert
        Assert.ThrowsAsync<QuizSubmissionException>(
            async () => await _quizService.SubmitQuizAsync(attempt.AttemptId, submitDto, studentId, "a@b.com"));
    }
}
