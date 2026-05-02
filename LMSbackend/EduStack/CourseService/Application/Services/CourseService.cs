// CourseService — Core business logic for course management (Application Layer).
// • CRUD for courses, sections, lessons with ownership validation.
// • Publishing enforces business rule: must have ≥1 section.
// • Every write operation validates InstructorId matches JWT user.

using CourseService.Application.DTOs.Requests;
using CourseService.Application.DTOs.Responses;
using CourseService.Application.Interfaces;
using CourseService.Domain.Entities;
using CourseService.Domain.Enums;
using CourseService.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Application.Services;

public class CourseService : ICourseService
{
    // All three repositories are injected via DI (constructor injection).
    // Each talks to its own DbSet in CourseDbContext.
    // We need all three because course operations often span multiple entities
    // (e.g., adding a lesson requires checking the section AND the course for ownership).
    private readonly ICourseRepository _courseRepo;
    private readonly ISectionRepository _sectionRepo;
    private readonly ILessonRepository _lessonRepo;
    private readonly ICategoryRepository _categoryRepo;

    public CourseService(
        ICourseRepository courseRepo,
        ISectionRepository sectionRepo,
        ILessonRepository lessonRepo,
        ICategoryRepository categoryRepo)
    {
        _courseRepo = courseRepo;
        _sectionRepo = sectionRepo;
        _lessonRepo = lessonRepo;
        _categoryRepo = categoryRepo;
    }

    // ─── Get All Courses ──────────────────────────────────────────────────────
    // Returns ALL courses for the public catalog page. No authentication needed.
    // Uses the lightweight CourseResponse (no nested sections/lessons) to keep
    // the payload small — listing 100 courses with full trees would be too heavy.
    public async Task<IEnumerable<CourseResponse>> GetAllCoursesAsync()
    {
        var courses = await _courseRepo.GetAllAsync();
        return courses.Select(MapToCourseResponse);
    }

    // ─── Get My Courses (Instructor Dashboard) ────────────────────────────────
    // Returns only courses created by this specific instructor.
    // instructorId comes from JWT "sub" claim — no one can see another
    // instructor's dashboard because they'd need their JWT.
    public async Task<IEnumerable<CourseResponse>> GetMyCourseAsync(Guid instructorId)
    {
        var courses = await _courseRepo.GetByInstructorIdAsync(instructorId);
        return courses.Select(MapToCourseResponse);
    }

    // ─── Get Course By ID (Detail View) ───────────────────────────────────────
    // Returns the FULL course tree: Course → Sections → Lessons (nested).
    // Uses GetByIdWithSectionsAsync which does eager loading with .Include().
    // This is the endpoint students hit when they click on a course card.
    public async Task<CourseDetailResponse> GetCourseByIdAsync(Guid courseId)
    {
        // GetByIdWithSectionsAsync loads sections (ordered) + their lessons (ordered).
        // If course doesn't exist, throw 404 via CourseNotFoundException.
        var course = await _courseRepo.GetByIdWithSectionsAsync(courseId)
            ?? throw new CourseNotFoundException();

        return MapToCourseDetailResponse(course);
    }

    // ─── Create Course ────────────────────────────────────────────────────────
    // Creates a new course in DRAFT status. Only Instructor/Admin can call this.
    // instructorId + instructorName come from JWT claims — we store the name
    // directly on the course (denormalization) to avoid calling Auth Service later.
    public async Task<CourseResponse> CreateCourseAsync(
        Guid instructorId, string instructorName, CreateCourseRequest request)
    {
        // Step 1: Validate the level string ("Beginner", "Intermediate", "Advanced").
        // TryParse is safer than Parse — returns false instead of throwing on invalid input.
        if (!Enum.TryParse<CourseLevel>(request.Level, ignoreCase: true, out var level))
            throw new DomainException("Invalid course level.");

        // Step 2: Create course via factory method.
        // Course.Create() sets Status = Draft and CreatedAt = UtcNow automatically.
        var course = Course.Create(
            request.Title,
            request.Description,
            request.ThumbnailUrl,
            request.Price,
            level,
            request.Language,
            instructorId,       // from JWT "sub" claim
            instructorName);    // from JWT "fullName" claim (denormalized)

        // Step 3: Save — AddAsync marks in EF tracker, SaveChangesAsync runs INSERT SQL.
        await _courseRepo.AddAsync(course);
        await _courseRepo.SaveChangesAsync();

        return MapToCourseResponse(course);
    }

