// CourseServiceTests — Unit tests for CourseService.Application.Services.CourseService
// Tests CRUD operations, ownership validation, and status lifecycle.

using Moq;
using CourseService.Application.DTOs.Requests;
using CourseService.Application.Interfaces;
using CourseService.Domain.Entities;
using CourseService.Domain.Enums;
using CourseService.Domain.Exceptions;

namespace NUnitTesting.CourseServiceTests;

[TestFixture]
public class CourseServiceTests
{
    private Mock<ICourseRepository> _courseRepoMock;
    private Mock<ISectionRepository> _sectionRepoMock;
    private Mock<ILessonRepository> _lessonRepoMock;
    private Mock<ICategoryRepository> _categoryRepoMock;
    private CourseService.Application.Services.CourseService _courseService;

    [SetUp]
    public void Setup()
    {
        _courseRepoMock = new Mock<ICourseRepository>();
        _sectionRepoMock = new Mock<ISectionRepository>();
        _lessonRepoMock = new Mock<ILessonRepository>();
        _categoryRepoMock = new Mock<ICategoryRepository>();

        _courseService = new CourseService.Application.Services.CourseService(
            _courseRepoMock.Object,
            _sectionRepoMock.Object,
            _lessonRepoMock.Object,
            _categoryRepoMock.Object);
    }

    // ─── GetAllCourses ────────────────────────────────────────────────────────

