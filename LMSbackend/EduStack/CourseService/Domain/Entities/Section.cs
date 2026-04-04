namespace CourseService.Domain.Entities
{
    public class Section
    {
        public Guid SectionId { get; private set; }
        public Guid CourseId { get; private set; }
        public string Title { get; private set; } = default!;
        public int Order { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Course Course { get; private set; } = default!;
        public ICollection<Lesson> Lessons { get; private set; } = new List<Lesson>();

        private Section() { }

        public static Section Create(Guid courseId, string title, int order)
        {
            return new Section
            {
                SectionId = Guid.NewGuid(),
                CourseId = courseId,
                Title = title,
                Order = order,
                CreatedAt = DateTime.UtcNow
            };
        }
        public void Update(string title, int order)
        {
            Title = title;
            Order = order;
        }
    }
}
