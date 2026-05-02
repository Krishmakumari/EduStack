// CourseEntityTests — Unit tests for the Course domain entity.
// Tests factory method, Update, and status lifecycle (Submit → Approve/Reject → Unpublish).

using CourseService.Domain.Entities;
using CourseService.Domain.Enums;
using CourseService.Domain.Exceptions;

namespace NUnitTesting.CourseServiceTests;

[TestFixture]
public class CourseEntityTests
{
    private Course CreateTestCourse()
    {
        return Course.Create(
            "Test Course", "Description", "thumb.jpg", 499m,
            CourseLevel.Beginner, "English", Guid.NewGuid(), "Instructor");
    }

    [Test]
    public void Create_SetsDefaultValues()
    {
        var course = CreateTestCourse();

        Assert.That(course.CourseId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(course.Title, Is.EqualTo("Test Course"));
        Assert.That(course.Status, Is.EqualTo(CourseStatus.Draft));
        Assert.That(course.CreatedAt, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void Update_ChangesFieldsAndSetsUpdatedAt()
    {
        var course = CreateTestCourse();
        Assert.That(course.UpdatedAt, Is.Null);

        course.Update("Updated Title", "Updated Desc", "new.jpg", 999m, CourseLevel.Advanced, "Hindi");

        Assert.That(course.Title, Is.EqualTo("Updated Title"));
        Assert.That(course.Price, Is.EqualTo(999m));
        Assert.That(course.Level, Is.EqualTo(CourseLevel.Advanced));
        Assert.That(course.UpdatedAt, Is.Not.Null);
    }

    [Test]
    public void SubmitForReview_NoSections_ThrowsDomainException()
    {
        var course = CreateTestCourse();
        // course has no sections

        var ex = Assert.Throws<DomainException>(() => course.SubmitForReview());
        Assert.That(ex!.Message, Does.Contain("no sections"));
    }

    [Test]
    public void SubmitForReview_WithSections_ChangesStatusToPendingApproval()
    {
        var course = CreateTestCourse();
        // Add a section to the course's collection
        course.Sections.Add(Section.Create(course.CourseId, "Section 1", 1));

        course.SubmitForReview();

        Assert.That(course.Status, Is.EqualTo(CourseStatus.PendingApproval));
    }

    [Test]
    public void Approve_FromPendingApproval_ChangesStatusToPublished()
    {
        var course = CreateTestCourse();
        course.Sections.Add(Section.Create(course.CourseId, "Sec", 1));
        course.SubmitForReview();

        course.Approve();

        Assert.That(course.Status, Is.EqualTo(CourseStatus.Published));
    }

    [Test]
    public void Approve_FromDraft_ThrowsDomainException()
    {
        var course = CreateTestCourse(); // status = Draft

        Assert.Throws<DomainException>(() => course.Approve());
    }

    [Test]
    public void Reject_FromPendingApproval_ChangesStatusToRejected()
    {
        var course = CreateTestCourse();
        course.Sections.Add(Section.Create(course.CourseId, "Sec", 1));
        course.SubmitForReview();

        course.Reject();

        Assert.That(course.Status, Is.EqualTo(CourseStatus.Rejected));
    }

    [Test]
    public void Reject_FromDraft_ThrowsDomainException()
    {
        var course = CreateTestCourse(); // status = Draft

        Assert.Throws<DomainException>(() => course.Reject());
    }

    [Test]
    public void Unpublish_RevertsToDraft()
    {
        var course = CreateTestCourse();
        course.Sections.Add(Section.Create(course.CourseId, "Sec", 1));
        course.SubmitForReview();
        course.Approve();

        course.Unpublish();

        Assert.That(course.Status, Is.EqualTo(CourseStatus.Draft));
    }

    [Test]
    public void Archive_SetsStatusToArchived()
    {
        var course = CreateTestCourse();

        course.Archive();

        Assert.That(course.Status, Is.EqualTo(CourseStatus.Archived));
    }
}
