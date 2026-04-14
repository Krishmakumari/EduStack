// QuizResultEvent — RabbitMQ message contract for the quiz_queue.
// • Published by QuizService when a student's quiz is graded.
// • Passed field drives the email content: "passed 🎉" or "not passed ❌".
// • Email included directly (event-carried state transfer) — no Auth Service call needed.

namespace NotificationService.Domain.Events;

public class QuizResultEvent
{
    public Guid UserId { get; set; }   // student who took the quiz
    public string Email { get; set; }  // where to send the result
    public bool Passed { get; set; }   // true = pass email, false = fail email
}
