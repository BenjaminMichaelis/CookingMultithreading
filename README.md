# CookingMultithreading

A kitchen-sized demonstration of blocking, asynchronous concurrency, CPU
parallelism, and shared-state synchronization. Each branch is a runnable lesson,
not a recommended production implementation.

## Run a lesson

Install the .NET 8 SDK or a newer SDK that can target .NET 8, with the .NET 8
runtime. From PowerShell in the repository root:

```powershell
git switch main
dotnet run --project .\CookingMultithreading\CookingMultithreading.csproj --configuration Release
```

Switch branches and run the same command.

Each lesson is a single `Program.cs`, so with the .NET 10 SDK or later you can
also run it as a [file-based app](https://learn.microsoft.com/dotnet/core/sdk/file-based-apps),
without the project file:

```powershell
dotnet run --file .\CookingMultithreading\Program.cs --configuration Release
```

`--file` is required because `Program.cs` sits next to the `.csproj`. File-based
runs target the SDK's default framework rather than .NET 8.

Branches are listed in lesson order below. Each lesson has a numbered local
branch alias so `git branch` sorts in the same order. The original branch names
remain available for compatibility.

| # | Numbered branch | Original branch | Lesson | Approximate cooking time |
|---|---|---|---|---|
| 01 | `01-Synchronous` | `main` | `Thread.Sleep`, one dish at a time; caller blocks | 6 seconds |
| 02 | `02-AsyncCooking` | `AsyncCooking` | Call `CookAsync`, immediately `.Wait()`, then start the next dish; caller blocks | 6 seconds |
| 03 | `03-AsyncCooking-ThreadIds` | `AsyncCooking-ThreadIds` | Same as lesson 02, with coordinator checkpoints | 6 seconds |
| 04 | `04-AsyncCookingSequentialAwait` | `AsyncCookingSequentialAwait` | Await each dish before starting the next; caller returns at the first incomplete await | 6 seconds |
| 05 | `05-AsyncCookingFireUpFront` | `AsyncCookingFireUpFront` | Start all dishes, then `.Wait()` for each; caller blocks | 3 seconds |
| 06 | `06-AwaitBreakfast` | `AwaitBreakfast` | Start all dishes, then await each task; caller returns at the first incomplete await | 3 seconds |
| 07 | `07-AwaitTaskWhenAll` | `AwaitTaskWhenAll` | Start all dishes, then await the group; caller returns at the first incomplete await | 3 seconds |
| 08 | `08-ParallelCooking` | `ParallelCooking` | Compare sequential and thread-pool CPU work | — |
| 09 | `09-SharedStateRace` | `SharedStateRace` | Reproduce a lost update, then fix it with `Interlocked.Increment` | — |

Eggs take 3 seconds, bacon 2, and toast 1. Fixed, unequal waits make sequential
versus overlapping intervals easier to see; completion order and exact timings
still depend on scheduling. The early `.Wait()` branches are intentionally
blocking counterexamples.

## Read the timeline

Each row contains elapsed milliseconds, the managed thread ID, the operation,
and its phase. Follow the **operation name**, not just the thread ID: an async
operation can resume on a different thread, and one thread can execute parts of
many operations.

Compare these events:

- **Dish started/finished:** overlapping intervals demonstrate concurrent
  operations. Overlapping waits are not proof of parallel CPU execution.
- **Caller / kitchen call returned:** in the blocking branches this happens
  after cooking finishes. In the async branches it happens while cooking is
  pending, before **Caller / breakfast ready**.
- **Kitchen await checkpoints:** later awaits may complete synchronously because
  those tasks finished while the kitchen was awaiting eggs.

`async` does not create a thread. Calling `CookAsync` executes synchronously up
to its first incomplete await. `Task.Delay` simulates waiting for an external
operation; no thread is occupied throughout that wait. There is deliberately no
`Task.Run` around these delay-based operations.

Starting all three operations before awaiting is what permits overlap.
`Task.WhenAll` neither starts tasks nor creates threads. Awaiting already-started
tasks individually also permits overlap; `WhenAll` expresses waiting for the
whole group and has different failure-handling semantics.

### Why output is buffered

.NET synchronizes `Console.WriteLine` through its shared output writer.
Writing during concurrent work can serialize the writes, perturb scheduling,
and hide races. It does not serialize the entire operation.

Here, every dish and the caller/coordinator have their own in-memory trace
buffer with one logical writer. After all work has completed, the buffers are
merged by `Stopwatch` timestamp and printed. There is no console I/O or shared
logging lock inside the measured work, and formatting/output are excluded from
the reported cooking time. Capturing events still has overhead: this is a
teaching timeline, not a benchmark. Equal or nearby timestamps do not establish
a total causal order across threads.

### CPU work versus waiting

`ParallelCooking` uses an actual arithmetic loop, not `Task.Delay` or
`Thread.Sleep`, and consumes the resulting checksums. Compare its sequential
and parallel runs, not its timings against the delay-based lessons.

Thread-pool scheduling and available CPU cores determine how much execution
overlaps. Neither different thread IDs nor `Task.Run` guarantees simultaneous
execution or a speedup. Single-core machines, CPU quotas, startup costs, and
other workloads can make the parallel version slower.

### A reproducible race

`SharedStateRace` uses two dedicated threads and a two-phase `Barrier` to make
both cooks read the same counter value before either writes it back, then
finish both writes before the next iteration. This deliberately forces the
unsafe interleaving: 2,000 attempted increments produce only 1,000 servings.
The barriers coordinate the demonstration; they do not make the read-modify-write
operation atomic.

The second run uses `Interlocked.Increment` and produces all 2,000 servings.
No logging occurs between reading and updating the counter. This demonstrates
lost updates without hoping that a particular scheduler or console timing will
expose a race.

## Verify a branch

The dependency-free PowerShell check builds and runs the current lesson, then
checks the operation ordering, caller blocking/nonblocking behavior, and CPU or
counter results as applicable:

```powershell
.\tests\Verify-Sample.ps1
.\tests\Verify-Sample.ps1 -FileBased
dotnet format .\CookingMultithreading.slnx --verify-no-changes --no-restore
```

Use `.\tests\Verify-Sample.ps1 -NoBuild` after an existing Release build.
`-FileBased` runs `Program.cs` as a file-based app (requires the .NET 10 SDK or
later) with warnings treated as errors. The check
does not require exact elapsed times, particular thread IDs, or a parallel
speedup. Run timing-sensitive demonstrations on an otherwise responsive machine.
