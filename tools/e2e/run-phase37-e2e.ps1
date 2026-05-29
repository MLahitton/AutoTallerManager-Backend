param(
    [string]$BaseUrl = "http://localhost:5077",
    [string]$AdminEmail = "[admin.autotaller@test.com](mailto:admin.autotaller@test.com)",
    [string]$AdminPassword = "Admin123*",
    [switch]$CreateTestData,
    [switch]$SkipDestructiveTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$script:RunStartedAt = Get-Date
$script:TestResults = New-Object System.Collections.ArrayList
$script:Warnings = New-Object System.Collections.ArrayList
$script:CriticalFailures = New-Object System.Collections.ArrayList
$script:MainFlowSteps = New-Object System.Collections.ArrayList
$script:TestCounter = 0
$script:BaseUrlNormalized = $BaseUrl.TrimEnd("/")
$script:MachineName = $env:COMPUTERNAME
$script:UserName = $env:USERNAME
$script:EnvironmentName = $env:ASPNETCORE_ENVIRONMENT
if ([string]::IsNullOrWhiteSpace($script:EnvironmentName)) {
    $script:EnvironmentName = "Unknown"
}

$script:GeneratedData = [ordered]@{
    clientUserId          = "N/A"
    clientPersonId        = "N/A"
    secondClientPersonId  = "N/A"
    receptionistUserId    = "N/A"
    receptionistPersonId  = "N/A"
    mechanicUserId        = "N/A"
    mechanicPersonId      = "N/A"
    vehicleBrandId        = "N/A"
    vehicleModelId        = "N/A"
    vehicleId             = "N/A"
    partBrandId           = "N/A"
    supplierId            = "N/A"
    partId                = "N/A"
    serviceOrderId        = "N/A"
    orderServiceId        = "N/A"
    orderServicePartId    = "N/A"
    invoiceId             = "N/A"
    paymentId             = "N/A"
}

function Write-TestLog {
    param(
        [string]$Message,
        [ValidateSet("INFO", "WARN", "ERROR", "PASS", "SKIP")]
        [string]$Level = "INFO"
    )

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff"
    Write-Host "[$timestamp][$Level] $Message"
}

function New-TestEmail {
    param([string]$Prefix = "e2e")
    $ticks = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
    $rnd = Get-Random -Minimum 100 -Maximum 999
    return "$Prefix.$ticks$rnd@test.com".ToLowerInvariant()
}

function New-TestDocumentNumber {
    param([string]$Prefix = "E2E")
    $ticks = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
    $rnd = Get-Random -Minimum 100 -Maximum 999
    return "$Prefix-$ticks-$rnd"
}

function New-TestVin {
    $seed = ("E2E" + [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds().ToString() + (Get-Random -Minimum 1000 -Maximum 9999).ToString()).ToUpperInvariant()
    $clean = ($seed -replace "[^A-Z0-9]", "")
    if ($clean.Length -lt 17) {
        $clean = $clean.PadRight(17, "X")
    }
    return $clean.Substring(0, 17)
}

function Get-PropertyValue {
    param(
        $Object,
        [string]$Name
    )

    if ($null -eq $Object) {
        return $null
    }

    $property = $Object.PSObject.Properties | Where-Object { $_.Name -ieq $Name } | Select-Object -First 1
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function Normalize-EmailInput {
    param([string]$Value)

    $normalized = ""
    if ($null -ne $Value) {
        $normalized = $Value
    }
    $normalized = $normalized.Trim()
    if ($normalized -match "mailto:([^)\]]+)") {
        return $matches[1].Trim().ToLowerInvariant()
    }

    if ($normalized -match "^\[([^\]]+)\]\(mailto:[^)]+\)$") {
        return $matches[1].Trim().ToLowerInvariant()
    }

    return $normalized.ToLowerInvariant()
}

function Sanitize-Response {
    param(
        $InputObject,
        [int]$MaxLength = 1400
    )

    if ($null -eq $InputObject) {
        return ""
    }

    $raw = ""

    if ($InputObject -is [string]) {
        $raw = $InputObject
    }
    else {
        try {
            $raw = $InputObject | ConvertTo-Json -Depth 20 -Compress
        }
        catch {
            $raw = [string]$InputObject
        }
    }

    if ([string]::IsNullOrWhiteSpace($raw)) {
        return ""
    }

    $raw = [regex]::Replace(
        $raw,
        '"(?<key>accessToken|refreshToken|password|passwordHash)"\s*:\s*"[^"]*"',
        [System.Text.RegularExpressions.MatchEvaluator]{
            param($m)
            $key = $m.Groups["key"].Value.ToLowerInvariant()
            switch ($key) {
                "accesstoken" { return '"accessToken":"***TOKEN_CAPTURED***"' }
                "refreshtoken" { return '"refreshToken":"***REFRESH_TOKEN_CAPTURED***"' }
                "password" { return '"password":"***REDACTED***"' }
                "passwordhash" { return '"passwordHash":"***REDACTED***"' }
                default { return $m.Value }
            }
        },
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    )

    $raw = [regex]::Replace(
        $raw,
        'Bearer\s+[A-Za-z0-9\-_\.=]+',
        'Bearer ***TOKEN_CAPTURED***',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    )

    if ($raw.Length -gt $MaxLength) {
        return $raw.Substring(0, $MaxLength) + "...(truncated)"
    }

    return $raw
}

function ConvertFrom-JsonSafe {
    param([string]$JsonText)

    if ([string]::IsNullOrWhiteSpace($JsonText)) {
        return $null
    }

    $convertCommand = Get-Command ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($null -ne $convertCommand -and $convertCommand.Parameters.ContainsKey("Depth")) {
        return ($JsonText | ConvertFrom-Json -Depth 20)
    }

    return ($JsonText | ConvertFrom-Json)
}

function Invoke-E2ERequest {
    param(
        [string]$Method,
        [string]$Url,
        [hashtable]$Headers = @{},
        $Body = $null,
        [int]$TimeoutSec = 120
    )

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $statusCode = 0
    $rawContent = ""
    $data = $null
    $errorMessage = ""

    try {
        $invokeParams = @{
            Uri         = $Url
            Method      = $Method
            TimeoutSec  = $TimeoutSec
            ErrorAction = "Stop"
        }

        $iwrCommand = Get-Command Invoke-WebRequest -ErrorAction SilentlyContinue
        if ($null -ne $iwrCommand -and $iwrCommand.Parameters.ContainsKey("UseBasicParsing")) {
            $invokeParams["UseBasicParsing"] = $true
        }

        if ($Headers -is [hashtable] -and $Headers.Count -gt 0) {
            $invokeParams["Headers"] = $Headers
        }

        if ($null -ne $Body) {
            if ($Body -is [string]) {
                $invokeParams["Body"] = $Body
            }
            else {
                $invokeParams["Body"] = $Body | ConvertTo-Json -Depth 20
            }
            $invokeParams["ContentType"] = "application/json"
        }

        $response = Invoke-WebRequest @invokeParams
        if ($null -ne $response) {
            if ($response.PSObject.Properties.Name -contains "StatusCode" -and $null -ne $response.StatusCode) {
                $statusCode = [int]$response.StatusCode
            }
            if ($response.PSObject.Properties.Name -contains "Content" -and $null -ne $response.Content) {
                $rawContent = [string]$response.Content
            }
        }
    }
    catch {
        $exception = $_.Exception
        if ($null -ne $exception -and -not [string]::IsNullOrWhiteSpace($exception.Message)) {
            $errorMessage = $exception.Message
        }
        else {
            $errorMessage = [string]$_
        }

        $response = $null

        try {
            if ($exception -is [System.Net.WebException] -and $null -ne $exception.Response) {
                $response = $exception.Response
            }
            elseif ($null -ne $exception -and $exception.PSObject.Properties.Name -contains "Response") {
                $response = $exception.Response
            }
        }
        catch {
            $response = $null
        }

        if ($null -ne $response) {
            try {
                if ($response -is [System.Net.HttpWebResponse]) {
                    $statusCode = [int]$response.StatusCode
                    $stream = $response.GetResponseStream()
                    if ($null -ne $stream) {
                        $reader = New-Object System.IO.StreamReader($stream)
                        try {
                            $rawContent = $reader.ReadToEnd()
                        }
                        finally {
                            $reader.Dispose()
                        }
                    }
                }
                else {
                    if ($response.PSObject.Properties.Name -contains "StatusCode" -and $null -ne $response.StatusCode) {
                        $statusCode = [int]$response.StatusCode
                    }

                    if ($response.PSObject.Properties.Name -contains "Content" -and $null -ne $response.Content) {
                        try {
                            $rawContent = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                        }
                        catch {
                            $rawContent = [string]$response.Content
                        }
                    }
                }
            }
            catch {
                if ([string]::IsNullOrWhiteSpace($errorMessage)) {
                    $errorMessage = $_.Exception.Message
                }
            }
        }
        else {
            if ($errorMessage -match "Unable to connect|actively refused|No connection could be made|timed out|Name or service not known|The remote name could not be resolved") {
                $errorMessage = "Unable to connect: $errorMessage"
            }
        }
    }
    finally {
        $stopwatch.Stop()
    }

    if (-not [string]::IsNullOrWhiteSpace($rawContent)) {
        try {
            $data = ConvertFrom-JsonSafe -JsonText $rawContent
        }
        catch {
            $data = $null
        }
    }

    return [PSCustomObject]@{
        StatusCode   = $statusCode
        RawContent   = $rawContent
        Data         = $data
        ErrorMessage = $errorMessage
        DurationMs   = [int]$stopwatch.Elapsed.TotalMilliseconds
    }
}

function Add-TestResult {
    param(
        [string]$TestName,
        [string]$Category,
        [string]$Method,
        [string]$Url,
        [string]$Role,
        $ExpectedStatusCode,
        [int]$ActualStatusCode,
        [bool]$Passed,
        [bool]$Skipped,
        [int]$DurationMs,
        [string]$RequestBodySummary,
        [string]$ResponseSnippet,
        [string]$ErrorMessage,
        [string]$Notes,
        [ValidateSet("Critical", "High", "Medium", "Low")]
        [string]$Criticality = "Medium"
    )

    $script:TestCounter++

    $expectedAsText = if ($ExpectedStatusCode -is [System.Collections.IEnumerable] -and -not ($ExpectedStatusCode -is [string])) {
        ($ExpectedStatusCode | ForEach-Object { [string]$_ }) -join ", "
    }
    else {
        [string]$ExpectedStatusCode
    }

    $result = [PSCustomObject]@{
        Id                 = $script:TestCounter
        TestName           = $TestName
        Category           = $Category
        Method             = $Method
        Url                = $Url
        Role               = $Role
        ExpectedStatusCode = $expectedAsText
        ActualStatusCode   = $ActualStatusCode
        Passed             = $Passed
        Skipped            = $Skipped
        DurationMs         = $DurationMs
        RequestBodySummary = $RequestBodySummary
        ResponseSnippet    = $ResponseSnippet
        ErrorMessage       = $ErrorMessage
        Notes              = $Notes
        Criticality        = $Criticality
    }

    $script:TestResults.Add($result) | Out-Null

    if (-not $Passed -and -not $Skipped -and $Criticality -eq "Critical") {
        $script:CriticalFailures.Add([PSCustomObject]@{
            Id       = $result.Id
            TestName = $result.TestName
            Category = $result.Category
            Method   = $result.Method
            Url      = $result.Url
            Expected = $result.ExpectedStatusCode
            Actual   = $result.ActualStatusCode
            Notes    = $result.Notes
        }) | Out-Null
    }
}

function Add-SkippedTest {
    param(
        [string]$TestName,
        [string]$Category,
        [string]$Method,
        [string]$Url,
        [string]$Role,
        $ExpectedStatusCode,
        [string]$Notes,
        [ValidateSet("Critical", "High", "Medium", "Low")]
        [string]$Criticality = "Low"
    )

    Add-TestResult -TestName $TestName `
        -Category $Category `
        -Method $Method `
        -Url $Url `
        -Role $Role `
        -ExpectedStatusCode $ExpectedStatusCode `
        -ActualStatusCode 0 `
        -Passed $false `
        -Skipped $true `
        -DurationMs 0 `
        -RequestBodySummary "" `
        -ResponseSnippet "" `
        -ErrorMessage "" `
        -Notes $Notes `
        -Criticality $Criticality

    Write-TestLog "$TestName skipped: $Notes" "SKIP"
}

function Assert-StatusCode {
    param(
        [int]$ActualStatusCode,
        $ExpectedStatusCode
    )

    if ($ExpectedStatusCode -is [int]) {
        return $ActualStatusCode -eq $ExpectedStatusCode
    }

    if ($ExpectedStatusCode -is [string]) {
        if ($ExpectedStatusCode -eq "Not200") {
            return $ActualStatusCode -ne 200
        }

        return [string]$ActualStatusCode -eq $ExpectedStatusCode
    }

    if ($ExpectedStatusCode -is [System.Collections.IEnumerable]) {
        foreach ($value in $ExpectedStatusCode) {
            if ($ActualStatusCode -eq [int]$value) {
                return $true
            }
        }

        return $false
    }

    return $false
}

function Invoke-TestCase {
    param(
        [string]$TestName,
        [string]$Category,
        [string]$Method,
        [string]$PathOrUrl,
        [string]$Role,
        $ExpectedStatusCode,
        [string]$Token = "",
        $Body = $null,
        [string]$Notes = "",
        [ValidateSet("Critical", "High", "Medium", "Low")]
        [string]$Criticality = "Medium",
        [switch]$ExpectNot200
    )

    $url = if ($PathOrUrl.StartsWith("http", [System.StringComparison]::OrdinalIgnoreCase)) {
        $PathOrUrl
    }
    else {
        "$script:BaseUrlNormalized$PathOrUrl"
    }

    $headers = @{}
    if (-not [string]::IsNullOrWhiteSpace($Token)) {
        $headers["Authorization"] = "Bearer $Token"
    }

    $requestBodySummary = if ($null -ne $Body) { Sanitize-Response -InputObject $Body -MaxLength 500 } else { "" }
    $response = Invoke-E2ERequest -Method $Method -Url $url -Headers $headers -Body $Body

    $passed = if ($ExpectNot200) {
        $response.StatusCode -ne 200
    }
    else {
        Assert-StatusCode -ActualStatusCode $response.StatusCode -ExpectedStatusCode $ExpectedStatusCode
    }

    $responseSnippet = if ($null -ne $response.Data) {
        Sanitize-Response -InputObject $response.Data
    }
    else {
        Sanitize-Response -InputObject $response.RawContent
    }

    Add-TestResult -TestName $TestName `
        -Category $Category `
        -Method $Method `
        -Url $url `
        -Role $Role `
        -ExpectedStatusCode $(if ($ExpectNot200) { "Not 200" } else { $ExpectedStatusCode }) `
        -ActualStatusCode $response.StatusCode `
        -Passed $passed `
        -Skipped $false `
        -DurationMs $response.DurationMs `
        -RequestBodySummary $requestBodySummary `
        -ResponseSnippet $responseSnippet `
        -ErrorMessage $response.ErrorMessage `
        -Notes $Notes `
        -Criticality $Criticality

    if ($passed) {
        Write-TestLog "$Category | $TestName => PASS ($($response.StatusCode))" "PASS"
    }
    else {
        Write-TestLog "$Category | $TestName => FAIL (Expected: $ExpectedStatusCode, Actual: $($response.StatusCode))" "ERROR"
    }

    return [PSCustomObject]@{
        StatusCode   = $response.StatusCode
        Data         = $response.Data
        RawContent   = $response.RawContent
        ErrorMessage = $response.ErrorMessage
        DurationMs   = $response.DurationMs
        Passed       = $passed
        Url          = $url
    }
}

function Get-AuthToken {
    param(
        [string]$Email,
        [string]$Password,
        [string]$Role = "Anonymous",
        [string]$Category = "Authentication",
        [string]$TestName = "Login",
        [ValidateSet("Critical", "High", "Medium", "Low")]
        [string]$Criticality = "High"
    )

    $response = Invoke-TestCase -TestName $TestName `
        -Category $Category `
        -Method "POST" `
        -PathOrUrl "/api/auth/login" `
        -Role $Role `
        -ExpectedStatusCode 200 `
        -Body @{
            Email    = $Email
            Password = $Password
        } `
        -Criticality $Criticality `
        -Notes "Obtains JWT access/refresh tokens."

    if (-not $response.Passed -or $null -eq $response.Data) {
        return $null
    }

    $authData = $response.Data
    $accessToken = [string](Get-PropertyValue -Object $authData -Name "accessToken")
    $refreshToken = [string](Get-PropertyValue -Object $authData -Name "refreshToken")
    $userObject = Get-PropertyValue -Object $authData -Name "user"

    if ([string]::IsNullOrWhiteSpace($accessToken) -and -not [string]::IsNullOrWhiteSpace($response.RawContent)) {
        try {
            $fallbackData = ConvertFrom-JsonSafe -JsonText $response.RawContent
            $accessToken = [string](Get-PropertyValue -Object $fallbackData -Name "accessToken")
            $refreshToken = [string](Get-PropertyValue -Object $fallbackData -Name "refreshToken")
            $userObject = Get-PropertyValue -Object $fallbackData -Name "user"
        }
        catch {
            # Keep null tokens; caller handles failure.
        }
    }

    if ([string]::IsNullOrWhiteSpace($accessToken)) {
        return $null
    }

    return [PSCustomObject]@{
        AccessToken  = $accessToken
        RefreshToken = $refreshToken
        UserId       = Get-PropertyValue -Object $userObject -Name "userId"
        PersonId     = Get-PropertyValue -Object $userObject -Name "personId"
        Email        = Get-PropertyValue -Object $userObject -Name "email"
        Roles        = Get-PropertyValue -Object $userObject -Name "roles"
    }
}

function Get-FirstOrDefaultId {
    param(
        $Items,
        [string[]]$PreferredNames = @()
    )

    if ($null -eq $Items) {
        return $null
    }

    $itemsArray = @($Items)
    if ($itemsArray.Count -eq 0) {
        return $null
    }

    foreach ($preferredName in $PreferredNames) {
        $match = $itemsArray | Where-Object {
            $name = Get-PropertyValue -Object $_ -Name "name"
            $null -ne $name -and $name.ToString().Equals($preferredName, [System.StringComparison]::OrdinalIgnoreCase)
        } | Select-Object -First 1

        if ($null -ne $match) {
            $id = Get-PropertyValue -Object $match -Name "id"
            if ($null -ne $id) {
                return [int]$id
            }
        }
    }

    $first = $itemsArray | Select-Object -First 1
    $firstId = Get-PropertyValue -Object $first -Name "id"
    if ($null -ne $firstId) {
        return [int]$firstId
    }

    return $null
}

function Resolve-CatalogIds {
    param([string]$AdminToken)

    $resolved = [ordered]@{
        DocumentTypeId       = $null
        GenderId             = $null
        CountryId            = $null
        VehicleTypeId        = $null
        ServiceTypeId        = $null
        OrderStatusPendingId = $null
        InvoiceStatusDraftId = $null
        PaymentMethodCashId  = $null
        PaymentStatusCompId  = $null
        CardTypeId           = $null
        SpecialtyId          = $null
        PartCategoryId       = $null
        PartBrandId          = $null
    }

    $publicCatalogResponse = Invoke-TestCase -TestName "Public registration catalogs" `
        -Category "Catalogs" `
        -Method "GET" `
        -PathOrUrl "/api/catalogs/public-registration" `
        -Role "Anonymous" `
        -ExpectedStatusCode 200 `
        -Criticality "High" `
        -Notes "Resolves IDs used by registration flows."

    if ($publicCatalogResponse.Passed -and $null -ne $publicCatalogResponse.Data) {
        $resolved.DocumentTypeId = Get-FirstOrDefaultId -Items (Get-PropertyValue $publicCatalogResponse.Data "documentTypes") -PreferredNames @("CC", "Cedula de Ciudadania")
        $resolved.GenderId = Get-FirstOrDefaultId -Items (Get-PropertyValue $publicCatalogResponse.Data "genders") -PreferredNames @("PreferNotToSay", "Other", "Male", "Female")
        $resolved.CountryId = Get-FirstOrDefaultId -Items (Get-PropertyValue $publicCatalogResponse.Data "countries") -PreferredNames @("Colombia")
    }

    $workshopCatalogResponse = Invoke-TestCase -TestName "Workshop catalogs as Admin" `
        -Category "Catalogs" `
        -Method "GET" `
        -PathOrUrl "/api/catalogs/workshop" `
        -Role "Admin" `
        -Token $AdminToken `
        -ExpectedStatusCode 200 `
        -Criticality "High" `
        -Notes "Resolves IDs used by workshop, invoice, payment and inventory flows."

    if ($workshopCatalogResponse.Passed -and $null -ne $workshopCatalogResponse.Data) {
        $resolved.VehicleTypeId = Get-FirstOrDefaultId -Items (Get-PropertyValue $workshopCatalogResponse.Data "vehicleTypes") -PreferredNames @("Sedan", "SUV")
        $resolved.ServiceTypeId = Get-FirstOrDefaultId -Items (Get-PropertyValue $workshopCatalogResponse.Data "serviceTypes") -PreferredNames @("Diagnostics", "Mechanical Repair", "Preventive Maintenance")
        $resolved.OrderStatusPendingId = Get-FirstOrDefaultId -Items (Get-PropertyValue $workshopCatalogResponse.Data "orderStatuses") -PreferredNames @("Pending")
        $resolved.InvoiceStatusDraftId = Get-FirstOrDefaultId -Items (Get-PropertyValue $workshopCatalogResponse.Data "invoiceStatuses") -PreferredNames @("Draft")
        $resolved.PaymentMethodCashId = Get-FirstOrDefaultId -Items (Get-PropertyValue $workshopCatalogResponse.Data "paymentMethods") -PreferredNames @("Cash", "BankTransfer")
        $resolved.PaymentStatusCompId = Get-FirstOrDefaultId -Items (Get-PropertyValue $workshopCatalogResponse.Data "paymentStatuses") -PreferredNames @("Completed")
        $resolved.CardTypeId = Get-FirstOrDefaultId -Items (Get-PropertyValue $workshopCatalogResponse.Data "cardTypes") -PreferredNames @("Visa", "Mastercard")
        $resolved.SpecialtyId = Get-FirstOrDefaultId -Items (Get-PropertyValue $workshopCatalogResponse.Data "mechanicSpecialties") -PreferredNames @("Brakes", "Engine", "Electrical")
        $resolved.PartCategoryId = Get-FirstOrDefaultId -Items (Get-PropertyValue $workshopCatalogResponse.Data "partCategories") -PreferredNames @("Brakes", "Engine", "Filters")
        $resolved.PartBrandId = Get-FirstOrDefaultId -Items (Get-PropertyValue $workshopCatalogResponse.Data "partBrands") -PreferredNames @("E2E")
    }

    return $resolved
}

function Stop-OnMissingCriticalValue {
    param(
        [string]$Name,
        $Value
    )

    $isMissing = $false

    if ($null -eq $Value) {
        $isMissing = $true
    }
    elseif ($Value -is [string] -and [string]::IsNullOrWhiteSpace($Value)) {
        $isMissing = $true
    }
    elseif ($Value -is [int] -and $Value -le 0) {
        $isMissing = $true
    }
    elseif ($Value -is [long] -and $Value -le 0) {
        $isMissing = $true
    }

    if ($isMissing) {
        $message = "Critical value '$Name' is missing. Main E2E flow cannot continue."
        $script:Warnings.Add($message) | Out-Null
        throw [System.InvalidOperationException]::new($message)
    }
}

function Get-ItemCount {
    param($Items)

    if ($null -eq $Items) {
        return 0
    }

    try {
        return @($Items).Length
    }
    catch {
        return (@($Items) | Measure-Object).Count
    }
}

function Add-MainFlowStep {
    param(
        [int]$Step,
        [string]$Action,
        [string]$Endpoint,
        [string]$Role,
        [string]$Expected,
        [int]$Actual,
        [string]$Result,
        [string]$Notes
    )

    $script:MainFlowSteps.Add([PSCustomObject]@{
        Step     = $Step
        Action   = $Action
        Endpoint = $Endpoint
        Role     = $Role
        Expected = $Expected
        Actual   = $Actual
        Result   = $Result
        Notes    = $Notes
    }) | Out-Null
}

function Get-TestSummary {
    $all = @()
    foreach ($item in $script:TestResults) {
        $all += $item
    }
    $total = $all.Count
    $passed = (@($all | Where-Object { $_.Passed -and -not $_.Skipped })).Count
    $failed = (@($all | Where-Object { -not $_.Passed -and -not $_.Skipped })).Count
    $skipped = (@($all | Where-Object { $_.Skipped })).Count
    $warnings = $script:Warnings.Count
    $critical = $script:CriticalFailures.Count
    $durationMs = ($all | Measure-Object -Property DurationMs -Sum).Sum
    if ($null -eq $durationMs) { $durationMs = 0 }

    $verdict = "PASS"
    if ($critical -gt 0) {
        $verdict = "FAIL"
    }
    elseif ($failed -gt 0 -or $skipped -gt 0 -or $warnings -gt 0) {
        $verdict = "PARTIAL"
    }

    return [PSCustomObject]@{
        Total        = $total
        Passed       = $passed
        Failed       = $failed
        Skipped      = $skipped
        Warnings     = $warnings
        Critical     = $critical
        DurationMs   = [int]$durationMs
        DurationText = [TimeSpan]::FromMilliseconds([int]$durationMs).ToString()
        Verdict      = $verdict
    }
}

function Save-ReportJson {
    param(
        [string]$Path,
        $Metadata,
        $Summary,
        $CategorySummary,
        $Recommendations,
        [string]$FinalVerdict
    )

    $reportObject = [ordered]@{
        metadata         = $Metadata
        summary          = $Summary
        categorySummary  = $CategorySummary
        criticalFailures = @($script:CriticalFailures)
        warnings         = @($script:Warnings)
        generatedData    = $script:GeneratedData
        results          = @($script:TestResults)
        recommendations  = $Recommendations
        finalVerdict     = $FinalVerdict
    }

    $reportObject | ConvertTo-Json -Depth 20 | Out-File -FilePath $Path -Encoding UTF8
}

function Save-ReportMarkdown {
    param(
        [string]$Path,
        $Metadata,
        $Summary,
        $CategorySummary,
        $Recommendations,
        [string]$FinalVerdict
    )

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("# AutoTallerManager Backend - E2E Test Report")
    $lines.Add("")
    $lines.Add("## 1. Metadata")
    $lines.Add("")
    $lines.Add("- ExecutionDateUtc: $($Metadata.ExecutionDateUtc)")
    $lines.Add("- BaseUrl: $($Metadata.BaseUrl)")
    $lines.Add("- Environment: $($Metadata.Environment)")
    $lines.Add("- Machine: $($Metadata.Machine)")
    $lines.Add("- User: $($Metadata.User)")
    $lines.Add("- Branch: $($Metadata.Branch)")
    $lines.Add("- Commit: $($Metadata.Commit)")
    $lines.Add("- Total tests: $($Summary.Total)")
    $lines.Add("- Passed: $($Summary.Passed)")
    $lines.Add("- Failed: $($Summary.Failed)")
    $lines.Add("- Skipped: $($Summary.Skipped)")
    $lines.Add("- Warnings: $($Summary.Warnings)")
    $lines.Add("- Duration: $($Summary.DurationText)")
    $lines.Add("- Final result: **$FinalVerdict**")
    $lines.Add("")
    $lines.Add("## 2. Executive Summary")
    $lines.Add("")
    if ($FinalVerdict -eq "PASS") {
        $lines.Add("The suite validated public endpoints, authentication, RBAC, ownership checks, workshop flow, invoice and payment flow, dashboards, reports, search, inventory and audit queries. No critical gaps were detected.")
    }
    elseif ($FinalVerdict -eq "PARTIAL") {
        $lines.Add("The suite executed core functional and security scenarios, but some tests failed or were skipped. The backend is partially validated and requires targeted fixes before final delivery.")
    }
    else {
        $lines.Add("Critical failures were detected in functional/security paths. The backend is not ready for delivery until those issues are corrected and re-tested.")
    }
    $lines.Add("")
    $lines.Add("## 3. Test Summary by Category")
    $lines.Add("")
    $lines.Add("| Category | Total | Passed | Failed | Skipped | Warnings |")
    $lines.Add("|---|---:|---:|---:|---:|---:|")
    foreach ($item in $CategorySummary) {
        $lines.Add("| $($item.Category) | $($item.Total) | $($item.Passed) | $($item.Failed) | $($item.Skipped) | $($item.Warnings) |")
    }
    $lines.Add("")
    $lines.Add("## 4. Critical Failures")
    $lines.Add("")
    if ($script:CriticalFailures.Count -eq 0) {
        $lines.Add("No critical failures were found.")
    }
    else {
        foreach ($failure in $script:CriticalFailures) {
            $lines.Add("- Test #$($failure.Id) $($failure.TestName) | $($failure.Method) $($failure.Url) | Expected: $($failure.Expected) | Actual: $($failure.Actual) | Notes: $($failure.Notes)")
        }
    }
    $lines.Add("")
    $lines.Add("## 5. Full E2E Flow Result")
    $lines.Add("")
    $lines.Add("| Step | Action | Endpoint | Role | Expected | Actual | Result | Notes |")
    $lines.Add("|---:|---|---|---|---|---:|---|---|")
    foreach ($step in $script:MainFlowSteps | Sort-Object Step) {
        $lines.Add("| $($step.Step) | $($step.Action) | $($step.Endpoint) | $($step.Role) | $($step.Expected) | $($step.Actual) | $($step.Result) | $($step.Notes) |")
    }
    if ($script:MainFlowSteps.Count -eq 0) {
        $lines.Add("| - | Main flow was not executed | - | - | - | - | SKIPPED | Missing prerequisites |")
    }
    $lines.Add("")
    $lines.Add("## 6. Security Results")
    $lines.Add("")
    $lines.Add("### 6.1 Unauthorized 401 Tests")
    $lines.Add("")
    $lines.Add("| Id | TestName | Endpoint | Expected | Actual | Result |")
    $lines.Add("|---:|---|---|---|---:|---|")
    foreach ($row in $script:TestResults | Where-Object { $_.Category -eq "UnauthorizedAccess401" }) {
        $resultText = if ($row.Passed) { "PASS" } elseif ($row.Skipped) { "SKIPPED" } else { "FAIL" }
        $lines.Add("| $($row.Id) | $($row.TestName) | $($row.Url) | $($row.ExpectedStatusCode) | $($row.ActualStatusCode) | $resultText |")
    }
    $lines.Add("")
    $lines.Add("### 6.2 Forbidden 403 Tests")
    $lines.Add("")
    $lines.Add("| Id | TestName | Endpoint | Expected | Actual | Result |")
    $lines.Add("|---:|---|---|---|---:|---|")
    foreach ($row in $script:TestResults | Where-Object { $_.Category -eq "ForbiddenAccess403" }) {
        $resultText = if ($row.Passed) { "PASS" } elseif ($row.Skipped) { "SKIPPED" } else { "FAIL" }
        $lines.Add("| $($row.Id) | $($row.TestName) | $($row.Url) | $($row.ExpectedStatusCode) | $($row.ActualStatusCode) | $resultText |")
    }
    $lines.Add("")
    $lines.Add("### 6.3 Ownership Tests")
    $lines.Add("")
    $lines.Add("| Id | TestName | Endpoint | Expected | Actual | Result |")
    $lines.Add("|---:|---|---|---|---:|---|")
    foreach ($row in $script:TestResults | Where-Object { $_.Category -eq "OwnershipSecurity" }) {
        $resultText = if ($row.Passed) { "PASS" } elseif ($row.Skipped) { "SKIPPED" } else { "FAIL" }
        $lines.Add("| $($row.Id) | $($row.TestName) | $($row.Url) | $($row.ExpectedStatusCode) | $($row.ActualStatusCode) | $resultText |")
    }
    $lines.Add("")
    $lines.Add("## 7. Detailed Results")
    $lines.Add("")
    $lines.Add("| Id | TestName | Category | Method | Url | Role | Expected | Actual | Result | DurationMs |")
    $lines.Add("|---:|---|---|---|---|---|---|---:|---|---:|")
    foreach ($row in $script:TestResults) {
        $resultText = if ($row.Skipped) { "SKIPPED" } elseif ($row.Passed) { "PASS" } else { "FAIL" }
        $lines.Add("| $($row.Id) | $($row.TestName) | $($row.Category) | $($row.Method) | $($row.Url) | $($row.Role) | $($row.ExpectedStatusCode) | $($row.ActualStatusCode) | $resultText | $($row.DurationMs) |")
    }
    $lines.Add("")
    $lines.Add("## 8. Response Details")
    $lines.Add("")
    foreach ($row in $script:TestResults) {
        $resultText = if ($row.Skipped) { "SKIPPED" } elseif ($row.Passed) { "PASS" } else { "FAIL" }
        $lines.Add("### Test #$($row.Id) - $($row.TestName)")
        $lines.Add("")
        $lines.Add("- Request:")
        $lines.Add("  - Method: $($row.Method)")
        $lines.Add("  - URL: $($row.Url)")
        $lines.Add("  - Role: $($row.Role)")
        if (-not [string]::IsNullOrWhiteSpace($row.RequestBodySummary)) {
            $lines.Add("  - Body summary: $($row.RequestBodySummary)")
        }
        else {
            $lines.Add("  - Body summary: N/A")
        }
        $lines.Add("- Expected:")
        $lines.Add("  - Status: $($row.ExpectedStatusCode)")
        $lines.Add("- Actual:")
        $lines.Add("  - Status: $($row.ActualStatusCode)")
        $lines.Add("  - Response snippet: $($row.ResponseSnippet)")
        if (-not [string]::IsNullOrWhiteSpace($row.ErrorMessage)) {
            $lines.Add("  - Error: $($row.ErrorMessage)")
        }
        $lines.Add("- Result: **$resultText**")
        if (-not [string]::IsNullOrWhiteSpace($row.Notes)) {
            $lines.Add("- Notes: $($row.Notes)")
        }
        $lines.Add("")
    }

    $lines.Add("## 9. Generated Test Data")
    $lines.Add("")
    foreach ($key in $script:GeneratedData.Keys) {
        $lines.Add("- $($key): $($script:GeneratedData[$key])")
    }
    $lines.Add("")
    $lines.Add("## 10. Recommendations")
    $lines.Add("")
    foreach ($rec in $Recommendations) {
        $lines.Add("- $rec")
    }
    $lines.Add("")
    $lines.Add("## 11. Final Verdict")
    $lines.Add("")
    switch ($FinalVerdict) {
        "PASS" { $lines.Add("PASS: El backend pasó las pruebas funcionales y de seguridad ejecutadas.") }
        "PARTIAL" { $lines.Add("PARTIAL: El backend funciona parcialmente, pero hubo pruebas omitidas o fallos no críticos.") }
        "FAIL" { $lines.Add("FAIL: El backend tiene fallos críticos que deben corregirse antes de entrega.") }
    }

    $lines -join "`r`n" | Out-File -FilePath $Path -Encoding UTF8
}

Write-TestLog "Starting E2E runner for AutoTallerManager backend." "INFO"
Write-TestLog "BaseUrl: $script:BaseUrlNormalized" "INFO"

$reportsDir = Join-Path $PSScriptRoot "reports"
if (-not (Test-Path $reportsDir)) {
    New-Item -Path $reportsDir -ItemType Directory | Out-Null
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$markdownReportPath = Join-Path $reportsDir "e2e-report-$timestamp.md"
$jsonReportPath = Join-Path $reportsDir "e2e-report-$timestamp.json"

$branch = "N/A"
$commit = "N/A"
try {
    $branchValue = git rev-parse --abbrev-ref HEAD 2>$null
    if (-not [string]::IsNullOrWhiteSpace($branchValue)) {
        $branch = $branchValue.Trim()
    }
}
catch { }

try {
    $commitValue = git rev-parse --short HEAD 2>$null
    if (-not [string]::IsNullOrWhiteSpace($commitValue)) {
        $commit = $commitValue.Trim()
    }
}
catch { }

$adminEmailNormalized = Normalize-EmailInput -Value $AdminEmail

$adminAuth = $null
$clientAuth = $null
$receptionistAuth = $null
$mechanicAuth = $null
$secondClientAuth = $null
$catalogIds = $null

$flowAbortedReason = $null
$flowContext = [ordered]@{
    serviceOrderId     = $null
    orderServiceId     = $null
    orderServicePartId = $null
    invoiceId          = $null
    invoiceTotal       = $null
    paymentId          = $null
    partId             = $null
}

try {
    # =====================================
    # 1. Preflight
    # =====================================
    Invoke-TestCase -TestName "API health via public catalogs" -Category "Preflight" -Method "GET" -PathOrUrl "/api/catalogs/public-registration" -Role "Anonymous" -ExpectedStatusCode 200 -Criticality "Critical" -Notes "Validates API reachability."

    $adminAuth = Get-AuthToken -Email $adminEmailNormalized -Password $AdminPassword -Role "Anonymous" -Category "Preflight" -TestName "Admin bootstrap login" -Criticality "Critical"
    Stop-OnMissingCriticalValue -Name "adminAccessToken" -Value $(if ($null -ne $adminAuth) { $adminAuth.AccessToken } else { $null })

    $adminRoles = @()
    if ($null -ne $adminAuth -and $null -ne $adminAuth.Roles) {
        $adminRoles = @($adminAuth.Roles | ForEach-Object { [string]$_ })
    }

    $tokenHasAdminRole = (@($adminRoles | Where-Object { $_.Equals("Admin", [System.StringComparison]::OrdinalIgnoreCase) }).Count -gt 0)
    Add-TestResult -TestName "Admin token contains Admin role claim" `
        -Category "Preflight" `
        -Method "POST" `
        -Url "$script:BaseUrlNormalized/api/auth/login" `
        -Role "Anonymous" `
        -ExpectedStatusCode "Token roles includes Admin" `
        -ActualStatusCode $(if ($tokenHasAdminRole) { 200 } else { 400 }) `
        -Passed $tokenHasAdminRole `
        -Skipped $false `
        -DurationMs 0 `
        -RequestBodySummary '{"email":"***REDACTED***","password":"***REDACTED***"}' `
        -ResponseSnippet (Sanitize-Response -InputObject @{ roles = $adminRoles }) `
        -ErrorMessage "" `
        -Notes "Validates bootstrap admin login returns Admin role claim." `
        -Criticality "Critical"

    if (-not $tokenHasAdminRole) {
        Stop-OnMissingCriticalValue -Name "adminRoleClaim" -Value $null
    }

    Invoke-TestCase -TestName "Admin dashboard access" -Category "Preflight" -Method "GET" -PathOrUrl "/api/admin/dashboard" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200 -Criticality "Critical"
    Invoke-TestCase -TestName "Admin users list access" -Category "Preflight" -Method "GET" -PathOrUrl "/api/users" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200 -Criticality "Critical"
    Invoke-TestCase -TestName "Admin roles list access" -Category "Preflight" -Method "GET" -PathOrUrl "/api/roles" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200 -Criticality "Critical"

    # =====================================
    # 2. Catalog IDs resolution
    # =====================================
    $catalogIds = Resolve-CatalogIds -AdminToken $adminAuth.AccessToken

    # =====================================
    # 3. Public endpoints and authentication
    # =====================================
    $primaryClientEmail = New-TestEmail -Prefix "client.e2e"
    $primaryClientPassword = "Client123*"
    $primaryClientDocument = New-TestDocumentNumber -Prefix "CLI"

    $registerClientResponse = Invoke-TestCase -TestName "Register client (public)" `
        -Category "PublicEndpoints" `
        -Method "POST" `
        -PathOrUrl "/api/auth/register-client" `
        -Role "Anonymous" `
        -ExpectedStatusCode 200 `
        -Criticality "Critical" `
        -Body @{
            DocumentTypeId = $(if ($null -ne $catalogIds.DocumentTypeId) { $catalogIds.DocumentTypeId } else { 1 })
            DocumentNumber = $primaryClientDocument
            FirstName      = "E2E"
            MiddleName     = $null
            LastName       = "Client"
            SecondLastName = $null
            BirthDate      = $null
            GenderId       = $catalogIds.GenderId
            AddressId      = $null
            Email          = $primaryClientEmail
            PhoneCountryId = $null
            PhoneNumber    = $null
            Password       = $primaryClientPassword
        } `
        -Notes "Creates the main client for E2E flow."

    if ($registerClientResponse.Passed -and $null -ne $registerClientResponse.Data) {
        $clientAuth = [PSCustomObject]@{
            AccessToken  = Get-PropertyValue -Object $registerClientResponse.Data -Name "accessToken"
            RefreshToken = Get-PropertyValue -Object $registerClientResponse.Data -Name "refreshToken"
            UserId       = Get-PropertyValue -Object (Get-PropertyValue -Object $registerClientResponse.Data -Name "user") -Name "userId"
            PersonId     = Get-PropertyValue -Object (Get-PropertyValue -Object $registerClientResponse.Data -Name "user") -Name "personId"
            Email        = $primaryClientEmail
        }

        $script:GeneratedData.clientUserId = $(if ($null -ne $clientAuth.UserId) { [int]$clientAuth.UserId } else { "N/A" })
        $script:GeneratedData.clientPersonId = $(if ($null -ne $clientAuth.PersonId) { [int]$clientAuth.PersonId } else { "N/A" })
    }

    Stop-OnMissingCriticalValue -Name "clientAccessToken" -Value $(if ($null -ne $clientAuth) { $clientAuth.AccessToken } else { $null })
    Stop-OnMissingCriticalValue -Name "clientPersonId" -Value $(if ($null -ne $clientAuth) { [int]$clientAuth.PersonId } else { 0 })

    $clientLoginResponse = Get-AuthToken -Email $primaryClientEmail -Password $primaryClientPassword -Role "Anonymous" -Category "Authentication" -TestName "Client login after register" -Criticality "High"
    if ($null -ne $clientLoginResponse) {
        $clientAuth = $clientLoginResponse
    }

    $refreshTokenToUse = $(if ($null -ne $clientAuth) { $clientAuth.RefreshToken } else { $null })
    Stop-OnMissingCriticalValue -Name "clientRefreshToken" -Value $refreshTokenToUse

    Invoke-TestCase -TestName "Client refresh token" -Category "Authentication" -Method "POST" -PathOrUrl "/api/auth/refresh" -Role "Anonymous" -ExpectedStatusCode 200 -Body @{ RefreshToken = $refreshTokenToUse } -Notes "Refresh token should return new JWT."
    Invoke-TestCase -TestName "Client logout" -Category "Authentication" -Method "POST" -PathOrUrl "/api/auth/logout" -Role "Anonymous" -ExpectedStatusCode @(200, 204) -Body @{ RefreshToken = $refreshTokenToUse } -Notes "Logout should invalidate refresh token."
    Invoke-TestCase -TestName "Client login wrong password" -Category "Authentication" -Method "POST" -PathOrUrl "/api/auth/login" -Role "Anonymous" -ExpectedStatusCode @(400, 401) -Body @{ Email = $primaryClientEmail; Password = "WrongPassword123*" } -Notes "Invalid password must not return 200." -Criticality "High"

    $clientRelogin = Get-AuthToken -Email $primaryClientEmail -Password $primaryClientPassword -Role "Anonymous" -Category "Authentication" -TestName "Client login for protected calls" -Criticality "High"
    if ($null -ne $clientRelogin) {
        $clientAuth = $clientRelogin
    }

    # =====================================
    # 4. Unauthorized 401 tests
    # =====================================
    $unauthorizedTests = @(
        @{ Name = "Account me without token"; Path = "/api/account/me"; Method = "GET" },
        @{ Name = "Admin dashboard without token"; Path = "/api/admin/dashboard"; Method = "GET" },
        @{ Name = "Receptionist dashboard without token"; Path = "/api/receptionist/dashboard"; Method = "GET" },
        @{ Name = "Mechanic dashboard without token"; Path = "/api/mechanic/dashboard"; Method = "GET" },
        @{ Name = "Client dashboard without token"; Path = "/api/client/dashboard"; Method = "GET" },
        @{ Name = "Users without token"; Path = "/api/users"; Method = "GET" },
        @{ Name = "Roles without token"; Path = "/api/roles"; Method = "GET" },
        @{ Name = "Audits without token"; Path = "/api/audits"; Method = "GET" },
        @{ Name = "Sales report without token"; Path = "/api/admin/reports/sales"; Method = "GET" },
        @{ Name = "Inventory summary without token"; Path = "/api/inventory/summary"; Method = "GET" }
    )

    foreach ($test in $unauthorizedTests) {
        Invoke-TestCase -TestName $test.Name -Category "UnauthorizedAccess401" -Method $test.Method -PathOrUrl $test.Path -Role "Anonymous" -ExpectedStatusCode 401 -Criticality "Critical" -Notes "Protected endpoint must reject missing token."
    }

    # =====================================
    # 5. Forbidden 403 with wrong role (Client)
    # =====================================
    $forbiddenTests = @(
        @{ Name = "Client blocked on admin dashboard"; Path = "/api/admin/dashboard"; Method = "GET" },
        @{ Name = "Client blocked on receptionist dashboard"; Path = "/api/receptionist/dashboard"; Method = "GET" },
        @{ Name = "Client blocked on mechanic dashboard"; Path = "/api/mechanic/dashboard"; Method = "GET" },
        @{ Name = "Client blocked on users"; Path = "/api/users"; Method = "GET" },
        @{ Name = "Client blocked on roles"; Path = "/api/roles"; Method = "GET" },
        @{ Name = "Client blocked on audits"; Path = "/api/audits"; Method = "GET" },
        @{ Name = "Client blocked on sales report"; Path = "/api/admin/reports/sales"; Method = "GET" },
        @{ Name = "Client blocked on search clients"; Path = "/api/search/clients?term=admin"; Method = "GET" },
        @{ Name = "Client blocked on inventory summary"; Path = "/api/inventory/summary"; Method = "GET" },
        @{ Name = "Client blocked on staff register"; Path = "/api/staff/register"; Method = "POST"; Body = @{
                DocumentTypeId = 1
                DocumentNumber = (New-TestDocumentNumber -Prefix "FORB")
                FirstName      = "Blocked"
                LastName       = "Client"
                Email          = (New-TestEmail -Prefix "blocked.staff")
                Password       = "Blocked123*"
                RoleName       = "Receptionist"
            }
        }
    )

    foreach ($test in $forbiddenTests) {
        $testBody = $null
        if ($test.ContainsKey("Body")) {
            $testBody = $test["Body"]
        }

        Invoke-TestCase -TestName $test.Name -Category "ForbiddenAccess403" -Method $test.Method -PathOrUrl $test.Path -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 403 -Body $testBody -Criticality "Critical" -Notes "Role-based access must deny this call."
    }

    # =====================================
    # 6. Admin access checks
    # =====================================
    $adminTests = @(
        @{ Name = "Admin dashboard"; Path = "/api/admin/dashboard" },
        @{ Name = "Users list"; Path = "/api/users" },
        @{ Name = "Roles list"; Path = "/api/roles" },
        @{ Name = "Audits CRUD list"; Path = "/api/audits" },
        @{ Name = "Admin audits recent"; Path = "/api/admin/audits/recent" },
        @{ Name = "Sales report"; Path = "/api/admin/reports/sales" },
        @{ Name = "Inventory report"; Path = "/api/admin/reports/inventory" },
        @{ Name = "Mechanics report"; Path = "/api/admin/reports/mechanics" },
        @{ Name = "Service orders report"; Path = "/api/admin/reports/service-orders" },
        @{ Name = "Payments report"; Path = "/api/admin/reports/payments" },
        @{ Name = "Workshop catalogs"; Path = "/api/catalogs/workshop" }
    )

    foreach ($test in $adminTests) {
        Invoke-TestCase -TestName $test.Name -Category "AdminAccess" -Method "GET" -PathOrUrl $test.Path -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200 -Criticality "High"
    }

    # =====================================
    # 7. Staff registration (Admin)
    # =====================================
    $receptionistEmail = New-TestEmail -Prefix "receptionist.e2e"
    $mechanicEmail = New-TestEmail -Prefix "mechanic.e2e"
    $receptionistPassword = "Receptionist123*"
    $mechanicPassword = "Mechanic123*"
    $mechanicSpecialtyId = $(if ($null -ne $catalogIds.SpecialtyId) { $catalogIds.SpecialtyId } else { 1 })

    if ($CreateTestData) {
        $receptionistResponse = Invoke-TestCase -TestName "Register Receptionist (Admin)" `
            -Category "StaffRegistration" `
            -Method "POST" `
            -PathOrUrl "/api/staff/register" `
            -Role "Admin" `
            -Token $adminAuth.AccessToken `
            -ExpectedStatusCode 200 `
            -Criticality "Critical" `
            -Body @{
                DocumentTypeId = $(if ($null -ne $catalogIds.DocumentTypeId) { $catalogIds.DocumentTypeId } else { 1 })
                DocumentNumber = (New-TestDocumentNumber -Prefix "REC")
                FirstName      = "E2E"
                MiddleName     = $null
                LastName       = "Receptionist"
                SecondLastName = $null
                BirthDate      = $null
                GenderId       = $catalogIds.GenderId
                AddressId      = $null
                Email          = $receptionistEmail
                PhoneCountryId = $null
                PhoneNumber    = $null
                Password       = $receptionistPassword
                RoleName       = "Receptionist"
                SpecialtyIds   = $null
            }

        if ($receptionistResponse.Passed -and $null -ne $receptionistResponse.Data) {
            $script:GeneratedData.receptionistUserId = [int](Get-PropertyValue $receptionistResponse.Data "userId")
            $script:GeneratedData.receptionistPersonId = [int](Get-PropertyValue $receptionistResponse.Data "personId")
        }

        $mechanicResponse = Invoke-TestCase -TestName "Register Mechanic (Admin)" `
            -Category "StaffRegistration" `
            -Method "POST" `
            -PathOrUrl "/api/staff/register" `
            -Role "Admin" `
            -Token $adminAuth.AccessToken `
            -ExpectedStatusCode 200 `
            -Criticality "Critical" `
            -Body @{
                DocumentTypeId = $(if ($null -ne $catalogIds.DocumentTypeId) { $catalogIds.DocumentTypeId } else { 1 })
                DocumentNumber = (New-TestDocumentNumber -Prefix "MEC")
                FirstName      = "E2E"
                MiddleName     = $null
                LastName       = "Mechanic"
                SecondLastName = $null
                BirthDate      = $null
                GenderId       = $catalogIds.GenderId
                AddressId      = $null
                Email          = $mechanicEmail
                PhoneCountryId = $null
                PhoneNumber    = $null
                Password       = $mechanicPassword
                RoleName       = "Mechanic"
                SpecialtyIds   = @($mechanicSpecialtyId)
            }

        if ($mechanicResponse.Passed -and $null -ne $mechanicResponse.Data) {
            $script:GeneratedData.mechanicUserId = [int](Get-PropertyValue $mechanicResponse.Data "userId")
            $script:GeneratedData.mechanicPersonId = [int](Get-PropertyValue $mechanicResponse.Data "personId")
        }

        $receptionistAuth = Get-AuthToken -Email $receptionistEmail -Password $receptionistPassword -Role "Anonymous" -Category "StaffRegistration" -TestName "Receptionist login" -Criticality "Critical"
        $mechanicAuth = Get-AuthToken -Email $mechanicEmail -Password $mechanicPassword -Role "Anonymous" -Category "StaffRegistration" -TestName "Mechanic login" -Criticality "Critical"

        Stop-OnMissingCriticalValue -Name "receptionistAccessToken" -Value $(if ($null -ne $receptionistAuth) { $receptionistAuth.AccessToken } else { $null })
        Stop-OnMissingCriticalValue -Name "mechanicAccessToken" -Value $(if ($null -ne $mechanicAuth) { $mechanicAuth.AccessToken } else { $null })

        Invoke-TestCase -TestName "Receptionist dashboard with receptionist token" -Category "StaffRegistration" -Method "GET" -PathOrUrl "/api/receptionist/dashboard" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200 -Criticality "High"
        Invoke-TestCase -TestName "Mechanic dashboard with mechanic token" -Category "StaffRegistration" -Method "GET" -PathOrUrl "/api/mechanic/dashboard" -Role "Mechanic" -Token $mechanicAuth.AccessToken -ExpectedStatusCode 200 -Criticality "High"

        if ($script:GeneratedData.mechanicPersonId -ne "N/A") {
            Invoke-TestCase -TestName "Get mechanic specialties by personId" -Category "StaffRegistration" -Method "GET" -PathOrUrl "/api/mechanics/$($script:GeneratedData.mechanicPersonId)/specialties" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200 -Criticality "Medium"
        }
        else {
            Add-SkippedTest -TestName "Get mechanic specialties by personId" -Category "StaffRegistration" -Method "GET" -Url "/api/mechanics/{personId}/specialties" -Role "Admin" -ExpectedStatusCode 200 -Notes "Mechanic personId was not created."
        }
    }
    else {
        Add-SkippedTest -TestName "Register Receptionist (Admin)" -Category "StaffRegistration" -Method "POST" -Url "/api/staff/register" -Role "Admin" -ExpectedStatusCode 200 -Notes "Skipped because -CreateTestData was not provided."
        Add-SkippedTest -TestName "Register Mechanic (Admin)" -Category "StaffRegistration" -Method "POST" -Url "/api/staff/register" -Role "Admin" -ExpectedStatusCode 200 -Notes "Skipped because -CreateTestData was not provided."
    }

    # =====================================
    # 8. Receptionist access
    # =====================================
    if ($null -ne $receptionistAuth -and -not [string]::IsNullOrWhiteSpace($receptionistAuth.AccessToken)) {
        Invoke-TestCase -TestName "Receptionist dashboard" -Category "ReceptionistAccess" -Method "GET" -PathOrUrl "/api/receptionist/dashboard" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Receptionist workshop catalogs" -Category "ReceptionistAccess" -Method "GET" -PathOrUrl "/api/catalogs/workshop" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Receptionist search clients" -Category "ReceptionistAccess" -Method "GET" -PathOrUrl "/api/search/clients?term=e2e" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Receptionist search vehicles" -Category "ReceptionistAccess" -Method "GET" -PathOrUrl "/api/search/vehicles?term=e2e" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Receptionist search parts" -Category "ReceptionistAccess" -Method "GET" -PathOrUrl "/api/search/parts?term=e2e" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Receptionist inventory low-stock" -Category "ReceptionistAccess" -Method "GET" -PathOrUrl "/api/inventory/low-stock" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Receptionist inventory summary" -Category "ReceptionistAccess" -Method "GET" -PathOrUrl "/api/inventory/summary" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Receptionist blocked from adjust-stock" -Category "ReceptionistAccess" -Method "POST" -PathOrUrl "/api/inventory/adjust-stock" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 403 -Body @{ PartId = 1; AdjustmentQuantity = 1; Reason = "E2E unauthorized test" }
        Invoke-TestCase -TestName "Receptionist blocked from admin reports" -Category "ReceptionistAccess" -Method "GET" -PathOrUrl "/api/admin/reports/sales" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 403
        Invoke-TestCase -TestName "Receptionist blocked from users" -Category "ReceptionistAccess" -Method "GET" -PathOrUrl "/api/users" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 403
    }
    else {
        Add-SkippedTest -TestName "Receptionist access suite" -Category "ReceptionistAccess" -Method "GET" -Url "/api/receptionist/dashboard" -Role "Receptionist" -ExpectedStatusCode 200 -Notes "Receptionist token is not available."
    }

    # =====================================
    # 9. Mechanic access
    # =====================================
    if ($null -ne $mechanicAuth -and -not [string]::IsNullOrWhiteSpace($mechanicAuth.AccessToken)) {
        Invoke-TestCase -TestName "Mechanic dashboard" -Category "MechanicAccess" -Method "GET" -PathOrUrl "/api/mechanic/dashboard" -Role "Mechanic" -Token $mechanicAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Mechanic my-assigned-services" -Category "MechanicAccess" -Method "GET" -PathOrUrl "/api/mechanic/my-assigned-services" -Role "Mechanic" -Token $mechanicAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Mechanic my-active-orders" -Category "MechanicAccess" -Method "GET" -PathOrUrl "/api/mechanic/my-active-orders" -Role "Mechanic" -Token $mechanicAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Mechanic workshop catalogs" -Category "MechanicAccess" -Method "GET" -PathOrUrl "/api/catalogs/workshop" -Role "Mechanic" -Token $mechanicAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Mechanic search parts" -Category "MechanicAccess" -Method "GET" -PathOrUrl "/api/search/parts?term=e2e" -Role "Mechanic" -Token $mechanicAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Mechanic blocked from users" -Category "MechanicAccess" -Method "GET" -PathOrUrl "/api/users" -Role "Mechanic" -Token $mechanicAuth.AccessToken -ExpectedStatusCode 403
        Invoke-TestCase -TestName "Mechanic blocked from admin dashboard" -Category "MechanicAccess" -Method "GET" -PathOrUrl "/api/admin/dashboard" -Role "Mechanic" -Token $mechanicAuth.AccessToken -ExpectedStatusCode 403
        Invoke-TestCase -TestName "Mechanic blocked from receptionist dashboard" -Category "MechanicAccess" -Method "GET" -PathOrUrl "/api/receptionist/dashboard" -Role "Mechanic" -Token $mechanicAuth.AccessToken -ExpectedStatusCode 403
    }
    else {
        Add-SkippedTest -TestName "Mechanic access suite" -Category "MechanicAccess" -Method "GET" -Url "/api/mechanic/dashboard" -Role "Mechanic" -ExpectedStatusCode 200 -Notes "Mechanic token is not available."
    }

    # =====================================
    # 10. Client access
    # =====================================
    if ($null -ne $clientAuth -and -not [string]::IsNullOrWhiteSpace($clientAuth.AccessToken)) {
        Invoke-TestCase -TestName "Client dashboard" -Category "ClientAccess" -Method "GET" -PathOrUrl "/api/client/dashboard" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Client my-vehicles" -Category "ClientAccess" -Method "GET" -PathOrUrl "/api/client/my-vehicles" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Client my-service-orders" -Category "ClientAccess" -Method "GET" -PathOrUrl "/api/client/my-service-orders" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Client my-invoices" -Category "ClientAccess" -Method "GET" -PathOrUrl "/api/client/my-invoices" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Client pending approvals" -Category "ClientAccess" -Method "GET" -PathOrUrl "/api/client/pending-approvals" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Client account me" -Category "ClientAccess" -Method "GET" -PathOrUrl "/api/account/me" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Client blocked from users list" -Category "ClientAccess" -Method "GET" -PathOrUrl "/api/users" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 403
        Invoke-TestCase -TestName "Client blocked from roles list" -Category "ClientAccess" -Method "GET" -PathOrUrl "/api/roles" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 403
        Invoke-TestCase -TestName "Client blocked from inventory summary" -Category "ClientAccess" -Method "GET" -PathOrUrl "/api/inventory/summary" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 403
    }
    else {
        Add-SkippedTest -TestName "Client access suite" -Category "ClientAccess" -Method "GET" -Url "/api/client/dashboard" -Role "Client" -ExpectedStatusCode 200 -Notes "Client token is not available."
    }

    # =====================================
    # 11. Main workshop E2E flow
    # =====================================
    if ($CreateTestData) {
        Stop-OnMissingCriticalValue -Name "clientPersonId" -Value $(if ($null -ne $clientAuth) { [int]$clientAuth.PersonId } else { 0 })
        Stop-OnMissingCriticalValue -Name "receptionistAccessToken" -Value $(if ($null -ne $receptionistAuth) { $receptionistAuth.AccessToken } else { $null })
        Stop-OnMissingCriticalValue -Name "mechanicAccessToken" -Value $(if ($null -ne $mechanicAuth) { $mechanicAuth.AccessToken } else { $null })
        Stop-OnMissingCriticalValue -Name "mechanicPersonId" -Value $(if ($script:GeneratedData.mechanicPersonId -ne "N/A") { [int]$script:GeneratedData.mechanicPersonId } else { 0 })

        $step = 1

        # Step 2 equivalent in requested flow: create VehicleBrand
        $brandResponse = Invoke-TestCase -TestName "Create vehicle brand E2E" -Category "WorkshopFlow" -Method "POST" -PathOrUrl "/api/vehicle-brands" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode @(200, 201) -Body @{ BrandName = "E2E Brand $timestamp" } -Criticality "Critical"
        $brandId = if ($brandResponse.Passed) { [int](Get-PropertyValue $brandResponse.Data "brandId") } else { 0 }
        if ($brandId -gt 0) { $script:GeneratedData.vehicleBrandId = $brandId }
        Add-MainFlowStep -Step $step -Action "Create VehicleBrand" -Endpoint "/api/vehicle-brands" -Role "Admin" -Expected "200/201" -Actual $brandResponse.StatusCode -Result $(if ($brandResponse.Passed) { "PASS" } else { "FAIL" }) -Notes "brandId=$brandId"
        $step++
        Stop-OnMissingCriticalValue -Name "vehicleBrandId" -Value $brandId

        $modelResponse = Invoke-TestCase -TestName "Create vehicle model E2E" -Category "WorkshopFlow" -Method "POST" -PathOrUrl "/api/vehicle-models" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode @(200, 201) -Body @{ BrandId = $brandId; ModelName = "E2E Model $timestamp" } -Criticality "Critical"
        $modelId = if ($modelResponse.Passed) { [int](Get-PropertyValue $modelResponse.Data "modelId") } else { 0 }
        if ($modelId -gt 0) { $script:GeneratedData.vehicleModelId = $modelId }
        Add-MainFlowStep -Step $step -Action "Create VehicleModel" -Endpoint "/api/vehicle-models" -Role "Admin" -Expected "200/201" -Actual $modelResponse.StatusCode -Result $(if ($modelResponse.Passed) { "PASS" } else { "FAIL" }) -Notes "modelId=$modelId"
        $step++
        Stop-OnMissingCriticalValue -Name "vehicleModelId" -Value $modelId

        $partBrandResponse = Invoke-TestCase -TestName "Create part brand E2E" -Category "WorkshopFlow" -Method "POST" -PathOrUrl "/api/part-brands" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode @(200, 201) -Body @{ Name = "E2E PartBrand $timestamp" } -Criticality "High"
        $partBrandId = if ($partBrandResponse.Passed) { [int](Get-PropertyValue $partBrandResponse.Data "partBrandId") } else { 0 }
        if ($partBrandId -gt 0) { $script:GeneratedData.partBrandId = $partBrandId }
        Add-MainFlowStep -Step $step -Action "Create PartBrand" -Endpoint "/api/part-brands" -Role "Admin" -Expected "200/201" -Actual $partBrandResponse.StatusCode -Result $(if ($partBrandResponse.Passed) { "PASS" } else { "FAIL" }) -Notes "partBrandId=$partBrandId"
        $step++

        $supplierResponse = Invoke-TestCase -TestName "Create supplier E2E" -Category "WorkshopFlow" -Method "POST" -PathOrUrl "/api/suppliers" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode @(200, 201) -Body @{
            Name     = "E2E Supplier $timestamp"
            TaxId    = "E2E-TAX-$timestamp"
            Phone    = "3000000000"
            Email    = "supplier.$timestamp@test.com"
            IsActive = $true
        } -Criticality "High"
        $supplierId = if ($supplierResponse.Passed) { [int](Get-PropertyValue $supplierResponse.Data "supplierId") } else { 0 }
        if ($supplierId -gt 0) { $script:GeneratedData.supplierId = $supplierId }
        Add-MainFlowStep -Step $step -Action "Create Supplier" -Endpoint "/api/suppliers" -Role "Admin" -Expected "200/201" -Actual $supplierResponse.StatusCode -Result $(if ($supplierResponse.Passed) { "PASS" } else { "FAIL" }) -Notes "supplierId=$supplierId"
        $step++

        $partResponse = Invoke-TestCase -TestName "Create part with stock E2E" -Category "WorkshopFlow" -Method "POST" -PathOrUrl "/api/parts" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode @(200, 201) -Body @{
            PartCategoryId = $(if ($null -ne $catalogIds.PartCategoryId) { $catalogIds.PartCategoryId } else { 1 })
            PartBrandId    = $(if ($partBrandId -gt 0) { $partBrandId } else { $null })
            Code           = "E2E-PART-$timestamp"
            Description    = "E2E Brake Pad Test"
            Stock          = 20
            MinimumStock   = 5
            UnitPrice      = 100000
            IsActive       = $true
        } -Criticality "Critical"
        $partId = if ($partResponse.Passed) { [int](Get-PropertyValue $partResponse.Data "partId") } else { 0 }
        if ($partId -gt 0) { $script:GeneratedData.partId = $partId; $flowContext.partId = $partId }
        Add-MainFlowStep -Step $step -Action "Create Part" -Endpoint "/api/parts" -Role "Admin" -Expected "200/201" -Actual $partResponse.StatusCode -Result $(if ($partResponse.Passed) { "PASS" } else { "FAIL" }) -Notes "partId=$partId"
        $step++
        Stop-OnMissingCriticalValue -Name "partId" -Value $partId

        $vehicleTypeId = $(if ($null -ne $catalogIds.VehicleTypeId) { $catalogIds.VehicleTypeId } else { 1 })
        $vin = New-TestVin
        $clientPersonId = [int]$clientAuth.PersonId

        $addVehicleResponse = Invoke-TestCase -TestName "Add vehicle to client" -Category "WorkshopFlow" -Method "POST" -PathOrUrl "/api/clients/$clientPersonId/vehicles" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode @(200, 201) -Body @{
            ModelId       = $modelId
            VehicleTypeId = $vehicleTypeId
            VIN           = $vin
            Year          = [DateTime]::UtcNow.Year
            Color         = "E2E Blue"
            Mileage       = 1000
        } -Criticality "Critical"
        $vehicleId = if ($addVehicleResponse.Passed) { [int](Get-PropertyValue $addVehicleResponse.Data "vehicleId") } else { 0 }
        if ($vehicleId -gt 0) { $script:GeneratedData.vehicleId = $vehicleId }
        Add-MainFlowStep -Step $step -Action "Add vehicle to client" -Endpoint "/api/clients/{personId}/vehicles" -Role "Receptionist" -Expected "200/201" -Actual $addVehicleResponse.StatusCode -Result $(if ($addVehicleResponse.Passed) { "PASS" } else { "FAIL" }) -Notes "vehicleId=$vehicleId"
        $step++
        Stop-OnMissingCriticalValue -Name "vehicleId" -Value $vehicleId

        $workshopIntakeResponse = Invoke-TestCase -TestName "Create workshop service order intake" -Category "WorkshopFlow" -Method "POST" -PathOrUrl "/api/workshop-intake/create-service-order" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode @(200, 201) -Body @{
            VehicleId              = $vehicleId
            InitialOrderStatusId   = $null
            EntryDate              = $null
            EstimatedDeliveryDate  = (Get-Date).AddDays(2).ToString("o")
            GeneralDescription     = "E2E workshop intake flow"
            HasScratches           = $false
            ScratchesDescription   = $null
            HasToolbox             = $false
            ToolboxDescription     = $null
            OwnershipCardDelivered = $true
            InventoryObservations  = "E2E intake"
            Services               = @(
                @{
                    ServiceTypeId = $(if ($null -ne $catalogIds.ServiceTypeId) { $catalogIds.ServiceTypeId } else { 1 })
                    Description   = "E2E diagnostics and repair"
                    LaborCost     = 150000
                }
            )
        } -Criticality "Critical"

        $serviceOrderId = if ($workshopIntakeResponse.Passed) { [int](Get-PropertyValue $workshopIntakeResponse.Data "serviceOrderId") } else { 0 }
        $services = if ($workshopIntakeResponse.Passed) { @(Get-PropertyValue $workshopIntakeResponse.Data "services") } else { @() }
        $firstService = if ((Get-ItemCount -Items $services) -gt 0) { $services[0] } else { $null }
        $orderServiceId = if ($null -ne $firstService) { [int](Get-PropertyValue $firstService "orderServiceId") } else { 0 }
        if ($serviceOrderId -gt 0) { $script:GeneratedData.serviceOrderId = $serviceOrderId; $flowContext.serviceOrderId = $serviceOrderId }
        if ($orderServiceId -gt 0) { $script:GeneratedData.orderServiceId = $orderServiceId; $flowContext.orderServiceId = $orderServiceId }
        Add-MainFlowStep -Step $step -Action "Create service order" -Endpoint "/api/workshop-intake/create-service-order" -Role "Receptionist" -Expected "200/201" -Actual $workshopIntakeResponse.StatusCode -Result $(if ($workshopIntakeResponse.Passed) { "PASS" } else { "FAIL" }) -Notes "serviceOrderId=$serviceOrderId; orderServiceId=$orderServiceId"
        $step++
        Stop-OnMissingCriticalValue -Name "serviceOrderId" -Value $serviceOrderId
        Stop-OnMissingCriticalValue -Name "orderServiceId" -Value $orderServiceId

        $assignMechanicResponse = Invoke-TestCase -TestName "Assign mechanic to order service" -Category "WorkshopFlow" -Method "POST" -PathOrUrl "/api/order-services/$orderServiceId/assign-mechanic" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200 -Body @{
            MechanicPersonId = [int]$script:GeneratedData.mechanicPersonId
            SpecialtyId      = $mechanicSpecialtyId
        } -Criticality "Critical"
        $mechanicAssignmentId = if ($assignMechanicResponse.Passed -and $null -ne $assignMechanicResponse.Data) { [int](Get-PropertyValue $assignMechanicResponse.Data "id") } else { 0 }
        Add-MainFlowStep -Step $step -Action "Assign mechanic" -Endpoint "/api/order-services/{id}/assign-mechanic" -Role "Receptionist" -Expected "200" -Actual $assignMechanicResponse.StatusCode -Result $(if ($assignMechanicResponse.Passed) { "PASS" } else { "FAIL" }) -Notes "mechanicAssignmentId=$mechanicAssignmentId"
        $step++

        $mechanicWorkResponse = Invoke-TestCase -TestName "Mechanic updates work performed" -Category "MechanicWorkflow" -Method "PUT" -PathOrUrl "/api/mechanic/order-services/$orderServiceId/work-performed" -Role "Mechanic" -Token $mechanicAuth.AccessToken -ExpectedStatusCode 200 -Body @{
            WorkPerformed = "E2E mechanic work report completed."
            LaborCost     = 180000
        } -Criticality "Critical"
        Add-MainFlowStep -Step $step -Action "Mechanic report work" -Endpoint "/api/mechanic/order-services/{id}/work-performed" -Role "Mechanic" -Expected "200" -Actual $mechanicWorkResponse.StatusCode -Result $(if ($mechanicWorkResponse.Passed) { "PASS" } else { "FAIL" }) -Notes ""
        $step++

        $requestPartResponse = Invoke-TestCase -TestName "Mechanic requests part" -Category "MechanicWorkflow" -Method "POST" -PathOrUrl "/api/order-services/$orderServiceId/request-part" -Role "Mechanic" -Token $mechanicAuth.AccessToken -ExpectedStatusCode 200 -Body @{
            PartId           = $partId
            Quantity         = 1
            AppliedUnitPrice = $null
        } -Criticality "Critical"
        $orderServicePartId = if ($requestPartResponse.Passed -and $null -ne $requestPartResponse.Data) { [int](Get-PropertyValue $requestPartResponse.Data "id") } else { 0 }
        if ($orderServicePartId -gt 0) {
            $script:GeneratedData.orderServicePartId = $orderServicePartId
            $flowContext.orderServicePartId = $orderServicePartId
        }
        Add-MainFlowStep -Step $step -Action "Request part" -Endpoint "/api/order-services/{id}/request-part" -Role "Mechanic" -Expected "200" -Actual $requestPartResponse.StatusCode -Result $(if ($requestPartResponse.Passed) { "PASS" } else { "FAIL" }) -Notes "orderServicePartId=$orderServicePartId"
        $step++
        Stop-OnMissingCriticalValue -Name "orderServicePartId" -Value $orderServicePartId

        $pendingApprovalsResponse = Invoke-TestCase -TestName "Client reads pending approvals" -Category "ClientApprovals" -Method "GET" -PathOrUrl "/api/client/pending-approvals" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 200 -Criticality "Critical"
        $pendingIncludesService = $false
        $pendingIncludesPart = $false
        if ($pendingApprovalsResponse.Passed -and $null -ne $pendingApprovalsResponse.Data) {
            $pendingServices = @(Get-PropertyValue $pendingApprovalsResponse.Data "orderServices")
            $pendingParts = @(Get-PropertyValue $pendingApprovalsResponse.Data "orderServiceParts")
            $pendingServiceMatches = @($pendingServices | Where-Object { [int](Get-PropertyValue $_ "orderServiceId") -eq $orderServiceId })
            $pendingPartMatches = @($pendingParts | Where-Object { [int](Get-PropertyValue $_ "orderServicePartId") -eq $orderServicePartId })
            $pendingIncludesService = ((Get-ItemCount -Items $pendingServiceMatches) -gt 0)
            $pendingIncludesPart = ((Get-ItemCount -Items $pendingPartMatches) -gt 0)
        }

        Add-TestResult -TestName "Pending approvals include created service/part" `
            -Category "ClientApprovals" `
            -Method "GET" `
            -Url "$script:BaseUrlNormalized/api/client/pending-approvals" `
            -Role "Client" `
            -ExpectedStatusCode "Pending list should include orderServiceId and orderServicePartId" `
            -ActualStatusCode $pendingApprovalsResponse.StatusCode `
            -Passed ($pendingApprovalsResponse.Passed -and $pendingIncludesService -and $pendingIncludesPart) `
            -Skipped $false `
            -DurationMs $pendingApprovalsResponse.DurationMs `
            -RequestBodySummary "" `
            -ResponseSnippet (Sanitize-Response $pendingApprovalsResponse.Data) `
            -ErrorMessage "" `
            -Notes "serviceFound=$pendingIncludesService; partFound=$pendingIncludesPart" `
            -Criticality "Critical"

        Add-MainFlowStep -Step $step -Action "Client checks pending approvals" -Endpoint "/api/client/pending-approvals" -Role "Client" -Expected "200 + contains pending service/part" -Actual $pendingApprovalsResponse.StatusCode -Result $(if ($pendingApprovalsResponse.Passed -and $pendingIncludesService -and $pendingIncludesPart) { "PASS" } else { "FAIL" }) -Notes ""
        $step++

        $approveServiceResponse = Invoke-TestCase -TestName "Client approves order service" -Category "ClientApprovals" -Method "POST" -PathOrUrl "/api/client/approvals/order-services/$orderServiceId/approve" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 200 -Body @{ Observation = "Approved by E2E client" } -Criticality "Critical"
        Add-MainFlowStep -Step $step -Action "Client approve service" -Endpoint "/api/client/approvals/order-services/{id}/approve" -Role "Client" -Expected "200" -Actual $approveServiceResponse.StatusCode -Result $(if ($approveServiceResponse.Passed) { "PASS" } else { "FAIL" }) -Notes ""
        $step++

        $approvePartResponse = Invoke-TestCase -TestName "Client approves order service part" -Category "ClientApprovals" -Method "POST" -PathOrUrl "/api/client/approvals/order-service-parts/$orderServicePartId/approve" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 200 -Body @{ Observation = "Approved part by E2E client" } -Criticality "Critical"
        Add-MainFlowStep -Step $step -Action "Client approve part" -Endpoint "/api/client/approvals/order-service-parts/{id}/approve" -Role "Client" -Expected "200" -Actual $approvePartResponse.StatusCode -Result $(if ($approvePartResponse.Passed) { "PASS" } else { "FAIL" }) -Notes ""
        $step++

        $completeOrderResponse = Invoke-TestCase -TestName "Complete service order" -Category "WorkshopFlow" -Method "POST" -PathOrUrl "/api/service-orders/$serviceOrderId/complete" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200 -Criticality "Critical"
        Add-MainFlowStep -Step $step -Action "Complete service order" -Endpoint "/api/service-orders/{id}/complete" -Role "Receptionist" -Expected "200" -Actual $completeOrderResponse.StatusCode -Result $(if ($completeOrderResponse.Passed) { "PASS" } else { "FAIL" }) -Notes ""
        $step++

        $fullDetailResponse = Invoke-TestCase -TestName "Get service order full detail" -Category "WorkshopFlow" -Method "GET" -PathOrUrl "/api/service-orders/$serviceOrderId/full-detail" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200 -Criticality "High"
        $hasServices = $false
        if ($fullDetailResponse.Passed -and $null -ne $fullDetailResponse.Data) {
            $fullDetailServices = @(Get-PropertyValue $fullDetailResponse.Data "services")
            $hasServices = (Get-ItemCount -Items $fullDetailServices) -gt 0
        }
        Add-TestResult -TestName "Full detail includes services" `
            -Category "WorkshopFlow" `
            -Method "GET" `
            -Url "$script:BaseUrlNormalized/api/service-orders/$serviceOrderId/full-detail" `
            -Role "Admin" `
            -ExpectedStatusCode "200 with non-empty services" `
            -ActualStatusCode $fullDetailResponse.StatusCode `
            -Passed ($fullDetailResponse.Passed -and $hasServices) `
            -Skipped $false `
            -DurationMs $fullDetailResponse.DurationMs `
            -RequestBodySummary "" `
            -ResponseSnippet (Sanitize-Response $fullDetailResponse.Data) `
            -ErrorMessage "" `
            -Notes "servicesCount=$(if ($fullDetailResponse.Data) { Get-ItemCount -Items @((Get-PropertyValue $fullDetailResponse.Data 'services')) } else { 0 })" `
            -Criticality "High"

        Add-MainFlowStep -Step $step -Action "Read full detail" -Endpoint "/api/service-orders/{id}/full-detail" -Role "Admin" -Expected "200 + services" -Actual $fullDetailResponse.StatusCode -Result $(if ($fullDetailResponse.Passed -and $hasServices) { "PASS" } else { "FAIL" }) -Notes ""
        $step++

        $generateInvoiceResponse = Invoke-TestCase -TestName "Generate invoice from service order" -Category "InvoiceFlow" -Method "POST" -PathOrUrl "/api/invoices/generate-from-service-order/$serviceOrderId" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200 -Body @{
            InvoiceNumber   = $null
            InvoiceStatusId = $(if ($null -ne $catalogIds.InvoiceStatusDraftId) { $catalogIds.InvoiceStatusDraftId } else { $null })
            Tax             = 19000
            Observations    = "E2E generated invoice"
        } -Criticality "Critical"
        $invoiceId = if ($generateInvoiceResponse.Passed) { [int](Get-PropertyValue $generateInvoiceResponse.Data "invoiceId") } else { 0 }
        $invoiceTotal = if ($generateInvoiceResponse.Passed) { [decimal](Get-PropertyValue $generateInvoiceResponse.Data "total") } else { 0 }
        if ($invoiceId -gt 0) { $script:GeneratedData.invoiceId = $invoiceId; $flowContext.invoiceId = $invoiceId; $flowContext.invoiceTotal = $invoiceTotal }
        Add-MainFlowStep -Step $step -Action "Generate invoice" -Endpoint "/api/invoices/generate-from-service-order/{serviceOrderId}" -Role "Receptionist" -Expected "200" -Actual $generateInvoiceResponse.StatusCode -Result $(if ($generateInvoiceResponse.Passed) { "PASS" } else { "FAIL" }) -Notes "invoiceId=$invoiceId; total=$invoiceTotal"
        $step++
        Stop-OnMissingCriticalValue -Name "invoiceId" -Value $invoiceId

        $issueInvoiceResponse = Invoke-TestCase -TestName "Issue invoice" -Category "InvoiceFlow" -Method "POST" -PathOrUrl "/api/invoices/$invoiceId/issue" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200 -Criticality "High"
        Add-MainFlowStep -Step $step -Action "Issue invoice" -Endpoint "/api/invoices/{id}/issue" -Role "Receptionist" -Expected "200" -Actual $issueInvoiceResponse.StatusCode -Result $(if ($issueInvoiceResponse.Passed) { "PASS" } else { "FAIL" }) -Notes ""
        $step++

        $paymentAmount = if ($invoiceTotal -gt 0) { $invoiceTotal } else { 1 }
        $recordPaymentResponse = Invoke-TestCase -TestName "Record payment for invoice" -Category "PaymentFlow" -Method "POST" -PathOrUrl "/api/invoices/$invoiceId/record-payment" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200 -Body @{
            PaymentMethodId = $(if ($null -ne $catalogIds.PaymentMethodCashId) { $catalogIds.PaymentMethodCashId } else { 1 })
            PaymentStatusId = $(if ($null -ne $catalogIds.PaymentStatusCompId) { $catalogIds.PaymentStatusCompId } else { 2 })
            PaymentDate     = $null
            Amount          = $paymentAmount
            Reference       = "E2E PAYMENT $timestamp"
            Card            = $null
        } -Criticality "Critical"
        $paymentId = if ($recordPaymentResponse.Passed) { [int](Get-PropertyValue $recordPaymentResponse.Data "paymentId") } else { 0 }
        if ($paymentId -gt 0) { $script:GeneratedData.paymentId = $paymentId; $flowContext.paymentId = $paymentId }
        Add-MainFlowStep -Step $step -Action "Record payment" -Endpoint "/api/invoices/{id}/record-payment" -Role "Receptionist" -Expected "200" -Actual $recordPaymentResponse.StatusCode -Result $(if ($recordPaymentResponse.Passed) { "PASS" } else { "FAIL" }) -Notes "paymentId=$paymentId; amount=$paymentAmount"
        $step++
        Stop-OnMissingCriticalValue -Name "paymentId" -Value $paymentId

        $summaryAdminResponse = Invoke-TestCase -TestName "Payment summary as Admin" -Category "PaymentFlow" -Method "GET" -PathOrUrl "/api/invoices/$invoiceId/payment-summary" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200 -Criticality "High"
        Add-MainFlowStep -Step $step -Action "Payment summary (Admin)" -Endpoint "/api/invoices/{id}/payment-summary" -Role "Admin" -Expected "200" -Actual $summaryAdminResponse.StatusCode -Result $(if ($summaryAdminResponse.Passed) { "PASS" } else { "FAIL" }) -Notes ""
        $step++

        $summaryClientResponse = Invoke-TestCase -TestName "Payment summary as owner Client" -Category "PaymentFlow" -Method "GET" -PathOrUrl "/api/invoices/$invoiceId/payment-summary" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 200 -Criticality "High"
        Add-MainFlowStep -Step $step -Action "Payment summary (Client owner)" -Endpoint "/api/invoices/{id}/payment-summary" -Role "Client" -Expected "200" -Actual $summaryClientResponse.StatusCode -Result $(if ($summaryClientResponse.Passed) { "PASS" } else { "FAIL" }) -Notes ""
        $step++

        # Dashboards category explicit checks
        Invoke-TestCase -TestName "Admin dashboard final" -Category "Dashboards" -Method "GET" -PathOrUrl "/api/admin/dashboard" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Client dashboard final" -Category "Dashboards" -Method "GET" -PathOrUrl "/api/client/dashboard" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Mechanic dashboard final" -Category "Dashboards" -Method "GET" -PathOrUrl "/api/mechanic/dashboard" -Role "Mechanic" -Token $mechanicAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Receptionist dashboard final" -Category "Dashboards" -Method "GET" -PathOrUrl "/api/receptionist/dashboard" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200
    }
    else {
        Add-SkippedTest -TestName "Main workshop flow" -Category "WorkshopFlow" -Method "POST" -Url "/api/workshop-intake/create-service-order" -Role "Receptionist/Admin" -ExpectedStatusCode "200/201" -Notes "Skipped because -CreateTestData was not provided."
    }

    # =====================================
    # 12. Ownership security tests
    # =====================================
    if ($CreateTestData -and $null -ne $flowContext.serviceOrderId -and $null -ne $flowContext.invoiceId) {
        $secondClientEmail = New-TestEmail -Prefix "client2.e2e"
        $secondClientPassword = "SecondClient123*"
        $secondClientRegister = Invoke-TestCase -TestName "Register second client for ownership tests" -Category "OwnershipSecurity" -Method "POST" -PathOrUrl "/api/auth/register-client" -Role "Anonymous" -ExpectedStatusCode 200 -Body @{
            DocumentTypeId = $(if ($null -ne $catalogIds.DocumentTypeId) { $catalogIds.DocumentTypeId } else { 1 })
            DocumentNumber = (New-TestDocumentNumber -Prefix "CLI2")
            FirstName      = "E2E"
            LastName       = "SecondClient"
            GenderId       = $catalogIds.GenderId
            Email          = $secondClientEmail
            Password       = $secondClientPassword
        } -Criticality "High"

        if ($secondClientRegister.Passed -and $null -ne $secondClientRegister.Data) {
            $secondClientAuth = [PSCustomObject]@{
                AccessToken  = Get-PropertyValue -Object $secondClientRegister.Data -Name "accessToken"
                RefreshToken = Get-PropertyValue -Object $secondClientRegister.Data -Name "refreshToken"
                PersonId     = Get-PropertyValue -Object (Get-PropertyValue -Object $secondClientRegister.Data -Name "user") -Name "personId"
            }
            if ($null -ne $secondClientAuth.PersonId) {
                $script:GeneratedData.secondClientPersonId = [int]$secondClientAuth.PersonId
            }
        }

        if ($null -ne $secondClientAuth -and -not [string]::IsNullOrWhiteSpace($secondClientAuth.AccessToken)) {
            Invoke-TestCase -TestName "Second client blocked from full-detail" -Category "OwnershipSecurity" -Method "GET" -PathOrUrl "/api/service-orders/$($flowContext.serviceOrderId)/full-detail" -Role "Client(Other)" -Token $secondClientAuth.AccessToken -ExpectedStatusCode @(400, 403, 404, 409) -Criticality "Critical"
            Invoke-TestCase -TestName "Second client blocked from payment-summary" -Category "OwnershipSecurity" -Method "GET" -PathOrUrl "/api/invoices/$($flowContext.invoiceId)/payment-summary" -Role "Client(Other)" -Token $secondClientAuth.AccessToken -ExpectedStatusCode @(400, 403, 404, 409) -Criticality "Critical"
            Invoke-TestCase -TestName "Second client blocked from approving order service" -Category "OwnershipSecurity" -Method "POST" -PathOrUrl "/api/client/approvals/order-services/$($flowContext.orderServiceId)/approve" -Role "Client(Other)" -Token $secondClientAuth.AccessToken -ExpectedStatusCode @(400, 403, 404, 409) -Body @{ Observation = "Unauthorized ownership approval attempt." } -Criticality "Critical"
        }
        else {
            Add-SkippedTest -TestName "Ownership security negative tests" -Category "OwnershipSecurity" -Method "GET" -Url "/api/service-orders/{id}/full-detail" -Role "Client(Other)" -ExpectedStatusCode "400/403/404/409" -Notes "Second client token was not available."
        }
    }
    else {
        Add-SkippedTest -TestName "Ownership security tests" -Category "OwnershipSecurity" -Method "GET" -Url "/api/service-orders/{id}/full-detail" -Role "Client(Other)" -ExpectedStatusCode "400/403/404/409" -Notes "Flow IDs were not available."
    }

    # =====================================
    # 13. Search tests
    # =====================================
    $searchToken = if ($null -ne $receptionistAuth -and -not [string]::IsNullOrWhiteSpace($receptionistAuth.AccessToken)) { $receptionistAuth.AccessToken } else { $adminAuth.AccessToken }
    $searchRole = if ($searchToken -eq $adminAuth.AccessToken) { "Admin" } else { "Receptionist" }

    Invoke-TestCase -TestName "Search clients by e2e term" -Category "Search" -Method "GET" -PathOrUrl "/api/search/clients?term=e2e" -Role $searchRole -Token $searchToken -ExpectedStatusCode 200
    Invoke-TestCase -TestName "Search vehicles by E2E term" -Category "Search" -Method "GET" -PathOrUrl "/api/search/vehicles?term=E2E" -Role $searchRole -Token $searchToken -ExpectedStatusCode 200
    if ($null -ne $flowContext.serviceOrderId) {
        Invoke-TestCase -TestName "Search service-orders by id term" -Category "Search" -Method "GET" -PathOrUrl "/api/search/service-orders?term=$($flowContext.serviceOrderId)" -Role $searchRole -Token $searchToken -ExpectedStatusCode 200
    }
    else {
        Add-SkippedTest -TestName "Search service-orders by id term" -Category "Search" -Method "GET" -Url "/api/search/service-orders?term={serviceOrderId}" -Role $searchRole -ExpectedStatusCode 200 -Notes "serviceOrderId is unavailable."
    }

    if ($null -ne $flowContext.invoiceId) {
        Invoke-TestCase -TestName "Search invoices by id term" -Category "Search" -Method "GET" -PathOrUrl "/api/search/invoices?term=$($flowContext.invoiceId)" -Role $searchRole -Token $searchToken -ExpectedStatusCode 200
    }
    else {
        Add-SkippedTest -TestName "Search invoices by id term" -Category "Search" -Method "GET" -Url "/api/search/invoices?term={invoiceId}" -Role $searchRole -ExpectedStatusCode 200 -Notes "invoiceId is unavailable."
    }

    Invoke-TestCase -TestName "Search parts by E2E term" -Category "Search" -Method "GET" -PathOrUrl "/api/search/parts?term=E2E" -Role $searchRole -Token $searchToken -ExpectedStatusCode 200
    Invoke-TestCase -TestName "Search suppliers by E2E term" -Category "Search" -Method "GET" -PathOrUrl "/api/search/suppliers?term=E2E" -Role $searchRole -Token $searchToken -ExpectedStatusCode 200
    Invoke-TestCase -TestName "Search mechanics by e2e term" -Category "Search" -Method "GET" -PathOrUrl "/api/search/mechanics?term=e2e" -Role $searchRole -Token $searchToken -ExpectedStatusCode 200
    Invoke-TestCase -TestName "Search parts with too-short term" -Category "Search" -Method "GET" -PathOrUrl "/api/search/parts?term=a" -Role $searchRole -Token $searchToken -ExpectedStatusCode 400 -Notes "Minimum term length should be enforced."

    # =====================================
    # 14. Reports tests
    # =====================================
    Invoke-TestCase -TestName "Sales report (Admin)" -Category "Reports" -Method "GET" -PathOrUrl "/api/admin/reports/sales" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200
    Invoke-TestCase -TestName "Inventory report (Admin)" -Category "Reports" -Method "GET" -PathOrUrl "/api/admin/reports/inventory" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200
    Invoke-TestCase -TestName "Mechanics report (Admin)" -Category "Reports" -Method "GET" -PathOrUrl "/api/admin/reports/mechanics" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200
    Invoke-TestCase -TestName "Service orders report (Admin)" -Category "Reports" -Method "GET" -PathOrUrl "/api/admin/reports/service-orders" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200
    Invoke-TestCase -TestName "Payments report (Admin)" -Category "Reports" -Method "GET" -PathOrUrl "/api/admin/reports/payments" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200
    Invoke-TestCase -TestName "Sales report invalid date range" -Category "Reports" -Method "GET" -PathOrUrl "/api/admin/reports/sales?from=2030-01-01&to=2020-01-01" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 400 -Notes "from > to should be rejected."

    # =====================================
    # 15. Audits tests
    # =====================================
    Invoke-TestCase -TestName "Admin audits recent" -Category "Audits" -Method "GET" -PathOrUrl "/api/admin/audits/recent" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200
    Invoke-TestCase -TestName "Admin audits by user 1" -Category "Audits" -Method "GET" -PathOrUrl "/api/admin/audits/by-user/1" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200
    $auditEntityRecordId = if ($script:GeneratedData.partId -ne "N/A") { [int]$script:GeneratedData.partId } else { 1 }
    Invoke-TestCase -TestName "Admin audits by entity and record" -Category "Audits" -Method "GET" -PathOrUrl "/api/admin/audits/by-entity?entity=Part&recordId=$auditEntityRecordId" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode @(200, 400, 404) -Notes "Non-auth failure is acceptable if no audit data exists."
    Invoke-TestCase -TestName "Client blocked from admin audits recent" -Category "Audits" -Method "GET" -PathOrUrl "/api/admin/audits/recent" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 403

    # =====================================
    # 16. Inventory tests
    # =====================================
    Invoke-TestCase -TestName "Inventory summary (Admin)" -Category "Inventory" -Method "GET" -PathOrUrl "/api/inventory/summary" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200
    Invoke-TestCase -TestName "Inventory low-stock (Admin)" -Category "Inventory" -Method "GET" -PathOrUrl "/api/inventory/low-stock" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200

    if (-not $SkipDestructiveTests) {
        if ($script:GeneratedData.partId -ne "N/A") {
            $partIdToAdjust = [int]$script:GeneratedData.partId
            Invoke-TestCase -TestName "Adjust stock positive (Admin)" -Category "Inventory" -Method "POST" -PathOrUrl "/api/inventory/adjust-stock" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200 -Body @{
                PartId             = $partIdToAdjust
                AdjustmentQuantity = 2
                Reason             = "E2E positive stock adjustment"
            }
            Invoke-TestCase -TestName "Adjust stock impossible negative (Admin)" -Category "Inventory" -Method "POST" -PathOrUrl "/api/inventory/adjust-stock" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode @(400, 409) -Body @{
                PartId             = $partIdToAdjust
                AdjustmentQuantity = -999999
                Reason             = "E2E invalid stock adjustment"
            } -Notes "Should not allow stock below zero."
        }
        else {
            Add-SkippedTest -TestName "Adjust stock positive (Admin)" -Category "Inventory" -Method "POST" -Url "/api/inventory/adjust-stock" -Role "Admin" -ExpectedStatusCode 200 -Notes "partId is unavailable."
            Add-SkippedTest -TestName "Adjust stock impossible negative (Admin)" -Category "Inventory" -Method "POST" -Url "/api/inventory/adjust-stock" -Role "Admin" -ExpectedStatusCode "400/409" -Notes "partId is unavailable."
        }
    }
    else {
        Add-SkippedTest -TestName "Adjust stock positive (Admin)" -Category "Inventory" -Method "POST" -Url "/api/inventory/adjust-stock" -Role "Admin" -ExpectedStatusCode 200 -Notes "Skipped due -SkipDestructiveTests."
        Add-SkippedTest -TestName "Adjust stock impossible negative (Admin)" -Category "Inventory" -Method "POST" -Url "/api/inventory/adjust-stock" -Role "Admin" -ExpectedStatusCode "400/409" -Notes "Skipped due -SkipDestructiveTests."
    }

    if ($null -ne $receptionistAuth -and -not [string]::IsNullOrWhiteSpace($receptionistAuth.AccessToken)) {
        Invoke-TestCase -TestName "Inventory summary (Receptionist)" -Category "Inventory" -Method "GET" -PathOrUrl "/api/inventory/summary" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 200
        Invoke-TestCase -TestName "Adjust stock forbidden for Receptionist" -Category "Inventory" -Method "POST" -PathOrUrl "/api/inventory/adjust-stock" -Role "Receptionist" -Token $receptionistAuth.AccessToken -ExpectedStatusCode 403 -Body @{
            PartId             = 1
            AdjustmentQuantity = 1
            Reason             = "Unauthorized adjustment test"
        }
    }
    else {
        Add-SkippedTest -TestName "Inventory summary (Receptionist)" -Category "Inventory" -Method "GET" -Url "/api/inventory/summary" -Role "Receptionist" -ExpectedStatusCode 200 -Notes "Receptionist token is unavailable."
    }

    # =====================================
    # 17. Final consistency checks
    # =====================================
    $finalAdminDashboard = Invoke-TestCase -TestName "Final admin dashboard snapshot" -Category "FinalConsistency" -Method "GET" -PathOrUrl "/api/admin/dashboard" -Role "Admin" -Token $adminAuth.AccessToken -ExpectedStatusCode 200 -Criticality "High"
    if ($finalAdminDashboard.Passed -and $null -ne $finalAdminDashboard.Data) {
        $totalUsers = [int](Get-PropertyValue $finalAdminDashboard.Data "totalUsers")
        $totalClients = [int](Get-PropertyValue $finalAdminDashboard.Data "totalClients")
        $totalMechanics = [int](Get-PropertyValue $finalAdminDashboard.Data "totalMechanics")
        $totalInvoicedAmount = [decimal](Get-PropertyValue $finalAdminDashboard.Data "totalInvoicedAmount")
        $totalCompletedPaymentsAmount = [decimal](Get-PropertyValue $finalAdminDashboard.Data "totalCompletedPaymentsAmount")

        Add-TestResult -TestName "Final admin metrics thresholds" `
            -Category "FinalConsistency" `
            -Method "GET" `
            -Url "$script:BaseUrlNormalized/api/admin/dashboard" `
            -Role "Admin" `
            -ExpectedStatusCode "totalUsers>=4,totalClients>=2,totalMechanics>=1,totalInvoicedAmount>=0,totalCompletedPaymentsAmount>=0" `
            -ActualStatusCode $finalAdminDashboard.StatusCode `
            -Passed ($totalUsers -ge 4 -and $totalClients -ge 2 -and $totalMechanics -ge 1 -and $totalInvoicedAmount -ge 0 -and $totalCompletedPaymentsAmount -ge 0) `
            -Skipped $false `
            -DurationMs $finalAdminDashboard.DurationMs `
            -RequestBodySummary "" `
            -ResponseSnippet (Sanitize-Response $finalAdminDashboard.Data) `
            -ErrorMessage "" `
            -Notes "totalUsers=$totalUsers,totalClients=$totalClients,totalMechanics=$totalMechanics,totalInvoicedAmount=$totalInvoicedAmount,totalCompletedPaymentsAmount=$totalCompletedPaymentsAmount" `
            -Criticality "High"
    }

    if ($null -ne $clientAuth -and -not [string]::IsNullOrWhiteSpace($clientAuth.AccessToken)) {
        $clientVehiclesResp = Invoke-TestCase -TestName "Final client my-vehicles list" -Category "FinalConsistency" -Method "GET" -PathOrUrl "/api/client/my-vehicles" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 200
        $clientOrdersResp = Invoke-TestCase -TestName "Final client my-service-orders list" -Category "FinalConsistency" -Method "GET" -PathOrUrl "/api/client/my-service-orders" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 200
        $clientInvoicesResp = Invoke-TestCase -TestName "Final client my-invoices list" -Category "FinalConsistency" -Method "GET" -PathOrUrl "/api/client/my-invoices" -Role "Client" -Token $clientAuth.AccessToken -ExpectedStatusCode 200

        $vehiclesCount = if ($clientVehiclesResp.Passed) { Get-ItemCount -Items @($clientVehiclesResp.Data) } else { 0 }
        $ordersCount = if ($clientOrdersResp.Passed) { Get-ItemCount -Items @($clientOrdersResp.Data) } else { 0 }
        $invoicesCount = if ($clientInvoicesResp.Passed) { Get-ItemCount -Items @($clientInvoicesResp.Data) } else { 0 }

        Add-TestResult -TestName "Client owns at least one vehicle/order/invoice" `
            -Category "FinalConsistency" `
            -Method "GET" `
            -Url "$script:BaseUrlNormalized/api/client/my-* (vehicles/orders/invoices)" `
            -Role "Client" `
            -ExpectedStatusCode ">=1 item in each list" `
            -ActualStatusCode 200 `
            -Passed ($vehiclesCount -ge 1 -and $ordersCount -ge 1 -and $invoicesCount -ge 1) `
            -Skipped $false `
            -DurationMs ($clientVehiclesResp.DurationMs + $clientOrdersResp.DurationMs + $clientInvoicesResp.DurationMs) `
            -RequestBodySummary "" `
            -ResponseSnippet "vehicles=$vehiclesCount,orders=$ordersCount,invoices=$invoicesCount" `
            -ErrorMessage "" `
            -Notes "Consistency check for client ownership data." `
            -Criticality "High"
    }

    if ($null -ne $mechanicAuth -and -not [string]::IsNullOrWhiteSpace($mechanicAuth.AccessToken)) {
        $mechanicAssignedResp = Invoke-TestCase -TestName "Final mechanic assigned services list" -Category "FinalConsistency" -Method "GET" -PathOrUrl "/api/mechanic/my-assigned-services" -Role "Mechanic" -Token $mechanicAuth.AccessToken -ExpectedStatusCode 200
        $assignedCount = if ($mechanicAssignedResp.Passed) { Get-ItemCount -Items @($mechanicAssignedResp.Data) } else { 0 }

        Add-TestResult -TestName "Mechanic has at least one assigned service" `
            -Category "FinalConsistency" `
            -Method "GET" `
            -Url "$script:BaseUrlNormalized/api/mechanic/my-assigned-services" `
            -Role "Mechanic" `
            -ExpectedStatusCode ">=1 item" `
            -ActualStatusCode $mechanicAssignedResp.StatusCode `
            -Passed ($mechanicAssignedResp.Passed -and $assignedCount -ge 1) `
            -Skipped $false `
            -DurationMs $mechanicAssignedResp.DurationMs `
            -RequestBodySummary "" `
            -ResponseSnippet "assignedServices=$assignedCount" `
            -ErrorMessage "" `
            -Notes "Consistency check for mechanic assignments." `
            -Criticality "High"
    }
}
catch {
    $flowAbortedReason = $_.Exception.Message
    $script:Warnings.Add("E2E execution aborted due to critical missing prerequisite: $flowAbortedReason") | Out-Null
    Write-TestLog "Execution aborted: $flowAbortedReason" "ERROR"
}
finally {
    $summary = Get-TestSummary

    $categorySummary = @(
        $script:TestResults |
            Group-Object Category |
            Sort-Object Name |
            ForEach-Object {
                $group = @($_.Group)
                [PSCustomObject]@{
                    Category = $_.Name
                    Total    = $group.Count
                    Passed   = (@($group | Where-Object { $_.Passed -and -not $_.Skipped })).Count
                    Failed   = (@($group | Where-Object { -not $_.Passed -and -not $_.Skipped })).Count
                    Skipped  = (@($group | Where-Object { $_.Skipped })).Count
                    Warnings = (@($group | Where-Object { $_.Notes -like "*warning*" })).Count
                }
            }
    )

    $recommendations = New-Object System.Collections.ArrayList
    if ($summary.Verdict -eq "PASS") {
        $recommendations.Add("Proceed with delivery candidate and keep this E2E runner in CI for regression coverage.") | Out-Null
    }
    else {
        if ((@($script:TestResults | Where-Object { $_.Category -eq "ForbiddenAccess403" -and -not $_.Passed -and -not $_.Skipped }).Count) -gt 0) {
            $recommendations.Add("Review [Authorize(Roles=...)] declarations and role assignments for failed 403 checks.") | Out-Null
        }
        if ((@($script:TestResults | Where-Object { $_.Category -eq "UnauthorizedAccess401" -and -not $_.Passed -and -not $_.Skipped }).Count) -gt 0) {
            $recommendations.Add("Review JWT middleware/order and authentication setup for failed 401 checks.") | Out-Null
        }
        if ((@($script:TestResults | Where-Object { $_.ActualStatusCode -eq 500 }).Count) -gt 0) {
            $recommendations.Add("Inspect API logs for 500 responses and add defensive validation where needed.") | Out-Null
        }
        if ((@($script:TestResults | Where-Object { $_.Category -eq "OwnershipSecurity" -and -not $_.Passed -and -not $_.Skipped }).Count) -gt 0) {
            $recommendations.Add("Review ownership validation in Application services for client-scoped resources.") | Out-Null
        }
        if ($null -ne $flowAbortedReason) {
            $recommendations.Add("Main workshop flow aborted: $flowAbortedReason") | Out-Null
        }
        if ($recommendations.Count -eq 0) {
            $recommendations.Add("Re-run E2E with fresh database and inspect failing endpoints listed in this report.") | Out-Null
        }
    }

    $metadata = [ordered]@{
        ExecutionDateUtc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
        BaseUrl          = $script:BaseUrlNormalized
        Environment      = $script:EnvironmentName
        Machine          = $script:MachineName
        User             = $script:UserName
        Branch           = $branch
        Commit           = $commit
        CreateTestData   = [bool]$CreateTestData
        SkipDestructive  = [bool]$SkipDestructiveTests
    }

    Save-ReportMarkdown -Path $markdownReportPath -Metadata $metadata -Summary $summary -CategorySummary $categorySummary -Recommendations @($recommendations) -FinalVerdict $summary.Verdict
    Save-ReportJson -Path $jsonReportPath -Metadata $metadata -Summary $summary -CategorySummary $categorySummary -Recommendations @($recommendations) -FinalVerdict $summary.Verdict

    Write-TestLog "Markdown report generated: $markdownReportPath" "INFO"
    Write-TestLog "JSON report generated: $jsonReportPath" "INFO"
    Write-TestLog "Final verdict: $($summary.Verdict) | Total: $($summary.Total) | Passed: $($summary.Passed) | Failed: $($summary.Failed) | Skipped: $($summary.Skipped)" "INFO"
}