    [Test]
    public async Task GetAllCoursesAsync_ReturnsCourseResponses()
    {
        // Arrange
        var courses = new List<Course>
        {
            Course.Create("C# Basics", "Learn C#", "thumb.jpg", 499m, CourseLevel.Beginner, "English", Guid.NewGuid(), "Instructor1"),
            Course.Create("Advanced .NET", "Deep dive", "thumb2.jpg", 999m, CourseLevel.Advanced, "English", Guid.NewGuid(), "Instructor2")
        };

        _courseRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(courses);

        // Act
        var result = (await _courseService.GetAllCoursesAsync()).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].Title, Is.EqualTo("C# Basics"));
        Assert.That(result[1].Level, Is.EqualTo("Advanced"));
        Assert.That(result[0].Status, Is.EqualTo("Draft"));
    }

    // ─── GetMyCourses ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetMyCourseAsync_ReturnsOnlyInstructorCourses()
    {
        // Arrange
        var instructorId = Guid.NewGuid();
        var courses = new List<Course>
        {
            Course.Create("My Course", "Desc", "thumb.jpg", 199m, CourseLevel.Intermediate, "English", instructorId, "Me")
        };

        _courseRepoMock.Setup(r => r.GetByInstructorIdAsync(instructorId)).ReturnsAsync(courses);

        // Act
        var result = (await _courseService.GetMyCourseAsync(instructorId)).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].InstructorId, Is.EqualTo(instructorId));
    }

    // ─── CreateCourse ─────────────────────────────────────────────────────────

    [Test]
    public async Task CreateCourseAsync_ValidRequest_ReturnsCourseResponse()
    {
        // Arrange
        var instructorId = Guid.NewGuid();
        var request = new CreateCourseRequest
        {
            Title = "New Course",
            Description = "Description",
            ThumbnailUrl = "thumb.jpg",
            Price = 299m,
            Level = "Beginner",
            Language = "English"
        };

        _courseRepoMock.Setup(r => r.AddAsync(It.IsAny<Course>())).Returns(Task.CompletedTask);
        _courseRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _courseService.CreateCourseAsync(instructorId, "Instructor Name", request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("New Course"));
        Assert.That(result.Status, Is.EqualTo("Draft"));
        Assert.That(result.InstructorId, Is.EqualTo(instructorId));
    }

    [Test]
    public void CreateCourseAsync_InvalidLevel_ThrowsDomainException()
    {
        // Arrange
        var request = new CreateCourseRequest
        {
            Title = "Course",
            Description = "Desc",
            ThumbnailUrl = "t.jpg",
            Price = 100m,
            Level = "SuperExpert", // invalid
            Language = "English"
        };

        // Act & Assert
        Assert.ThrowsAsync<DomainException>(
            async () => await _courseService.CreateCourseAsync(Guid.NewGuid(), "Instr", request));
    }

    // ─── UpdateCourse ─────────────────────────────────────────────────────────

    [Test]
    public async Task UpdateCourseAsync_OwnerUpdates_ReturnsUpdatedResponse()
    {
        // Arrange
        var instructorId = Guid.NewGuid();
        var course = Course.Create("Old Title", "Old Desc", "old.jpg", 100m, CourseLevel.Beginner, "English", instructorId, "Instr");

        _courseRepoMock.Setup(r => r.GetByIdAsync(course.CourseId)).ReturnsAsync(course);
        _courseRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = new UpdateCourseRequest
        {
            Title = "New Title",
            Description = "New Desc",
            ThumbnailUrl = "new.jpg",
            Price = 200m,
            Level = "Advanced",
            Language = "Hindi"
        };

        // Act
        var result = await _courseService.UpdateCourseAsync(instructorId, course.CourseId, request);

        // Assert
        Assert.That(result.Title, Is.EqualTo("New Title"));
        Assert.That(result.Level, Is.EqualTo("Advanced"));
    }

    [Test]
    public void UpdateCourseAsync_NotOwner_ThrowsUnauthorized()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var course = Course.Create("Course", "Desc", "t.jpg", 100m, CourseLevel.Beginner, "EN", ownerId, "Owner");

        _courseRepoMock.Setup(r => r.GetByIdAsync(course.CourseId)).ReturnsAsync(course);

        var request = new UpdateCourseRequest
        {
            Title = "Hacked",
            Description = "d",
            ThumbnailUrl = "t",
            Price = 0,
            Level = "Beginner",
            Language = "EN"
        };

        // Act & Assert — attackerId != ownerId should throw
        Assert.ThrowsAsync<CourseService.Domain.Exceptions.UnAuthorizedAccessException>(
            async () => await _courseService.UpdateCourseAsync(attackerId, course.CourseId, request));
    }

    [Test]
    public void UpdateCourseAsync_NonExistentCourse_ThrowsCourseNotFound()
    {
        // Arrange
        _courseRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Course?)null);

        var request = new UpdateCourseRequest
        {
            Title = "X", Description = "X", ThumbnailUrl = "X",
            Price = 0, Level = "Beginner", Language = "EN"
        };

        // Act & Assert
        Assert.ThrowsAsync<CourseNotFoundException>(
            async () => await _courseService.UpdateCourseAsync(Guid.NewGuid(), Guid.NewGuid(), request));
    }

    // ─── DeleteCourse ─────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteCourseAsync_Owner_DeletesSuccessfully()
    {
        // Arrange
        var instructorId = Guid.NewGuid();
        var course = Course.Create("Course", "Desc", "t.jpg", 100m, CourseLevel.Beginner, "EN", instructorId, "Instr");

        _courseRepoMock.Setup(r => r.GetByIdAsync(course.CourseId)).ReturnsAsync(course);
        _courseRepoMock.Setup(r => r.DeleteAsync(course)).Returns(Task.CompletedTask);
        _courseRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act & Assert — should not throw
        Assert.DoesNotThrowAsync(
            async () => await _courseService.DeleteCourseAsync(instructorId, course.CourseId));

        _courseRepoMock.Verify(r => r.DeleteAsync(course), Times.Once);
    }

    [Test]
    public void DeleteCourseAsync_NotOwner_ThrowsUnauthorized()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var course = Course.Create("Course", "Desc", "t.jpg", 100m, CourseLevel.Beginner, "EN", ownerId, "Owner");

        _courseRepoMock.Setup(r => r.GetByIdAsync(course.CourseId)).ReturnsAsync(course);

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _courseService.DeleteCourseAsync(Guid.NewGuid(), course.CourseId));
    }

    // ─── AddSection ───────────────────────────────────────────────────────────

    [Test]
    public async Task AddSectionAsync_Owner_ReturnsSectionResponse()
    {
        // Arrange
        var instructorId = Guid.NewGuid();
        var course = Course.Create("Course", "Desc", "t.jpg", 100m, CourseLevel.Beginner, "EN", instructorId, "Instr");

        _courseRepoMock.Setup(r => r.GetByIdAsync(course.CourseId)).ReturnsAsync(course);
        _sectionRepoMock.Setup(r => r.AddAsync(It.IsAny<Section>())).Returns(Task.CompletedTask);
        _sectionRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = new AddSectionRequest { Title = "Section 1", Order = 1 };

        // Act
        var result = await _courseService.AddSectionAsync(instructorId, course.CourseId, request);

        // Assert
        Assert.That(result.Title, Is.EqualTo("Section 1"));
        Assert.That(result.Order, Is.EqualTo(1));
    }

    // ─── Category Management ──────────────────────────────────────────────────

    [Test]
    public async Task CreateCategoryAsync_ReturnsCategoryResponse()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);
        _categoryRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _courseService.CreateCategoryAsync("Programming", "Learn to code");

        // Assert
        Assert.That(result.Name, Is.EqualTo("Programming"));
        Assert.That(result.Description, Is.EqualTo("Learn to code"));
    }

    [Test]
    public void DeleteCategoryAsync_NonExistent_ThrowsKeyNotFound()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Category?)null);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _courseService.DeleteCategoryAsync(Guid.NewGuid()));
    }
}
