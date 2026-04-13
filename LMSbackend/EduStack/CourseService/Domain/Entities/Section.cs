// Section Entity — Groups lessons within a course.
// • Belongs to a Course (CourseId FK). One Course → Many Sections.
// • Order field controls display sequence without re-creating records.
// • Cascade delete: deleting a Section deletes all its Lessons.

namespace CourseService.Domain.Entities
{
    public class Section
    {
        public Guid SectionId { get; private set; }
        public Guid CourseId { get; private set; }    // FK → Course
        public string Title { get; private set; } = default!;
        public int Order { get; private set; }        // display sequence (1, 2, 3...)
        public DateTime CreatedAt { get; private set; }

        // Navigation properties
        public Course Course { get; private set; } = default!;
        public ICollection<Lesson> Lessons { get; private set; } = new List<Lesson>();

        private Section() { }

        /// <summary>Factory — creates a section belonging to a course.</summary>
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
