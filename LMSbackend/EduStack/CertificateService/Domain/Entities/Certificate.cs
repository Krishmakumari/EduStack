// Certificate Entity — Records that a student earned a certificate for a course.
// • Public setters (no DDD factory) — simpler entity, created once, never modified.
// • UserName/CourseTitle are denormalized: captured at generation time to avoid
//   cross-service calls and to preserve the original name on the certificate.
// • FilePath stores the location of the generated PDF on disk.

namespace CertificateService.Domain.Entities;

public class Certificate
{
    public Guid CertificateId { get; set; }   // PK — unique certificate identifier

    public Guid UserId { get; set; }           // who earned it (from Auth Service)
    public Guid CourseId { get; set; }         // which course (from Course Service)

    public string UserName { get; set; }       // denormalized — display name at time of issue
    public string CourseTitle { get; set; }     // denormalized — course name at time of issue

    public DateTime IssuedAt { get; set; }     // when the certificate was generated (UTC)

    public string FilePath { get; set; }        // local path to the PDF file on disk
}