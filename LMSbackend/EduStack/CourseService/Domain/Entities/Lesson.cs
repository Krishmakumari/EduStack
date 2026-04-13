// Lesson Entity — The most granular content unit in a course.
// • Belongs to a Section (SectionId FK). One Section → Many Lessons.
// • VideoUrl/Content nullable: lessons can be video-only, text-only, or both.
// • IsFreePreview: marketing feature — lets students preview before enrolling.

namespace CourseService.Domain.Entities
{
    public class Lesson
    {
        public Guid LessonId { get; private set; }
        public Guid SectionId { get; private set; }     // FK → Section
        public string Title { get; private set; } = default!;
        public string? VideoUrl { get; private set; }    // nullable — not all lessons have video
        public string? Content { get; private set; }     // nullable — text/markdown content
        public int DurationInSeconds { get; private set; } // used by other services for progress tracking
        public int Order { get; private set; }           // display sequence within its section
        public bool IsFreePreview { get; private set; }  // if true, accessible without enrollment
        public DateTime CreatedAt { get; private set; }

        public Section Section { get; private set; } = default!;

        private Lesson() { }

        /// <summary>Factory — creates a lesson belonging to a section.</summary>
        public static Lesson Create(
            Guid sectionId,
            string title,
            string? videoUrl,
            string? content,
            int durationInSeconds,
            int order,
            bool isFreePreview = false)
        {
            return new Lesson
            {
                LessonId = Guid.NewGuid(),
                SectionId = sectionId,
                Title = title,
                VideoUrl = videoUrl,
                Content = content,
                DurationInSeconds = durationInSeconds,
                Order = order,
                IsFreePreview = isFreePreview,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Update(string title, string? videoUrl, string? content,
        int durationInSeconds, int order, bool isFreePreview)
        {
            Title = title;
            VideoUrl = videoUrl;
            Content = content;
            DurationInSeconds = durationInSeconds;
            Order = order;
            IsFreePreview = isFreePreview;
        }
    }
}