    // ─── Update Course ────────────────────────────────────────────────────────
    // Updates course details (title, description, price, etc.).
    // SECURITY: Only the instructor who created the course can update it.
    public async Task<CourseResponse> UpdateCourseAsync(
        Guid instructorId, Guid courseId, UpdateCourseRequest request)
    {
        // Step 1: Find the course — uses lightweight GetByIdAsync (no sections loaded).
        var course = await _courseRepo.GetByIdAsync(courseId)
            ?? throw new CourseNotFoundException();

        // Step 2: OWNERSHIP CHECK — compare JWT user ID with the course's creator.
        // Without this, any instructor could edit any course.
        if (course.InstructorId != instructorId)
            throw new UnAuthorizedAccessException();

        // Step 3: Validate enum input.
        if (!Enum.TryParse<CourseLevel>(request.Level, ignoreCase: true, out var level))
            throw new DomainException("Invalid course level.");

        // Step 4: Update via domain method — sets UpdatedAt = UtcNow for audit trail.
        // EF change tracking detects the property changes automatically.
        course.Update(
            request.Title,
            request.Description,
            request.ThumbnailUrl,
            request.Price,
            level,
            request.Language);

        // No need to call UpdateAsync — EF tracks changes automatically. Just save.
        await _courseRepo.SaveChangesAsync();

        return MapToCourseResponse(course);
    }

    // ─── Delete Course ────────────────────────────────────────────────────────
    // Deletes the course and ALL its sections + lessons (cascade delete in DB).
    // SECURITY: Only the course owner can delete it.
    public async Task DeleteCourseAsync(Guid instructorId, Guid courseId)
    {
        var course = await _courseRepo.GetByIdAsync(courseId)
            ?? throw new CourseNotFoundException();

        // Ownership check — prevent Instructor A from deleting Instructor B's course.
        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        // Cascade delete: SQL Server auto-deletes all sections and lessons
        // because of OnDelete(DeleteBehavior.Cascade) in CourseConfiguration.
        await _courseRepo.DeleteAsync(course);
        await _courseRepo.SaveChangesAsync();
    }

    public async Task AdminDeleteCourseAsync(Guid courseId)
    {
        var course = await _courseRepo.GetByIdAsync(courseId)
            ?? throw new CourseNotFoundException();

        await _courseRepo.DeleteAsync(course);
        await _courseRepo.SaveChangesAsync();
    }

    public async Task<IEnumerable<CourseResponse>> GetAllCoursesAdminAsync()
    {
        var courses = await _courseRepo.GetAllAsync();
        return courses.Select(MapToCourseResponse);
    }

    public async Task SubmitCourseForReviewAsync(Guid instructorId, Guid courseId)
    {
        var course = await _courseRepo.GetByIdWithSectionsAsync(courseId)
            ?? throw new CourseNotFoundException();

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        course.SubmitForReview();
        await _courseRepo.SaveChangesAsync();
    }

    public async Task ApproveCourseAsync(Guid courseId)
    {
        var course = await _courseRepo.GetByIdAsync(courseId)
            ?? throw new CourseNotFoundException();

        course.Approve();
        await _courseRepo.SaveChangesAsync();
    }

    public async Task RejectCourseAsync(Guid courseId)
    {
        var course = await _courseRepo.GetByIdAsync(courseId)
            ?? throw new CourseNotFoundException();

        course.Reject();
        await _courseRepo.SaveChangesAsync();
    }

    // ─── Unpublish Course ─────────────────────────────────────────────────────
    // Reverts a published course back to Draft status.
    // Useful when instructor wants to make changes before students see them.
    public async Task UnpublishCourseAsync(Guid instructorId, Guid courseId)
    {
        // Lightweight load — no sections needed (no business rule to check for unpublish).
        var course = await _courseRepo.GetByIdAsync(courseId)
            ?? throw new CourseNotFoundException();

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        course.Unpublish();
        await _courseRepo.SaveChangesAsync();
    }

    // ─── Add Section ──────────────────────────────────────────────────────────
    // Adds a new section to an existing course.
    // OWNERSHIP: Load the COURSE to verify the instructor owns it,
    // even though we're creating a SECTION. This is the "walk up" pattern.
    public async Task<SectionResponse> AddSectionAsync(
        Guid instructorId, Guid courseId, AddSectionRequest request)
    {
        // Step 1: Verify the course exists.
        var course = await _courseRepo.GetByIdAsync(courseId)
            ?? throw new CourseNotFoundException();

        // Step 2: Verify ownership — must own the course to add sections to it.
        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        // Step 3: Create section via factory — sets SectionId + CreatedAt.
        var section = Section.Create(courseId, request.Title, request.Order);

        await _sectionRepo.AddAsync(section);
        await _sectionRepo.SaveChangesAsync();

        return MapToSectionResponse(section);
    }

    // ─── Update Section ───────────────────────────────────────────────────────
    // Updates title and order of an existing section.
    // WALK UP: Section → Course → check InstructorId.
    public async Task<SectionResponse> UpdateSectionAsync(
        Guid instructorId, Guid sectionId, UpdateSectionRequest request)
    {
        // Step 1: Find the section.
        var section = await _sectionRepo.GetByIdAsync(sectionId)
            ?? throw new DomainException("Section not found.");

        // Step 2: Walk UP to the parent course to verify ownership.
        var course = await _courseRepo.GetByIdAsync(section.CourseId)
            ?? throw new CourseNotFoundException();

        // Step 3: Ownership check on the parent course.
        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        section.Update(request.Title, request.Order);
        await _sectionRepo.SaveChangesAsync();

        return MapToSectionResponse(section);
    }

