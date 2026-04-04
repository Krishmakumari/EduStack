using CourseService.Domain.Enums;
using CourseService.Domain.Exceptions;

namespace CourseService.Domain.Entities
{
    /// <summary>
    /// Business Rules:
    /// - A course is created in Draft status by default.
    /// - A course cannot be published unless it contains at least one section.
    /// - Updates modify course details and track the last updated timestamp.
    /// - Status transitions (Draft, Published, Archived) are controlled through explicit methods.
    ///
    /// This entity encapsulates domain logic to ensure consistency and enforce invariants.
    /// </summary>
    public class Course
    {
        public Guid CourseId { get; private set; }
        public string Title { get;private set; } = default!; //Initialize this property with null, but suppress null warnings because I will assign a real value later.
        public string Description { get;private set; }
        public string ThumbnailUrl { get;private set; }
        public decimal Price { get;private set; }
        public CourseLevel Level { get; private set; }
        public CourseStatus Status { get;private set; }
        public string Language { get;private set; } = default!;
        public Guid InstructorId { get;private set; }
        public string InstructorName { get;private set; } = default!;
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get;private set;  }

        public ICollection<Section> Sections { get; private set; } = new List<Section>();

        private Course() { }
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
                Status = CourseStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };
        }

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
