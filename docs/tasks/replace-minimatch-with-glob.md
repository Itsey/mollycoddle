---
status: todo
title: Replace Minimatch with Glob v1.1.9 across mollycoddle
created: 2026-05-17
priority: medium
reference: 1
---
# Reference
1 — Replace `Minimatch` NuGet v2.0.0 with `Glob` v1.1.9 (by kthompson) to eliminate legacy package warnings

# What
All references to the `Minimatch` NuGet package (v2.0.0) are removed from the mollycoddle solution and replaced with the `Glob` v1.1.9 package (`GlobExpressions`). Every call site in `mollycoddle.support` and `mollycoddle.test` uses the `GlobExpressions` API directly — no adapter or wrapper layer exists. The public properties `MinmatchActionCheckEntity.DoesMatch`, `RegexLineCheckEntity.DoesMatch`, and `RegexLineValidator.FileMinmatch` are typed as `GlobExpressions.Glob` rather than `Minimatcher`. No legacy-package warnings appear when building the solution after the change.

# Why
`Minimatch` v2.0.0 produces legacy/deprecation warnings at build time. The project needs a maintained, warning-free glob-matching library. `Glob` v1.1.9 by kthompson (`GlobExpressions`) is the agreed replacement and must provide identical matching semantics — case-insensitive, Windows-path-aware — so that all existing rule evaluations produce the same results.

# Acceptance

- **Given** the solution is built with `dotnet build`, **when** the build completes, **then** zero warnings referencing `Minimatch` or legacy package deprecation are emitted.
- **Given** the `mollycoddle.support` project file, **when** inspected, **then** it contains a `<PackageReference>` for `Glob` v1.1.9 and no `<PackageReference>` for `Minimatch`.
- **Given** the `mollycoddle.test` project file, **when** inspected, **then** it contains no `<PackageReference>` for `Minimatch` (add `Glob` reference only if test project references the type directly).
- **Given** all 6 previously Minimatch-referencing source files, **when** inspected, **then** none contain the token `Minimatch` or `Minimatcher` in any `using` directive, type reference, or instantiation.
- **Given** `MinmatchActionCheckEntity.DoesMatch`, `RegexLineCheckEntity.DoesMatch`, and `RegexLineValidator.FileMinmatch`, **when** their declared types are inspected, **then** each is typed as `GlobExpressions.Glob`.
- **Given** a glob pattern previously matched via `new Minimatcher(pattern, new Options { AllowWindowsPaths = true, IgnoreCase = true })`, **when** the equivalent `GlobExpressions.Glob` instance is used with case-insensitive and Windows-path options, **then** the match result is identical for all patterns exercised by the existing test suite.
- **Given** `Minimatcher.CreateFilter(pattern, options)` call sites, **when** refactored, **then** each is replaced by an equivalent `GlobExpressions.Glob`-based predicate (`Func<string, bool>`) that produces the same true/false result for all inputs covered by existing tests.
- **Given** the full test suite (`dotnet test`), **when** run after the replacement, **then** all tests that passed before the change continue to pass and no new test failures are introduced.

# Out of Scope

- Upgrading `Glob` beyond v1.1.9 or evaluating other glob libraries.
- Introducing an adapter, wrapper class, or compatibility shim between `Minimatch` and `GlobExpressions`.
- Changing the matching logic, rule behaviour, or any feature beyond the mechanical library swap.
- Adding new tests beyond what is needed to verify the replacement is semantically equivalent.
- Modifying any CI/CD pipeline configuration unrelated to the package swap.

# Assumptions & Constraints

- `Glob` v1.1.9 (NuGet package id `Glob`, by kthompson / `GlobExpressions` namespace) is available on nuget.org and compatible with the target framework used by `mollycoddle.support`.
- `GlobExpressions.Glob` supports case-insensitive matching and treats backslash (`\`) as a path separator (Windows path handling), equivalent to Minimatch's `AllowWindowsPaths = true, IgnoreCase = true` options.
- The three usage patterns (`new Minimatcher(...)`, `Minimatcher.CreateFilter(...)`, `Options { ... }`) cover all call sites; no reflection-based or string-literal references to `Minimatch` types exist.
- The existing test suite provides sufficient coverage to validate semantic equivalence; no new behavioural tests are required as part of this task.
- No external consumers depend on the public `Minimatcher`-typed properties (i.e., `DoesMatch` and `FileMinmatch` are not part of a published API surface consumed outside this repository).