    // ─── Delete Section ───────────────────────────────────────────────────────
    // Deletes a section and ALL its lessons (cascade delete in DB).
    // Same walk-up ownership check: Section → Course → InstructorId.
    public async Task DeleteSectionAsync(Guid instructorId, Guid sectionId)
    {
        var section = await _sectionRepo.GetByIdAsync(sectionId)
            ?? throw new DomainException("Section not found.");

        var course = await _courseRepo.GetByIdAsync(section.CourseId)
            ?? throw new CourseNotFoundException();

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        // Cascade: deleting section auto-deletes all its lessons in SQL.
        await _sectionRepo.DeleteAsync(section);
        await _sectionRepo.SaveChangesAsync();
    }

    // ─── Add Lesson ───────────────────────────────────────────────────────────
    // Adds a lesson to a section. Lessons are the actual content (video, text).
    // WALK UP TWO LEVELS: Lesson → Section → Course → check InstructorId.
    public async Task<LessonResponse> AddLessonAsync(
        Guid instructorId, Guid sectionId, AddLessonRequest request)
    {
        // Step 1: Find the parent section.
        var section = await _sectionRepo.GetByIdAsync(sectionId)
            ?? throw new DomainException("Section not found.");

        // Step 2: Find the grandparent course (via section.CourseId).
        var course = await _courseRepo.GetByIdAsync(section.CourseId)
            ?? throw new CourseNotFoundException();

        // Step 3: Ownership check on the grandparent course.
        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        // Step 4: Create lesson via factory — sets LessonId, CreatedAt.
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
    // Updates lesson content (title, video, text, duration, order, free preview).
    // WALKS UP TWO LEVELS: Lesson → Section → Course → InstructorId.
    public async Task<LessonResponse> UpdateLessonAsync(
        Guid instructorId, Guid lessonId, UpdateLessonRequest request)
    {
        // Step 1: Find the lesson.
        var lesson = await _lessonRepo.GetByIdAsync(lessonId)
            ?? throw new DomainException("Lesson not found.");

        // Step 2: Walk up to parent section.
        var section = await _sectionRepo.GetByIdAsync(lesson.SectionId)
            ?? throw new DomainException("Section not found.");

        // Step 3: Walk up to grandparent course.
        var course = await _courseRepo.GetByIdAsync(section.CourseId)
            ?? throw new CourseNotFoundException();

        // Step 4: Ownership check.
        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException();

        // Step 5: Update all mutable fields.
        lesson.Update(
            request.Title,
            request.VideoUrl,
            request.Content,
            request.DurationInSeconds,
            request.Order,
            request.IsFreePreview);

        // EF tracks changes automatically — just save.
        await _lessonRepo.SaveChangesAsync();

        return MapToLessonResponse(lesson);
    }

    // ─── Delete Lesson ────────────────────────────────────────────────────────
    // Deletes a single lesson. Same two-level walk-up ownership check.
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
    // Manual entity → DTO mapping. We use manual mapping instead of AutoMapper
    // because for a small number of DTOs, it's simpler, faster, and easier to debug.
    // Enums are converted to strings (.ToString()) for JSON-friendly API responses.

    // Lightweight response — used by GetAll and GetMyCourses (no nested data).
    private static CourseResponse MapToCourseResponse(Course course) => new()
    {
        CourseId = course.CourseId,
        Title = course.Title,
        Description = course.Description,
        ThumbnailUrl = course.ThumbnailUrl,
        Price = course.Price,
        Level = course.Level.ToString(),    // enum → string for JSON
        Status = course.Status.ToString(),  // enum → string for JSON
        Language = course.Language,
        InstructorName = course.InstructorName,
        InstructorId = course.InstructorId,
        CreatedAt = course.CreatedAt
    };

    // Full tree response — used by GetCourseById (detail view page).
    // Includes nested sections and lessons, all sorted by Order.
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
            .OrderBy(s => s.Order)          // display sections in correct order
            .Select(MapToSectionResponse)
            .ToList()
    };

    // Section with nested lessons, sorted by Order.
    private static SectionResponse MapToSectionResponse(Section section) => new()
    {
        SectionId = section.SectionId,
        Title = section.Title,
        Order = section.Order,
        Lessons = section.Lessons
            .OrderBy(l => l.Order)          // display lessons in correct order
            .Select(MapToLessonResponse)
            .ToList()
    };

    // Leaf-level mapping — single lesson (the smallest content unit).
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

    // ─── Category Management ──────────────────────────────────────────────────

    public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        return categories.Select(c => new CategoryResponse
        {
            CategoryId = c.CategoryId,
            Name = c.Name,
            Description = c.Description
        });
    }

    public async Task<CategoryResponse> CreateCategoryAsync(string name, string? description)
    {
        var category = new Category(name, description);
        await _categoryRepo.AddAsync(category);
        await _categoryRepo.SaveChangesAsync();

        return new CategoryResponse
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task UpdateCategoryAsync(Guid id, string name, string? description)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");

        category.Update(name, description);
        await _categoryRepo.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");

        _categoryRepo.Delete(category);
        await _categoryRepo.SaveChangesAsync();
    }
}