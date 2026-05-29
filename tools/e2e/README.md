# AutoTallerManager E2E Runner (Phase 37)

## 1. What this is
This runner executes end-to-end HTTP tests against the local AutoTallerManager API using native PowerShell only.

It validates:
- Public endpoints.
- JWT authentication and refresh/logout behavior.
- 401 unauthorized behavior without token.
- 403 forbidden behavior with wrong role.
- Role-based access for Admin, Receptionist, Mechanic and Client.
- Ownership checks for client-scoped resources.
- Full workshop flow (intake -> execution -> approvals -> invoice -> payment).
- Dashboards, reports, search, inventory and audit endpoints.
- Markdown and JSON report generation.

## 2. Prerequisites
- .NET SDK installed.
- API running locally (default: `http://localhost:5077`).
- Development bootstrap admin available:
  - Email: `admin.autotaller@test.com`
  - Password: `Admin123*`

## 3. Start the API
Example:
```powershell
dotnet run --project .\Api\Api.csproj
```

## 4. Build and run E2E
From repository root:
```powershell
dotnet build .\AutoTallerManager.slnx
.\tools\e2e\run-phase37-e2e.ps1 -BaseUrl "http://localhost:5077" -CreateTestData
```

Optional flags:
- `-SkipDestructiveTests` skips stock-adjustment write tests.
- `-AdminEmail` and `-AdminPassword` can override bootstrap credentials.

## 5. Report output
Reports are generated in:
- `tools/e2e/reports/e2e-report-YYYYMMDD-HHMMSS.md`
- `tools/e2e/reports/e2e-report-YYYYMMDD-HHMMSS.json`

## 6. Verdict meaning
- `PASS`: all critical tests passed and no blocking issues detected.
- `PARTIAL`: backend works partially; non-critical failures/skips/warnings found.
- `FAIL`: critical security/functional failures detected.

## 7. Data created by runner
With `-CreateTestData`, the script may create demo entities (client, staff, vehicle, service order, part, invoice, payment, etc.) prefixed for E2E traceability.

## 8. Security and sanitization
The reports sanitize sensitive values:
- Access token -> `***TOKEN_CAPTURED***`
- Refresh token -> `***REFRESH_TOKEN_CAPTURED***`
- Password/passwordHash -> `***REDACTED***`

## 9. Important
Do not run this script against production environments. It is intended for local/dev demo validation only.
