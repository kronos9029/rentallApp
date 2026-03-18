param(
    [Parameter(Mandatory = $true)]
    [string]$PlaintextPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [Parameter(Mandatory = $true)]
    [string]$KeyBase64
)

$plaintext = Get-Content -Raw -Path $PlaintextPath
$key = [Convert]::FromBase64String($KeyBase64)

if ($key.Length -notin @(16, 24, 32)) {
    throw "AES key must be 128, 192, or 256 bits."
}

$nonce = [System.Security.Cryptography.RandomNumberGenerator]::GetBytes(12)
$plaintextBytes = [System.Text.Encoding]::UTF8.GetBytes($plaintext)
$ciphertext = New-Object byte[] $plaintextBytes.Length
$tag = New-Object byte[] 16

$aes = [System.Security.Cryptography.AesGcm]::new($key, 16)
try {
    $aes.Encrypt($nonce, $plaintextBytes, $ciphertext, $tag)
}
finally {
    $aes.Dispose()
}

$payload = New-Object byte[] ($nonce.Length + $tag.Length + $ciphertext.Length)
[Array]::Copy($nonce, 0, $payload, 0, $nonce.Length)
[Array]::Copy($tag, 0, $payload, $nonce.Length, $tag.Length)
[Array]::Copy($ciphertext, 0, $payload, $nonce.Length + $tag.Length, $ciphertext.Length)

$outputDirectory = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
    New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
}

Set-Content -Path $OutputPath -Value $payload -AsByteStream
