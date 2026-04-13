# EduStack — Start All Services
$root = $PSScriptRoot

$services = @(
    "AuthService",
    "CourseService",
    "EnrollmentService",
    "LearningService",
    "PaymentService",
    "QuizService",
    "CertificateService",
    "NotificationService"
)

foreach ($svc in $services) {
    $path = Join-Path $root $svc
    if (Test-Path $path) {
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$path'; dotnet run" -WindowStyle Normal
        Write-Host "Started $svc"
    } else {
        Write-Host "Skipped $svc (folder not found)"
    }
}

# Start API Gateway last
$gatewayPath = Join-Path $root "ApiGateway"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$gatewayPath'; dotnet run" -WindowStyle Normal
Write-Host "Started ApiGateway"

Write-Host ""
Write-Host "All services started. Gateway is at http://localhost:5271"
