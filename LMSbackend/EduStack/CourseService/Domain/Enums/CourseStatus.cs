// CourseStatus Enum — Lifecycle state: Draft → Published → Archived.
// • Draft = not visible to students. Published = enrollable. Archived = hidden but preserved.

namespace CourseService.Domain.Enums
{
    public enum CourseStatus
    {
        Draft,
        Published,
        Archived
    }
}
