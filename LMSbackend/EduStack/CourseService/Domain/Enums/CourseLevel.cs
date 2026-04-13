// CourseLevel Enum — Difficulty categorization for course catalog filtering.
// • Stored as STRING in DB via HasConversion<string>() for readability.

namespace CourseService.Domain.Enums
{
    public enum CourseLevel
    {
        Beginner,
        Intermediate,
        Advanced
    }
}
