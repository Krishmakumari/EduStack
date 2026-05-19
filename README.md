# EduLearn LMS — Microservices Learning Management System

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Angular](https://img.shields.io/badge/Angular-17-DD0031?logo=angular)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Event%20Bus-FF6600?logo=rabbitmq)
![JWT](https://img.shields.io/badge/Auth-JWT%20Bearer-000000?logo=jsonwebtokens)

A fully individual case study implementing a **production-grade Learning Management System** using **.NET 8 Microservices + Angular 17**, following Clean
Architecture, Domain-Driven Design, and all backend/frontend best practices.

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Services](#services)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [API Endpoints Summary](#api-endpoints-summary)
- [Design Patterns Used](#design-patterns-used)
- [Security](#security)
- [Messaging — RabbitMQ](#messaging--rabbitmq)
- [Frontend — Angular 17](#frontend--angular-17)
- [Testing](#testing)
- [Best Practices Implemented](#best-practices-implemented)
- [Good-to-Have Features](#good-to-have-features)
- [Author](#author)

---

## Architecture Overview

EduLearn follows a **microservices architecture** with a single API Gateway entry point. Each service owns its own isolated SQL Server database 
(Database-per-Service pattern), communicates synchronously via HTTP for real-time checks, and asynchronously via **RabbitMQ** for event-driven notifications.

Angular Frontend (localhost:4200)
↓ JWT Bearer Token
API Gateway — Ocelot (localhost:5271)
JWT edge validation | CORS | Swagger aggregation
↓ Route rewrite to downstream service
┌──────────────────────────────────────────────────────────────┐
│  Auth:5124  Course:5248  Enroll:5080  Payment:5090           │
│  Learning:5240  Quiz:5160  Cert:5030  Notif:5300             │
└──────────────────────────────────────────────────────────────┘
↓ Each service owns its own SQL Server database
RabbitMQ Event Bus
(enrollment_queue · certificate_queue · quiz_queue)


---

## Services

| # | Service | Port | Responsibility |
|---|---------|------|----------------|
| 1 | **API Gateway** | 5271 | Ocelot reverse proxy, JWT edge auth, CORS, Swagger aggregation |
| 2 | **Auth Service** | 5124 | Register, Login, JWT + Refresh Token rotation, OTP password reset |
| 3 | **Course Service** | 5248 | Course/Lesson CRUD, publish lifecycle, DDD factory, repository pattern |
| 4 | **Enrollment Service** | 5080 | Enroll students, lesson progress, 2-layer duplicate prevention |
| 5 | **Payment Service** | 5090 | 4-state payment lifecycle, DDD guard methods, refund entity |
| 6 | **Learning Service** | 5240 | 80% watch rule, cross-service enrollment check, upsert progress |
| 7 | **Quiz Service** | 5160 | Create/grade quizzes, idempotent submissions, no-repo pattern |
| 8 | **Certificate Service** | 5030 | QuestPDF generation, filesystem storage, denormalized entity |
| 9 | **Notification Service** | 5300 | RabbitMQ consumer (IHostedService), MailKit SMTP, 2 trigger paths |
| 10 | **Angular Frontend** | 4200 | Standalone components, lazy loading, JWT auth, 7 feature services |

---

## Tech Stack

### Backend

| Technology | Usage |
|------------|-------|
| **C# 12 / .NET 8** | Core language — Lambda, LINQ, DateTime, async/await |
| **ASP.NET Core Web API** | RESTful controller-based endpoints |
| **Entity Framework Core** | ORM with Repository + Unit of Work pattern |
| **SQL Server** | Relational database per service |
| **Ocelot** | API Gateway / reverse proxy |
| **RabbitMQ** | Async event bus (IHostedService consumer) |
| **JWT + BCrypt** | Stateless auth + secure password hashing |
| **QuestPDF** | Code-first PDF certificate generation |
| **MailKit** | SMTP email via Gmail port 587 |
| **Swagger / Swashbuckle** | API documentation with MMLib aggregation |
| **NUnit** | Unit and integration testing |

### Frontend

| Technology | Usage |
|------------|-------|
| **Angular 17** | Standalone components, no NgModules |
| **Angular Reactive Forms** | Form validation |
| **RxJS** | tap(), subscribe(), HTTP observables |
| **Angular Guards** | Role-based route protection |
| **Lazy Loading** | Feature-module bundle splitting |

---

## Project Structure

EduLearnLMS/
├── ApiGateway/
│   └── ocelot.json
├── Services/
│   ├── AuthService/
│   │   ├── Domain/              # Entities: User, RefreshToken, PasswordReset
│   │   ├── Application/         # Interfaces, DTOs, Services
│   │   ├── Infrastructure/      # EF Core DbContext, Repositories
│   │   └── API/                 # Controllers, Middleware, Program.cs
│   ├── CourseService/
│   ├── EnrollmentService/
│   ├── PaymentService/
│   ├── LearningService/
│   ├── QuizService/
│   ├── CertificateService/
│   └── NotificationService/
└── Frontend/
└── edulearn-angular/
└── src/app/
├── core/            # Navbar, Footer (singleton)
├── features/
│   ├── auth/        # Login, Register, ForgotPassword, ResetPassword
│   ├── courses/     # CourseList, CourseDetail, CourseForm, MyCourses
│   ├── student/     # MyLearning, Player, Checkout, Payments
│   └── admin/       # AdminDashboard (tabbed)
├── services/        # AuthService, CourseService, EnrollmentService...
└── guards/          # admin.guard.ts, instructor.guard.ts, student.guard.ts

Each service follows **Clean Architecture (4 layers)**:
Domain → Application → Infrastructure → API
(dependencies always point inward — domain has zero framework dependencies)

## API Endpoints Summary

### Auth Service — `/gateway/auth`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/register` | Public | Create user, BCrypt hash password |
| POST | `/login` | Public | Verify credentials, return JWT + refresh token |
| POST | `/refresh-token` | Public | Rotate refresh token, issue new JWT |
| POST | `/forgot-password` | Public | Generate 6-digit OTP with 15-min expiry |
| POST | `/reset-password` | Public | Validate OTP, set new BCrypt hashed password |
| GET | `/me` | Bearer JWT | Return user from JWT claims — zero DB calls |

### Course Service — `/gateway/courses`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/` | Instructor | Create course (Draft state) |
| PUT | `/{id}` | Instructor | Update course metadata |
| DELETE | `/{id}` | Instructor | Delete course and all lessons |
| POST | `/{id}/publish` | Instructor | Transition Draft → Published |
| POST | `/{id}/unpublish` | Instructor | Transition Published → Draft |
| GET | `/` | Public | List all published courses |
| GET | `/search` | Public | Search by title/category |
| GET | `/instructor/my` | Instructor | Instructor's own courses |
| POST | `/{id}/lessons` | Instructor | Add lesson to course |
| PUT | `/{id}/lessons/{lid}` | Instructor | Update lesson |
| DELETE | `/{id}/lessons/{lid}` | Instructor | Remove lesson |

### Enrollment Service — `/gateway/enrollments`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/` | Student | Enroll in a course |
| GET | `/my` | Student | List all my enrollments |
| GET | `/{id}` | Student | Enrollment detail with lesson progress |
| POST | `/{id}/complete-lesson` | Student | Mark lesson as completed (upsert) |
| GET | `/{id}/progress` | Student | Progress stats: total/completed/% |
| GET | `/check` | Internal | IsUserEnrolled check (called by Learning Svc) |

### Payment Service — `/gateway/payments`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/initiate` | Student | Create payment (Pending state) |
| POST | `/{id}/confirm` | Student | Set Completed + transactionId |
| POST | `/{id}/fail` | Student | Set Failed + failureReason |
| POST | `/{id}/refund` | Student | Set Refunded — guard: only from Completed |
| GET | `/my` | Student | All payments for current student |
| GET | `/admin/payments/course/{id}` | Admin/Instructor | All payments for a course |

### Learning Service — `/gateway/learning`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/progress` | Bearer JWT | Upsert watch progress (triggers 80% rule) |
| GET | `/courses/{cId}/lessons/{lId}/progress` | Bearer JWT | Get single lesson progress |
| GET | `/courses/{cId}/progress` | Bearer JWT | Get overall course progress |

### Quiz Service — `/gateway/quiz`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/` | Instructor/Admin | Create quiz with CourseId and PassScore |
| POST | `/{id}/questions` | Instructor/Admin | Add MC or T/F question |
| POST | `/{id}/start` | Student | Create QuizAttempt (Status = InProgress) |
| POST | `/attempt/{id}/submit` | Student | Grade attempt, save answers, return score |

### Certificate Service — `/gateway/certificate`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/generate` | Bearer JWT | Generate PDF, save to disk + DB |
| GET | `/download/{id}` | Public | Stream PDF bytes as application/pdf |

---

## Design Patterns Used

| Pattern | Services | Purpose |
|---------|----------|---------|
| **Clean Architecture (4 layers)** | All | Domain → App → Infra → API; inward dependencies only |
| **Repository Pattern** | Course, Enrollment, Payment, Learning | Abstracts DB from application logic; enables unit testing |
| **DDD Factory Methods** | User, Enrollment, Payment | Guarantees valid initial state — no half-baked entities |
| **DDD Domain Methods** | Enrollment, Payment | Business rules live on entity: `MarkCompleted()`, `MarkRefunded()` |
| **Database-per-Service** | All | Each service has its own isolated DB schema |
| **Event-Carried State Transfer** | RabbitMQ events | Events carry email/name so consumers need no extra calls |
| **Typed HttpClient** | Learning Service | `IHttpClientFactory` manages TCP connection pooling |
| **Ocelot Reverse Proxy** | API Gateway | Single entry point; routes rewritten from `/gateway/*` to `/api/*` |
| **Upsert Pattern** | Learning Service | Create-or-update lesson progress in one operation |
| **Two-Layer Duplicate Prevention** | Enrollment Service | App-level check + DB UNIQUE INDEX catches race conditions |
| **Private Setters** | Course, Enrollment, Payment | External code cannot set state directly; all changes via domain methods |
| **Idempotent Submission Guard** | Quiz Service | Double-clicking Submit fails safely on second request |

---

## Security

### JWT Security Model

| Property | Value |
|----------|-------|
| Algorithm | HMAC-SHA256 (HS256) |
| Access token expiry | 60 minutes |
| ClockSkew | Zero — expired is immediately expired, no grace period |
| Claims | UserId, Email, Role, Issuer, Audience |
| Validation | Issuer + Audience + Lifetime + SigningKey (all 4 checked) |
| Refresh token | 7-day opaque random string, rotated on every use |
| Defense in depth | Validated at Gateway (fail-fast) AND at each downstream service |

### Additional Security Measures

- **BCrypt with automatic salting** — unique salt per password, embedded in hash, intentionally slow
- **Email enumeration prevention** — wrong email and wrong password return the identical error message
- **Admin ban** — `IsActive = false` blocks login even with correct password
- **OTP password reset** — 6-digit code, 15-minute expiry, `IsUsed` flag prevents replay
- **Ownership checks** — service layer verifies `StudentId` before returning any personal data
- **Refund guard** — `MarkRefunded()` on entity throws `DomainException` if status is not `Completed`

### Exception → HTTP Mapping (Global)

| Exception | HTTP | Trigger |
|-----------|------|---------|
| `DomainException` | 400 | Business rule violation |
| `NotFoundException` | 404 | Entity not found by ID |
| `UnauthorizedAccessException` | 403 | Student accessing another's data |
| `AlreadyEnrolledException` | 400 | Re-enrolling in same course |
| `PaymentAlreadyCompleted` | 400 | Double-confirming a payment |
| `QuizSubmissionException` | 400 | Submitting an already-graded attempt |
| Unhandled exceptions | 500 | Caught by `GlobalExceptionMiddleware` fallback |

---

## Messaging — RabbitMQ

The Notification Service is an `IHostedService` that runs for the full application lifetime, listening to 3 queues simultaneously.

### Queue → Email Mapping

| Queue | Event Type | Email Subject | Publisher |
|-------|------------|---------------|-----------|
| `enrollment_queue` | `EnrollmentCompletedEvent` | Enrollment Confirmed | Enrollment Service |
| `certificate_queue` | `CertificateGeneratedEvent` | Certificate Ready | Certificate Service |
| `quiz_queue` | `QuizResultEvent` | Quiz Result | Quiz Service |

### Cross-Service Data Flow

Student registers / logs in → receives JWT (UserId + Role)
Student browses published courses → gets CourseId
Student initiates payment → Payment: Pending → Completed
Student enrolls with CourseId → EnrollmentId returned
Enrollment Service → RabbitMQ: publishes EnrollmentCompletedEvent
Notification Service ← RabbitMQ: consumes event, sends confirmation email
Student watches lesson → Learning Service calls Enrollment Service to verify
Learning Service upserts LessonProgress → IsCompleted = true when >= 80% watched
Student starts quiz attempt → submits answers → receives Score + Passed/Failed
Student requests certificate → QuestPDF generates PDF → saved to disk + DB


---

## Frontend — Angular 17

### Architecture Decisions

| Decision | Detail |
|----------|--------|
| **Standalone components** | No NgModules; each component declares its own `imports[]` |
| **Lazy loading** | `loadComponent()` — student bundle never loads for admin session |
| **Manual JWT injection** | Each service has private `authHeaders()` method returning `HttpHeaders` with Bearer token |
| **Functional guards** | `CanActivateFn` pattern (Angular 17 best practice) |
| **`isPlatformBrowser()` guard** | All `localStorage` calls wrapped for SSR compatibility |

### Route Guards

| Guard | Checks | Redirect on Fail |
|-------|--------|-----------------|
| `studentGuard` | `role == 'Student'` | `/` (home) |
| `instructorGuard` | `role == 'Instructor'` OR `'Admin'` | `/admin/dashboard` (if Admin) |
| `adminGuard` | `role == 'Admin'` | `/` (home) |


