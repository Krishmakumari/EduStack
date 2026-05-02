namespace CourseService.Domain.Entities
{
    public class Category
    {
        public Guid CategoryId { get; private set; }
        public string Name { get; private set; } = default!;
        public string? Description { get; private set; }

        private Category() { }

        public Category(string name, string? description = null)
        {
            CategoryId = Guid.NewGuid();
            Name = name;
            Description = description;
        }

        public void Update(string name, string? description = null)
        {
            Name = name;
            Description = description;
        }
    }
}
