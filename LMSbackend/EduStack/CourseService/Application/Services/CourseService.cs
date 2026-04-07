using CourseService.Application.DTOs.Requests;
using CourseService.Application.DTOs.Responses;
using CourseService.Application.Interfaces;
using CourseService.Domain.Entities;
using CourseService.Domain.Enums;
using CourseService.Domain.Exceptions;

namespace CourseService.Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepo;
    private readonly ISectionRepository _sectionRepo;
    private readonly ILessonRepository _lessonRepo;

    public CourseService(
        ICourseRepository courseRepo,
        ISectionRepository sectionRepo,
        ILessonRepository lessonRepo)
    {
        _courseRepo = courseRepo;
        _sectionRepo = sectionRepo;
        _lessonRepo = lessonRepo;
    }

    // ─── Get All Courses ──────────────────────────────────────────────────────
    public async Task<IEnumerable<CourseResponse>> GetAllCoursesAsync()
    {
        var courses = await _courseRepo.GetAllAsync();
        return courses.Select(MapToCourseResponse);
    }

    // ─── Get My Courses (Instructor) ──────────────────────────────────────────
    public async Task<IEnumerable<CourseResponse>> GetMyCourseAsync(Guid instructorId)
    {
        var courses = await _courseRepo.GetByInstructorIdAsync(instructorId);
        return courses.Select(MapToCourseResponse);
    }

    // ─── Get Course By ID (with sections + lessons) ───────────────────────────
    public async Task<CourseDetailResponse> GetCourseByIdAsync(Guid courseId)
    {
        var course = await _courseRepo.GetByIdWithSectionsAsync(courseId)
            ?? throw new CourseNotFoundException();

        return MapToCourseDetailResponse(course);
    }

    // ─── Create Course ────────────────────────────────────────────────────────
    public async Task<CourseResponse> CreateCourseAsync(
        Guid instructorId, string instructorName, CreateCourseRequest request)
    {
        if (!Enum.TryParse<CourseLevel>(request.Level, ignoreCase: true, out var level))
            throw new DomainException("Invalid course level.");

        var course = Course.Create(
            request.Title,
            request.Description,
            request.ThumbnailUrl,
            request.Price,
            level,
            request.Language,
            instructorId,
            instructorName);

        await _courseRepo.AddAsync(course);
        await _courseRepo.SaveChangesAsync();

        return MapToCourseResponse(course);
    }

    // ─── Update Course ────────────────────────────────────────────────────────
    public async Task<CourseResponse> UpdateCourseAsync(
        Guid instructorId, Guid courseId, UpdateCourseRequest request)
    {
        var course = await _courseRepo.GetByIdAsync(courseId)
            ?? throw new CourseNotFoundException();

        if (course.InstructorId != instructorId)
            throw new UnAuthorizedAccessException();

        if (!Enum.TryParse<CourseLevel>(request.Level, ignoreCase: true, out var level))
            throw new DomainException("Invalid course level.");

        course.Update(
            request.Title,
            request.Description,
            request.ThumbnailUrl,
            request.Price,
            level,
            request.Language);

        await _courseRepo.SaveChangesAsync();

        return MapToCourseResponse(course);
    }

    // ─── Delete Course ────────────────────────────────────────────────────────
    public async Task DeleteCourseAsync(Guid instructorId, Guid courseId)
    {
        var course = await _courseRepo.GetByIdAsync(courseId)
            ?? throw new CourseNotFoundException();

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        await _courseRepo.DeleteAsync(course);
        await _courseRepo.SaveChangesAsync();
    }

    // ─── Publish ──────────────────────────────────────────────────────────────
    public async Task PublishCourseAsync(Guid instructorId, Guid courseId)
    {
        var course = await _courseRepo.GetByIdWithSectionsAsync(courseId)
            ?? throw new CourseNotFoundException();

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        course.Publish();
        await _courseRepo.SaveChangesAsync();
    }

    // ─── Unpublish ────────────────────────────────────────────────────────────
    public async Task UnpublishCourseAsync(Guid instructorId, Guid courseId)
    {
        var course = await _courseRepo.GetByIdAsync(courseId)
            ?? throw new CourseNotFoundException();

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        course.Unpublish();
        await _courseRepo.SaveChangesAsync();
    }

    // ─── Add Section ──────────────────────────────────────────────────────────
    public async Task<SectionResponse> AddSectionAsync(
        Guid instructorId, Guid courseId, AddSectionRequest request)
    {
        var course = await _courseRepo.GetByIdAsync(courseId)
            ?? throw new CourseNotFoundException();

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        var section = Section.Create(courseId, request.Title, request.Order);

        await _sectionRepo.AddAsync(section);
        await _sectionRepo.SaveChangesAsync();

        return MapToSectionResponse(section);
    }

    // ─── Update Section ───────────────────────────────────────────────────────
    public async Task<SectionResponse> UpdateSectionAsync(
        Guid instructorId, Guid sectionId, UpdateSectionRequest request)
    {
        var section = await _sectionRepo.GetByIdAsync(sectionId)
            ?? throw new DomainException("Section not found.");

        var course = await _courseRepo.GetByIdAsync(section.CourseId)
            ?? throw new CourseNotFoundException();

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        section.Update(request.Title, request.Order);
        await _sectionRepo.SaveChangesAsync();

        return MapToSectionResponse(section);
    }

    // ─── Delete Section ───────────────────────────────────────────────────────
    public async Task DeleteSectionAsync(Guid instructorId, Guid sectionId)
    {
        var section = await _sectionRepo.GetByIdAsync(sectionId)
            ?? throw new DomainException("Section not found.");

        var course = await _courseRepo.GetByIdAsync(section.CourseId)
            ?? throw new CourseNotFoundException();

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        await _sectionRepo.DeleteAsync(section);
        await _sectionRepo.SaveChangesAsync();
    }

    // ─── Add Lesson ───────────────────────────────────────────────────────────
    public async Task<LessonResponse> AddLessonAsync(
        Guid instructorId, Guid sectionId, AddLessonRequest request)
    {
        var section = await _sectionRepo.GetByIdAsync(sectionId)
            ?? throw new DomainException("Section not found.");

        var course = await _courseRepo.GetByIdAsync(section.CourseId)
            ?? throw new CourseNotFoundException();

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        var lesson = Lesson.Create(
            sectionId,
            request.Title,
            request.VideoUrl,
            request.Content,
            request.DurationInSeconds,
            request.Order,
            request.IsFreePreview);

        await _lessonRepo.AddAsync(lesson);
        await _lessonRepo.SaveChangesAsync();

        return MapToLessonResponse(lesson);
    }

    // ─── Update Lesson ────────────────────────────────────────────────────────
    public async Task<LessonResponse> UpdateLessonAsync(
        Guid instructorId, Guid lessonId, UpdateLessonRequest request)
    {
        var lesson = await _lessonRepo.GetByIdAsync(lessonId)
            ?? throw new DomainException("Lesson not found.");

        var section = await _sectionRepo.GetByIdAsync(lesson.SectionId)
            ?? throw new DomainException("Section not found.");

        var course = await _courseRepo.GetByIdAsync(section.CourseId)
            ?? throw new CourseNotFoundException();

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        lesson.Update(
            request.Title,
            request.VideoUrl,
            request.Content,
            request.DurationInSeconds,
            request.Order,
            request.IsFreePreview);

        await _lessonRepo.SaveChangesAsync();

        return MapToLessonResponse(lesson);
    }

    // ─── Delete Lesson ────────────────────────────────────────────────────────
    public async Task DeleteLessonAsync(Guid instructorId, Guid lessonId)
    {
        var lesson = await _lessonRepo.GetByIdAsync(lessonId)
            ?? throw new DomainException("Lesson not found.");

        var section = await _sectionRepo.GetByIdAsync(lesson.SectionId)
            ?? throw new DomainException("Section not found.");

        var course = await _courseRepo.GetByIdAsync(section.CourseId)
            ?? throw new CourseNotFoundException();

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        await _lessonRepo.DeleteAsync(lesson);
        await _lessonRepo.SaveChangesAsync();
    }

    // ─── Mapping Helpers ──────────────────────────────────────────────────────
    private static CourseResponse MapToCourseResponse(Course course) => new()
    {
        CourseId = course.CourseId,
        Title = course.Title,
        Description = course.Description,
        ThumbnailUrl = course.ThumbnailUrl,
        Price = course.Price,
        Level = course.Level.ToString(),
        Status = course.Status.ToString(),
        Language = course.Language,
        InstructorName = course.InstructorName,
        InstructorId = course.InstructorId,
        CreatedAt = course.CreatedAt
    };

    private static CourseDetailResponse MapToCourseDetailResponse(Course course) => new()
    {
        CourseId = course.CourseId,
        Title = course.Title,
        Description = course.Description,
        ThumbnailUrl = course.ThumbnailUrl,
        Price = course.Price,
        Level = course.Level.ToString(),
        Status = course.Status.ToString(),
        Language = course.Language,
        InstructorName = course.InstructorName,
        InstructorId = course.InstructorId,
        CreatedAt = course.CreatedAt,
        Sections = course.Sections
            .OrderBy(s => s.Order)
            .Select(MapToSectionResponse)
            .ToList()
    };

    private static SectionResponse MapToSectionResponse(Section section) => new()
    {
        SectionId = section.SectionId,
        Title = section.Title,
        Order = section.Order,
        Lessons = section.Lessons
            .OrderBy(l => l.Order)
            .Select(MapToLessonResponse)
            .ToList()
    };

    private static LessonResponse MapToLessonResponse(Lesson lesson) => new()
    {
        LessonId = lesson.LessonId,
        Title = lesson.Title,
        VideoUrl = lesson.VideoUrl,
        Content = lesson.Content,
        DurationInSeconds = lesson.DurationInSeconds,
        Order = lesson.Order,
        IsFreePreview = lesson.IsFreePreview
    };
}