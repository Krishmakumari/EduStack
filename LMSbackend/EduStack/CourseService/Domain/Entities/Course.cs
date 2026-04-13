// Course Entity — Root of the Course → Section → Lesson hierarchy.
// • Uses DDD: private setters + factory method (Course.Create) to enforce valid state.
// • Created in Draft status by default; must have ≥1 section to publish.
// • InstructorName is denormalized to avoid cross-service calls to Auth Service.

using CourseService.Domain.Enums;
using CourseService.Domain.Exceptions;

namespace CourseService.Domain.Entities
{
    public class Course
    {
        public Guid CourseId { get; private set; }
        public string Title { get;private set; } = default!;
        public string Description { get;private set; }
        public string ThumbnailUrl { get;private set; }

        // Stored as decimal(18,2) in DB — supports prices like ₹499.99
        public decimal Price { get;private set; }

        // Stored as string in DB via HasConversion<string>() — "Beginner" not 0
        public CourseLevel Level { get; private set; }
        public CourseStatus Status { get;private set; }
        public string Language { get;private set; } = default!;

        // The instructor who created this course — used for ownership checks.
        // This GUID comes from the JWT "sub" claim (same ID from Auth Service).
        public Guid InstructorId { get;private set; }

        // Denormalized: copied from JWT at creation time to avoid calling Auth Service.
        // If Auth Service is down, course catalog still shows instructor names.
        public string InstructorName { get;private set; } = default!;

        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get;private set;  }

        // Navigation: One Course → Many Sections (cascade delete configured in EF)
        public ICollection<Section> Sections { get; private set; } = new List<Section>();

        // Private constructor forces creation through factory method below.
        private Course() { }

        /// <summary>
        /// Factory Method — creates a new course in Draft status.
        /// InstructorId and InstructorName come from the JWT claims.
        /// </summary>
        public static Course Create(string title,string description, string thumbnailUrl,
                                    decimal price,CourseLevel level,string language,
                                    Guid instructorId,string instructorName)
        {
            return new Course
            {
                CourseId = Guid.NewGuid(),
                Title = title,
                Description = description,
                ThumbnailUrl = thumbnailUrl,
                Price = price,
                Level = level,
                Language = language,
                InstructorId = instructorId,
                InstructorName = instructorName,
                Status = CourseStatus.Draft,    // always starts as Draft
                CreatedAt = DateTime.UtcNow
            };
        }

        // Updates mutable fields. UpdatedAt is set for audit trail.
        public void Update(string title, string description, string thumbnailUrl,
        decimal price, CourseLevel level, string language)
        {
            Title = title;
            Description = description;
            ThumbnailUrl = thumbnailUrl;
            Price = price;
            Level = level;
            Language = language;
            UpdatedAt = DateTime.UtcNow;
        }

        // ─── Status Lifecycle ───────────────────────────────────────────────
        // Draft ──(Publish)──► Published ──(Unpublish)──► Draft
        //                                       └──(Archive)──► Archived

        /// <summary>
        /// Publishes the course. Enforces business rule: must have ≥1 section.
        /// </summary>
        public void Publish()
        {
            if (!Sections.Any())
                throw new DomainException("Cannot publish a course with no sections.");
            Status = CourseStatus.Published;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Unpublish()
        {
            Status = CourseStatus.Draft;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Archive()
        {
            Status = CourseStatus.Archived;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
