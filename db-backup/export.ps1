$server = "localhost"
$database = "ShopAI"
$outputFile = "D:\Final_Year_Project-BSIT-20260531T111602Z-3-001\Final_Year_Project-BSIT\ShopAI\db-backup\ShopAI.sql"

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("-- ShopAI Database Export")
[void]$sb.AppendLine("-- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
[void]$sb.AppendLine("USE [$database];")
[void]$sb.AppendLine("GO")
[void]$sb.AppendLine("")

$tables = sqlcmd -S $server -E -d $database -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME != '__EFMigrationsHistory' ORDER BY TABLE_NAME" -h -1 -W 2>&1 | Where-Object { $_.Trim() -ne '' -and $_ -notmatch "rows affected" }

foreach ($table in $tables) {
    $table = $table.Trim()
    Write-Host "Processing: $table"

    # Get columns using pipe delimiter
    $colLines = sqlcmd -S $server -E -d $database -Q "
        SELECT c.COLUMN_NAME, c.DATA_TYPE, c.CHARACTER_MAXIMUM_LENGTH, c.IS_NULLABLE
        FROM INFORMATION_SCHEMA.COLUMNS c
        WHERE c.TABLE_NAME = '$table'
        ORDER BY c.ORDINAL_POSITION
    " -s "|" -W -h -1 2>&1 | Where-Object { $_.Trim() -ne '' -and $_ -notmatch "rows affected" }

    # DROP + CREATE
    [void]$sb.AppendLine("IF OBJECT_ID('dbo.[$table]', 'U') IS NOT NULL DROP TABLE dbo.[$table];")
    [void]$sb.AppendLine("CREATE TABLE dbo.[$table] (")

    $colDefs = @()
    foreach ($line in $colLines) {
        $parts = $line.Trim() -split '\|'
        if ($parts.Count -ge 4) {
            $colName = $parts[0].Trim()
            $dataType = $parts[1].Trim()
            $maxLen = $parts[2].Trim()
            $nullable = $parts[3].Trim()

            $sqlType = switch ($dataType) {
                "nvarchar" { if ($maxLen -eq "-1" -or $maxLen -eq "NULL") { "NVARCHAR(MAX)" } else { "NVARCHAR($maxLen)" } }
                "varchar"  { if ($maxLen -eq "-1" -or $maxLen -eq "NULL") { "VARCHAR(MAX)" } else { "VARCHAR($maxLen)" } }
                "ntext"    { "NVARCHAR(MAX)" }
                "int"      { "INT" }
                "bigint"   { "BIGINT" }
                "bit"      { "BIT" }
                "datetime" { "DATETIME2(7)" }
                "datetime2" { "DATETIME2(7)" }
                "decimal"  { "DECIMAL(18,2)" }
                "float"    { "FLOAT" }
                "uniqueidentifier" { "UNIQUEIDENTIFIER" }
                default    { $dataType.ToUpper() }
            }
            $nullStr = if ($nullable -eq "YES") { "NULL" } else { "NOT NULL" }
            $colDefs += "    [$colName] $sqlType $nullStr"
        }
    }
    [void]$sb.AppendLine(($colDefs -join ",`n"))
    [void]$sb.AppendLine(");")
    [void]$sb.AppendLine("GO")
    [void]$sb.AppendLine("")

    # Get data count
    $countResult = sqlcmd -S $server -E -d $database -Q "SELECT COUNT(*) FROM [$table]" -h -1 -W -s "|" 2>&1
    $count = 0
    foreach ($c in $countResult) {
        $trimmed = $c.Trim()
        if ($trimmed -match '^\d+$') { $count = [int]$trimmed; break }
    }

    if ($count -gt 0) {
        # Get column names
        $colNames = sqlcmd -S $server -E -d $database -Q "
            SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = '$table' ORDER BY ORDINAL_POSITION
        " -h -1 -W -s "|" 2>&1 | Where-Object { $_.Trim() -ne '' -and $_ -notmatch "rows affected" } | ForEach-Object { "[$($_.Trim())]" }
        $colList = $colNames -join ", "

        # Get data using tab delimiter for reliable parsing
        $dataFile = [System.IO.Path]::GetTempFileName()
        sqlcmd -S $server -E -d $database -Q "SELECT * FROM [$table]" -h -1 -W -s "`t" -o $dataFile 2>&1 | Out-Null

        $lines = Get-Content $dataFile
        $batchSize = 50
        $i = 0

        while ($i -lt $lines.Count) {
            $line = $lines[$i].Trim()
            $i++
            if ($line -eq "" -or $line -match "rows affected" -or $line -match "^\(" -or $line -match "^-") { continue }

            $values = @()
            $batch = 0
            # Process this line as first row of batch
            while ($true) {
                $fields = $line -split "`t"
                $sqlValues = @()
                foreach ($f in $fields) {
                    $v = $f.Trim()
                    if ($v -eq "" -or $v -eq "NULL") {
                        $sqlValues += "NULL"
                    } else {
                        $escaped = $v -replace "'", "''"
                        $sqlValues += "N'$escaped'"
                    }
                }
                $values += "    (" + ($sqlValues -join ", ") + ")"
                $batch++

                if ($batch -ge $batchSize) { break }
                if ($i -ge $lines.Count) { break }

                $line = $lines[$i].Trim()
                $i++
                if ($line -eq "" -or $line -match "rows affected" -or $line -match "^\(" -or $line -match "^-") { continue }
            }

            if ($values.Count -gt 0) {
                [void]$sb.AppendLine("INSERT INTO dbo.[$table] ($colList) VALUES")
                [void]$sb.AppendLine(($values -join ",`n"))
                [void]$sb.AppendLine(";")
                [void]$sb.AppendLine("GO")
            }
        }

        Remove-Item $dataFile -ErrorAction SilentlyContinue
    }

    [void]$sb.AppendLine("")
}

[System.IO.File]::WriteAllText($outputFile, $sb.ToString(), [System.Text.Encoding]::UTF8)

# Post-process: remove artifact lines
$content = Get-Content $outputFile -Raw
# Remove lines like (N'(22 rows affected)')
$content = $content -replace "(?m)^\s*\(N'\(\d+ rows affected\)'\)\s*\r?\n", ""
# Remove standalone "rows affected" lines
$content = $content -replace "(?m)^\s*\(\d+ rows affected\)\s*\r?\n", ""
# Remove empty INSERT ... VALUES followed by semicolons
$content = $content -replace "(?s)INSERT INTO [^\n]+ VALUES\r?\n\s*;\r?\nGO", ""
[System.IO.File]::WriteAllText($outputFile, $content, [System.Text.Encoding]::UTF8)

$fileSize = (Get-Item $outputFile).Length
Write-Host "`nDone! SQL file saved to: $outputFile ($fileSize bytes)" -ForegroundColor Green
