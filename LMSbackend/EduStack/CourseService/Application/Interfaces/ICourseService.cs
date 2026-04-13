// ICourseService — Contract for all course, section, and lesson operations.
// • Dependency Inversion: controller depends on this interface, not the concrete class.
// • InstructorId param on write methods enables ownership validation.

using CourseService.Application.DTOs.Requests;
using CourseService.Application.DTOs.Responses;

namespace CourseService.Application.Interfaces;

public interface ICourseService
{
    Task<IEnumerable<CourseResponse>> GetAllCoursesAsync();
    Task<IEnumerable<CourseResponse>> GetMyCourseAsync(Guid instructorId);
    Task<CourseDetailResponse> GetCourseByIdAsync(Guid courseId);
    Task<CourseResponse> CreateCourseAsync(Guid instructorId, string instructorName, CreateCourseRequest request);
    Task<CourseResponse> UpdateCourseAsync(Guid instructorId, Guid courseId, UpdateCourseRequest request);
    Task DeleteCourseAsync(Guid instructorId, Guid courseId);
    Task PublishCourseAsync(Guid instructorId, Guid courseId);
    Task UnpublishCourseAsync(Guid instructorId, Guid courseId);

    Task<SectionResponse> AddSectionAsync(Guid instructorId, Guid courseId, AddSectionRequest request);
    Task<SectionResponse> UpdateSectionAsync(Guid instructorId, Guid sectionId, UpdateSectionRequest request);
    Task DeleteSectionAsync(Guid instructorId, Guid sectionId);

    Task<LessonResponse> AddLessonAsync(Guid instructorId, Guid sectionId, AddLessonRequest request);
    Task<LessonResponse> UpdateLessonAsync(Guid instructorId, Guid lessonId, UpdateLessonRequest request);
    Task DeleteLessonAsync(Guid instructorId, Guid lessonId);
}