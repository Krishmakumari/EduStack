namespace CourseService.Domain.Entities
{
    public class Lesson
    {
        public Guid LessonId { get; private set; }
        public Guid SectionId { get; private set; }
        public string Title { get; private set; } = default!;
        public string? VideoUrl { get; private set; }
        public string? Content { get; private set; }
        public int DurationInSeconds { get; private set; }
        public int Order { get; private set; }
        public bool IsFreePreview { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Section Section { get; private set; } = default!;

        private Lesson() { }

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
