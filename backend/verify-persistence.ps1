$ErrorActionPreference = 'Stop'
$baseUrl = 'http://localhost:5071'
$api = "$baseUrl/api/credit-requests"
$appDirectory = Join-Path $PSScriptRoot 'CreditRequests.Api/bin/Debug/net10.0'

function Invoke-Api {
    param($Method, $Url, $ExpectedStatus, $Body)

    $options = @{ Method = $Method; Uri = $Url; UseBasicParsing = $true; TimeoutSec = 10 }
    if ($null -ne $Body) {
        $options.ContentType = 'application/json'
        $options.Body = ConvertTo-Json -InputObject $Body -Compress
    }

    try {
        $response = Invoke-WebRequest @options
        $actual = [int]$response.StatusCode
    }
    catch {
        if (-not $_.Exception.Response) { throw }
        $actual = [int]$_.Exception.Response.StatusCode
    }

    if ($actual -ne $ExpectedStatus) { throw "$Method $Url devolvió $actual; se esperaba $ExpectedStatus." }
    if ($actual -lt 400) { return $response.Content | ConvertFrom-Json }
}

function Start-Api {
    $process = Start-Process dotnet -ArgumentList @('CreditRequests.Api.dll', '--urls', $baseUrl) `
        -WorkingDirectory $appDirectory -WindowStyle Hidden -PassThru
    try {
        for ($attempt = 0; $attempt -lt 20; $attempt++) {
            if ($process.HasExited) { throw 'La API terminó durante el arranque.' }
            try {
                Invoke-Api GET $api 200 | Out-Null
                return $process
            }
            catch [System.Net.WebException] {
                Start-Sleep -Milliseconds 300
            }
        }
        throw 'La API no respondió durante el arranque.'
    }
    catch {
        Stop-Api $process
        throw
    }
}

function Stop-Api {
    param($Process)
    if ($null -ne $Process -and -not $Process.HasExited) {
        Stop-Process -InputObject $Process
        $Process.WaitForExit()
    }
}

if (-not (Test-Path (Join-Path $appDirectory 'CreditRequests.Api.dll'))) {
    throw 'Compila primero el backend con dotnet build en modo Debug.'
}

$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$process = $null
try {
    $process = Start-Api
    $payload = @{
        documentNumber = 'DEMO' + [guid]::NewGuid().ToString('N').Substring(0, 10)
        fullName = 'Persona de prueba'
        requestedAmount = 5000000.25
        termMonths = 12
    }

    $invalid = $payload.Clone()
    $invalid.termMonths = 121
    Invoke-Api POST $api 400 $invalid | Out-Null

    $first = Invoke-Api POST $api 201 $payload
    if ($first.status -ne 'Pending') { throw 'La solicitud recién creada debe quedar pendiente.' }
    $firstUrl = "$api/$($first.id)"
    Invoke-Api PATCH "$firstUrl/status" 400 @{ status = '1' } | Out-Null
    Invoke-Api PATCH "$firstUrl/status" 200 @{ status = 'Approved' } | Out-Null
    Invoke-Api PATCH "$firstUrl/status" 409 @{ status = 'Rejected' } | Out-Null
    Invoke-Api GET "$api/0" 404 | Out-Null

    $payload.documentNumber = 'DEMO' + [guid]::NewGuid().ToString('N').Substring(0, 10)
    $second = Invoke-Api POST $api 201 $payload
    Invoke-Api PATCH "$api/$($second.id)/status" 200 @{ status = 'Rejected' } | Out-Null

    Stop-Api $process
    $process = Start-Api
    $approved = Invoke-Api GET $firstUrl 200
    $rejected = Invoke-Api GET "$api/$($second.id)" 200
    $list = @(Invoke-Api GET $api 200)
    if ($approved.status -ne 'Approved' -or $rejected.status -ne 'Rejected' -or
        $approved.requestedAmount -ne $payload.requestedAmount -or
        $list.id -notcontains $first.id -or $list.id -notcontains $second.id) {
        throw 'La consulta luego del reinicio no devolvió los datos guardados.'
    }

    Write-Output "Persistencia verificada después de reiniciar la API. Solicitudes ficticias: $($first.id) Approved y $($second.id) Rejected."
    Write-Output 'Respuestas verificadas: 200, 201, 400, 404 y 409.'
}
finally {
    Stop-Api $process
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
}
