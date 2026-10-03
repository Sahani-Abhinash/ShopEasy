# Sprint 0 — Repository and tooling

**Status:** ☑ Done
**Started:** 2026-10-03
**Finished:** 2026-10-03
**Guide:** [Chapter 1](../architecture-guide/01-requirements-and-architecture.md) · **Backlog:** [Sprint 0](../planning/sprint-backlog.md#sprint-0--repository-and-tooling)

**Goal:** an empty but well-structured repository with CI running on every PR.

---

## 1. Tasks

| # | Task | Owner | Status |
|---|---|---|---|
| 1 | Install and verify tools | 👤 | ☑ Done (old Terraform/kubectl copies in PATH: cleanup optional) |
| 2 | `git init` locally | 👤 | ☑ Done |
| 3 | `.gitignore`, `.gitattributes` | 🤖 | ☑ Done (`git ls-files` shows no `bin/`/`obj/`) |
| 4 | Folder structure | 🤖 | ☑ Done |
| 5 | Solution + `Directory.Build.props` + `Directory.Packages.props` | 🤖 | ☑ Done (build succeeded) |
| 6 | `.editorconfig` + `dotnet format` check | 🤖 / 👤 | ☑ Done (`--verify-no-changes` clean) |
| 7 | `ShopEasy.ServiceDefaults` project | 🤖 | ☑ Done (builds with 0 warnings) |
| 8 | Root `README.md` | 🤖 | ☑ Done |
| 9 | ADR template + ADRs 0001–0003 | 🤖 | ☑ Done |
| 10 | CI workflow `ci.yml` | 🤖 | ☑ Done (run #1 green; deprecation warnings fixed in follow-up) |
| 11 | First commit, GitHub repository, push | 👤 | ☑ Done: `Sahani-Abhinash/ShopEasy` (private) |
| 12 | Branch protection on `main` | 👤 | ◐ Deferred: process-only while private; enforce when made public |

---

## 2. Work log

### 2026-10-03 — Tool verification (task 1)

**What we did:** ran version checks for all tools.

| Tool | Result | Status |
|---|---|---|
| .NET SDK | 10.0.301 | ✅ |
| Node.js / npm | v24.11.1 / 11.6.2 | ✅ |
| Git | 2.52.0 | ✅ |
| Docker / Compose | 29.6.2 / v5.3.1 | ✅ installed, running state to confirm (`docker info`) |
| Azure CLI | 2.87.0 (updates available) | ✅ |
| Terraform, kubectl, kind, Helm | No output | ❓ Probably not installed |

**What was missing:** Terraform, kubectl, kind, Helm (to be confirmed).

**Fix / next action:** run the version checks again separately; install missing tools with `winget` (Hashicorp.Terraform, Kubernetes.kubectl, Kubernetes.kind, Helm.Helm). Not needed until Sprints 9–10, so this doesn’t block Sprint 0.

### 2026-10-03 — Installing the missing tools (task 1)

**What we did:** installed with `winget`:

| Tool | Version | Note |
|---|---|---|
| Terraform | 1.16.4 | **Was already installed**—winget upgraded it |
| kubectl | 1.37.1 | New install |
| Helm | 4.3.0 | New install |
| kind | 0.33.0 | New install |

**What was missing:** kubectl, Helm, kind. Terraform was present, so the first check was misleading (see Issues).

**Fix / next action:** open a **new terminal** (PATH refresh) and re-run the version checks; confirm Docker Desktop is running with `docker info`.

### 2026-10-03 — Verification in a new terminal (task 1)

**What we did:** re-ran the version checks.

| Tool | Expected (winget) | Actually runs | Status |
|---|---|---|---|
| Terraform | 1.16.4 | **v1.15.2** | ⚠ older copy earlier in PATH |
| kubectl | 1.37.1 | **v1.36.1** (Kustomize v5.8.1) | ⚠ older copy earlier in PATH (likely Docker Desktop’s) |
| kind | 0.33.0 | 0.33.0 | ✅ |
| Helm | 4.3.0 | v4.3.0 | ✅ |
| Docker | running | `linux / 29.6.2` | ✅ Docker Desktop running, Linux containers |

**What was missing:** the new Terraform and kubectl are installed but not the ones being used.

**Fix / next action:** `where.exe terraform` and `where.exe kubectl` to find the older copies. Not blocking: both versions work for this project, and they aren’t used until Sprints 9–10.

### 2026-10-03 — Local repository (task 2)

**What we did:** `git init -b main`; `git status` showed only `docs/` untracked. No commit yet, on purpose: `.gitignore` must exist before the first commit.

### 2026-10-03 — Repository files (tasks 3–10)

**What we did:** Claude created:

| File | Purpose |
|---|---|
| `.gitignore` | Ignores build output, secrets (`.env`, `secrets.json`), Terraform state, `node_modules` |
| `.gitattributes` | LF line endings in the repo (CRLF for `.ps1`/`.cmd`) |
| `global.json` | Pins .NET SDK 10.0.301 (`rollForward: latestFeature`) |
| `Directory.Build.props` | net10.0, nullable, implicit usings, warnings as errors, analyzers, code style in build |
| `Directory.Packages.props` | Central package management (versions in one place) |
| `.editorconfig` | Formatting and naming rules (file-scoped namespaces, `_camelCase` fields) |
| `ShopEasy.slnx` | Solution in the new XML format (.NET 10 default) |
| `src/BuildingBlocks/ShopEasy.ServiceDefaults` | Empty `AddServiceDefaults()` / `MapDefaultEndpoints()` |
| `tests/`, `deploy/`, `infrastructure/` | Empty folders kept with `.gitkeep` |
| `README.md` | Purpose, layout, prerequisites, build commands |
| `docs/adr/0000–0003` | ADR template + microservices, database per service, orchestrated saga |
| `.github/workflows/ci.yml` | On PR/push to main: restore → format check → build → test → upload results |

**What was missing / changed:** the first CI draft used NuGet caching with `packages.lock.json`. We don’t have lock files, so the setup step would fail. Removed caching for now (to revisit when packages are added).

**Next action:** 👤 verify locally (`dotnet build`, `dotnet format --verify-no-changes`, `git status`), then first commit.

### 2026-10-03 — Local verification (tasks 3–7)

**What we did:** `dotnet format --verify-no-changes` returned no output (clean). `git status` lists only the expected files and folders.

**What was missing:** the `dotnet build` output wasn’t captured, so the build is not verified yet. `.gitignore` can only be fully checked after a build creates `bin/` and `obj/`.

**Next action:** 👤 run `dotnet build ShopEasy.slnx` and `git status` again.

**Result:** `dotnet build` succeeded in 3.8 s with no warnings: restore, central package management, `global.json`, and the analyzers all work. `git status` is unchanged after the build. But git shows only the top-level `src/` folder, so whether `bin/` and `obj/` are ignored is confirmed at `git add` time, when every file is listed.

### 2026-10-03 — First push and CI run (tasks 10–11)

**What we did:** first commit, created the **private** repository `github.com/Sahani-Abhinash/ShopEasy`, pushed `main`. CI run #1 **succeeded in 27 s**: checkout, .NET setup from `global.json`, restore, format check, build, test, upload.

**What was missing:** the run showed two annotations:

| Annotation | Meaning | Fix |
|---|---|---|
| ⚠ `actions/upload-artifact@v4` targets deprecated Node.js 20 | The action will stop working when GitHub removes Node 20 | Upgraded to `@v5` |
| ℹ `ubuntu-latest` moves to Ubuntu 26 from 2026-10-19 | The runner OS could change under us without notice | Pinned `runs-on: ubuntu-24.04`; upgrade on purpose later |

**Next action:** 👤 commit the CI fix through a **pull request** (this also tests the PR trigger), then set up branch protection.

### 2026-10-03 — PR #1 and sprint close (demo)

**What we did:** created branch `chore/ci-runner-and-artifact`, opened **PR #1**, the `build-and-test` check passed, merged into `main` (commit `2d54978`), pulled locally. Verified that no `bin/` or `obj/` files are tracked.

**What was missing:** the remote branch was not deleted after the merge (the "Delete branch" button is still shown).

**Fix / next action:** 👤 click **Delete branch** on PR #1, and locally run `git branch -d chore/ci-runner-and-artifact`.

---

## 3. Issues and fixes

| Date | Problem | Cause | Fix |
|---|---|---|---|
| 2026-10-03 | Terraform/kubectl/kind/Helm printed nothing in the first check | Output stopped after `az --version`. Terraform turned out to be installed, so it wasn’t (only) missing tools: `az` is a batch script (`az.cmd`), and pasting many commands at once can lose the lines after it | Run tool checks one per line, or `az` last. kubectl, Helm, kind were genuinely missing → installed with winget |
| 2026-10-03 | Terraform shows 1.15.2 and kubectl 1.36.1 after installing 1.16.4 / 1.37.1 | Another copy of each appears earlier in PATH (kubectl is likely the one bundled with Docker Desktop) | `where.exe` showed: `C:\terraform\terraform.exe` (old manual install) and `C:\Program Files\Docker\Docker\resources\bin\kubectl.exe` (Docker Desktop) come before `WinGet\Links`. The **system** PATH is searched before the **user** PATH, where winget adds its links. Fix: delete `C:\terraform` and remove it from the system PATH. Keep Docker’s kubectl (one minor version older is supported, and Docker Desktop updates it) |
| 2026-10-03 | CI warning: Node.js 20 deprecated (`upload-artifact@v4`); notice: `ubuntu-latest` → Ubuntu 26 | Action and runner image versions move on | `upload-artifact@v5`; runner pinned to `ubuntu-24.04` |

## 4. Decisions made in this sprint

| Decision | Why |
|---|---|
| Default branch `main` (`git init -b main`) | Modern convention; matches CI and branch protection |
| `.slnx` solution format | Default in .NET 10; readable XML, fewer merge conflicts than `.sln` |
| Central package management | One place for all NuGet versions across services |
| Warnings as errors + code style in build | Quality problems fail the build early, locally and in CI |
| `global.json` pins the SDK | Same SDK locally and in CI |
| Repository stays **private** for now; made public later | Branch rules aren’t enforced on private repos with GitHub Free. Until it’s public, the rule “every change goes through a PR with green CI; no direct pushes to `main`” is followed by discipline. When made public: set up the ruleset (PR required, `build-and-test` check required, no force push, no deletion) |
| 👤/🤖 ownership rule | User does all tool and service configuration personally (learning goal); Claude writes repository files |

## 5. What I learned

- _(your own notes; suggestions below)_
- `.slnx`, `Directory.Build.props`, and central package management keep settings in one place for all services.
- Windows searches the system PATH before the user PATH, so old tool copies can win over winget installs.
- CI annotations matter: deprecated runtimes and moving `-latest` runner labels break pipelines later. Pin versions and upgrade on purpose.
- GitHub Free: private repos are free, but branch rules are only enforced on public repos (or with Pro).

## 6. Sprint demo

- [x] A PR with a trivial change shows a green CI check and can be merged (PR #1)
