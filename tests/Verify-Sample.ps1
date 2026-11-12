param([switch]$NoBuild, [switch]$FileBased)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$sampleDirectory = Join-Path (Split-Path $PSScriptRoot -Parent) 'CookingMultithreading'
$project = Join-Path $sampleDirectory 'CookingMultithreading.csproj'

if ($FileBased) {
    # File-based apps (.NET 10+ SDK) build implicitly; a sibling .csproj requires --file.
    $output = @(& dotnet run --file (Join-Path $sampleDirectory 'Program.cs') --configuration Release -p:TreatWarningsAsErrors=true)
}
else {
    if (-not $NoBuild) {
        & dotnet build $project --configuration Release --nologo
        if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    }

    $output = @(& dotnet run --project $project --configuration Release --no-build)
}
if ($LASTEXITCODE -ne 0) { throw 'Sample failed.' }
$output | Write-Output
$text = $output -join "`n"

function Assert-Condition([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw $Message }
}

$lessonMatch = [regex]::Match($text, '(?m)^Lesson: ([\w-]+)$')
Assert-Condition $lessonMatch.Success 'Missing lesson name.'
$lesson = $lessonMatch.Groups[1].Value
$events = [System.Collections.Generic.List[object]]::new()
$run = ''
foreach ($line in $output) {
    if ($line -match '^Run: (.+)$') { $run = $Matches[1] }
    if ($line -match '^\s*(\d+\.\d{3})\s*\|\s*(\d+)\s*\|\s*([^|]+?)\s*\|\s*(.+)$') {
        $events.Add([pscustomobject]@{
            Run = $run
            Milliseconds = [double]::Parse($Matches[1], [Globalization.CultureInfo]::InvariantCulture)
            Operation = $Matches[3].Trim()
            Phase = $Matches[4]
        })
    }
}
Assert-Condition ($events.Count -gt 0) 'Missing buffered timeline.'

function Get-Event([string]$RunName, [string]$Operation, [string]$Phase) {
    $found = @($events | Where-Object { $_.Run -eq $RunName -and $_.Operation -eq $Operation -and $_.Phase -eq $Phase })
    Assert-Condition ($found.Count -eq 1) "Expected one '$Phase' event for $Operation in $RunName."
    return $found[0]
}

function Assert-Dishes([string]$RunName) {
    foreach ($dish in @('Eggs', 'Bacon', 'Toast')) {
        $start = Get-Event $RunName $dish 'started'
        $finish = Get-Event $RunName $dish 'finished'
        Assert-Condition ($start.Milliseconds -le $finish.Milliseconds) "$dish finished before it started."
    }
}

function Assert-Sequential([string]$RunName) {
    $eggs = Get-Event $RunName 'Eggs' 'finished'
    $baconStart = Get-Event $RunName 'Bacon' 'started'
    $baconFinish = Get-Event $RunName 'Bacon' 'finished'
    $toast = Get-Event $RunName 'Toast' 'started'
    Assert-Condition ($eggs.Milliseconds -le $baconStart.Milliseconds) 'Eggs and bacon should be sequential.'
    Assert-Condition ($baconFinish.Milliseconds -le $toast.Milliseconds) 'Bacon and toast should be sequential.'
}

switch ($lesson) {
    'ParallelCooking' {
        Assert-Dishes 'CPU sequential'
        Assert-Dishes 'CPU parallel'
        Assert-Sequential 'CPU sequential'
        $sequential = [regex]::Match($text, '(?m)^Sequential checksum: (\d+)$')
        $parallel = [regex]::Match($text, '(?m)^Parallel checksum: (\d+)$')
        Assert-Condition ($sequential.Success -and $parallel.Success) 'Missing CPU results.'
        $iterations = [long]100000000
        $cycles = [long][math]::Floor($iterations / 97)
        $remainder = $iterations % 97
        $expected = [long](3 * ($cycles * 4656 + $remainder * ($remainder - 1) / 2))
        Assert-Condition ([long]$sequential.Groups[1].Value -eq $expected) 'Incorrect sequential CPU checksum.'
        Assert-Condition ([long]$parallel.Groups[1].Value -eq $expected) 'Incorrect parallel CPU checksum.'
    }
    'SharedStateRace' {
        foreach ($runName in @('Unsafe counter', 'Atomic counter')) {
            foreach ($cook in @('Cook 1', 'Cook 2')) {
                $start = Get-Event $runName $cook 'started'
                $finish = Get-Event $runName $cook 'finished'
                Assert-Condition ($start.Milliseconds -le $finish.Milliseconds) "$cook finished before it started."
            }
        }
        $unsafe = [regex]::Match($text, '(?m)^Unsafe servings: (\d+) / (\d+)$')
        $atomic = [regex]::Match($text, '(?m)^Atomic servings: (\d+) / (\d+)$')
        Assert-Condition ($unsafe.Success -and $atomic.Success) 'Missing counter results.'
        Assert-Condition ([int]$unsafe.Groups[1].Value * 2 -eq [int]$unsafe.Groups[2].Value) 'The coordinated lost update was not reproduced.'
        Assert-Condition ($unsafe.Groups[2].Value -eq $atomic.Groups[2].Value) 'The counter workloads differ.'
        Assert-Condition ($atomic.Groups[1].Value -eq $atomic.Groups[2].Value) 'Atomic increments lost updates.'
    }
    { $_ -in @('main', 'AsyncCooking', 'AsyncCooking-ThreadIds', 'AsyncCookingSequentialAwait', 'AsyncCookingFireUpFront', 'AwaitBreakfast', 'AwaitTaskWhenAll') } {
        Assert-Dishes 'Breakfast'
        if ($lesson -in @('main', 'AsyncCooking', 'AsyncCooking-ThreadIds', 'AsyncCookingSequentialAwait')) {
            Assert-Sequential 'Breakfast'
        }
        else {
            $starts = @('Eggs', 'Bacon', 'Toast') | ForEach-Object { (Get-Event 'Breakfast' $_ 'started').Milliseconds }
            $finishes = @('Eggs', 'Bacon', 'Toast') | ForEach-Object { (Get-Event 'Breakfast' $_ 'finished').Milliseconds }
            Assert-Condition (($starts | Measure-Object -Maximum).Maximum -lt ($finishes | Measure-Object -Minimum).Minimum) 'The three cooking intervals did not overlap.'
        }

        $returned = Get-Event 'Breakfast' 'Caller' 'kitchen call returned'
        $finished = Get-Event 'Breakfast' 'Kitchen' 'finished'
        $ready = Get-Event 'Breakfast' 'Caller' 'breakfast ready'
        Assert-Condition ($ready.Milliseconds -ge $finished.Milliseconds) 'Breakfast was served before cooking completed.'
        if ($lesson -in @('AsyncCookingSequentialAwait', 'AwaitBreakfast', 'AwaitTaskWhenAll')) {
            Assert-Condition ($returned.Milliseconds -lt $finished.Milliseconds) 'The async kitchen call blocked its caller.'
        }
        else {
            Assert-Condition ($returned.Milliseconds -ge $finished.Milliseconds) 'The blocking kitchen call returned too soon.'
        }
    }
    default { throw "Unknown lesson: $lesson" }
}

Write-Output "Verified $lesson."
