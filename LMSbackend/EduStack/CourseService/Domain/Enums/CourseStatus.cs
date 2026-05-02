// CourseStatus Enum — Lifecycle state: Draft → PendingApproval → Published → Rejected → Archived.
// • Draft = not visible to students. 
// • PendingApproval = instructor submitted for review.
// • Published = enrollable. 
// • Rejected = admin rejected the course.
// • Archived = hidden but preserved.

namespace CourseService.Domain.Enums
{
    public enum CourseStatus
    {
        Draft,
        PendingApproval,
        Published,
        Rejected,
        Archived
    }
}
