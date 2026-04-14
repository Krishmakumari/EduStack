// AlreadyEnrolledException — Thrown when a student tries to enroll in a course they're already in.
// • Uses C# primary constructor syntax (inline base call) — compact exception definition.
// • Mapped to 409 Conflict in middleware — the request is valid but conflicts with existing state.

namespace EnrollmentService.Domain.Exceptions;

public class AlreadyEnrolledException()
    : DomainException("You are already enrolled in this course.");