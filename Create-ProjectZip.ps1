# Erstellt eine ZIP-Datei vom Projektordner und lässt Build-Ausgaben und Binärdateien weg.
#
# Die ZIP landet standardmäßig im aktuellen Ordner der Konsole.
#
# Aufruf (im Projektordner oder von überall):
#   powershell -ExecutionPolicy Bypass -File .\Create-ProjectZip.ps1
#   powershell -ExecutionPolicy Bypass -File .\Create-ProjectZip.ps1 -SourceDir "C:\Pfad\Anicluster" -OutputFile "C:\Temp\Anicluster.zip"
#
# Ausgeschlossen werden: *.exe, *.pdf, die Ordner obj, bin, .vs, .vscode, .git sowie weitere übliche Build- und Benutzerdateien.
# Ausnahme: Alles unter Anicluster\bin\Debug\net8.0-windows\db\ wird wieder aufgenommen (Entwicklungsdatenbank).
# Die Listen unten lassen sich einfach erweitern.

param(
    [string]$SourceDir = $PSScriptRoot,
    [string]$OutputFile = ""
)

$ErrorActionPreference = "Stop"

# --- Einstellungen ---------------------------------------------------------------------------------------------

# Dateimuster (nur der Dateiname), die nicht in die ZIP kommen
$excludeFilePatterns = @(
    "*.exe", "*.pdf", "*.pdb", "*.user", "*.suo", "*.zip", "*.tmp"
)

# Ordnernamen, die an beliebiger Stelle im Baum nicht in die ZIP kommen
$excludeDirectoryNames = @(
    "obj", "bin", ".vs", ".vscode", ".git", "publish", "Publish"
)

# Ausnahmen: Pfade (relativ zum Projektordner, mit \), die trotz Ausschluss wieder aufgenommen werden.
# Gilt für alles, was unterhalb dieser Ordner liegt (auch *.exe usw. wären dort wieder dabei).
$includeAgainPrefixes = @(
    "Anicluster\bin\Debug\net8.0-windows\db\"
)

# ---------------------------------------------------------------------------------------------------------------

$SourceDir = (Resolve-Path -LiteralPath $SourceDir).Path.TrimEnd('\')
if ($OutputFile -eq "") {
    # Standard: im aktuellen Ordner der Konsole (nicht im Ordner darüber)
    $folderName = Split-Path -Leaf $SourceDir
    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $OutputFile = Join-Path (Get-Location).Path "$folderName-$stamp.zip"
}
$OutputFile = [System.IO.Path]::GetFullPath($OutputFile)

function Test-IncludedAgain([string]$relativePath) {
    foreach ($prefix in $includeAgainPrefixes) {
        if ($relativePath.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }
    return $false
}

function Test-Excluded([string]$relativePath, [string]$fileName) {
    if (Test-IncludedAgain $relativePath) {
        return $false
    }
    foreach ($pattern in $excludeFilePatterns) {
        if ($fileName -like $pattern) {
            return $true
        }
    }
    # Jeder Ordner im Pfad (ohne den Dateinamen am Ende) wird gegen die Liste geprüft
    $parts = $relativePath.Split('\')
    for ($i = 0; $i -lt $parts.Length - 1; $i++) {
        foreach ($name in $excludeDirectoryNames) {
            if ($parts[$i] -ieq $name) {
                return $true
            }
        }
    }
    return $false
}

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

if (Test-Path -LiteralPath $OutputFile) {
    Remove-Item -LiteralPath $OutputFile -Force
}

Write-Host "Quelle : $SourceDir"
Write-Host "Ziel   : $OutputFile"

$added = 0
$skipped = 0
$stream = [System.IO.File]::Open($OutputFile, [System.IO.FileMode]::Create)
try {
    $archive = New-Object System.IO.Compression.ZipArchive($stream, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        $files = Get-ChildItem -LiteralPath $SourceDir -Recurse -File -Force
        foreach ($file in $files) {
            if ($file.FullName -ieq $OutputFile) {
                continue # die ZIP-Datei nie in sich selbst packen
            }
            $relative = $file.FullName.Substring($SourceDir.Length + 1)
            if (Test-Excluded $relative $file.Name) {
                $skipped++
                continue
            }
            # Schrägstriche im Eintragsnamen, damit die ZIP überall korrekt entpackt wird
            $entryName = $relative.Replace('\', '/')
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
                $archive, $file.FullName, $entryName, [System.IO.Compression.CompressionLevel]::Optimal
            ) | Out-Null
            $added++
        }
    }
    finally {
        $archive.Dispose()
    }
}
finally {
    $stream.Dispose()
}

$sizeMb = [Math]::Round((Get-Item -LiteralPath $OutputFile).Length / 1MB, 2)
Write-Host "Fertig : $added Dateien aufgenommen, $skipped ausgeschlossen, $sizeMb MB"
