# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

@AGENTS.md

## Project

«QS: Спецодежда» (Workwear) — a desktop app for tracking the issue of workwear and PPE to employees. It is a GTK# 2 app on Mono/.NET Framework 4.6.2 (x86) that works with a MySQL/MariaDB database through NHibernate. Code comments, UI strings and commit messages are written in Russian.

Git submodules are required: `QSProjects` (the company's shared framework: `QS.*` libraries and the Gamma bindings), `My-FyiReporting` (RDL report engine and GTK viewer) and `RusGuardSharp`. Clone with `--recursive`, or run `git submodule update --init --recursive`.

## Build and test

There are two solutions:
- `Workwear.sln` — the full app (Mono `msbuild`, .NET Framework 4.6.2, Platform `x86`). This includes the GTK project `Workwear/` and the old test project `WorkwearTest/`.
- `Workwear.dotnet.sln` — only the SDK-style projects (`netstandard2.0` libraries plus `netcoreapp3.1` tests). `global.json` pins SDK 3.1.426.

```bash
# restore (the same order CI uses)
nuget restore My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln
nuget restore QSProjects/QSProjectsLib.sln
nuget restore Workwear.sln

# build the app, then run it (./run.sh starts Workwear/bin/Debug/workwear.exe under mono)
msbuild Workwear.sln /p:Configuration=Debug /p:Platform=x86
./run.sh

# main non-SQL tests (NUnit, in-memory SQLite)
dotnet test Workwear.Test/Workwear.Test.csproj --no-restore -v:q
dotnet test Workwear.Test/Workwear.Test.csproj --filter "FullyQualifiedName~<Namespace.ClassOrTest>"

# SQL tests: they start MariaDB/MySQL through Testcontainers (Docker). Run them only when the user explicitly asks (see AGENTS.md)
dotnet test Workwear.Test.Sql/Workwear.Test.Sql.csproj
```

The old UI/ViewModel tests in `WorkwearTest/` target net462:
- `dotnet build` and `dotnet test` on this project do not work. `dotnet test` can return 0 without running any tests.
- Build it through the solution so the platform mappings apply: `msbuild Workwear.sln /t:WorkwearTest /p:Configuration=Debug /p:Platform=x86 /v:minimal`
- Run it from `WorkwearTest/bin/Debug`. The report tests resolve `Integration/Reports/*.rdl` relative to the working directory.
- Command: `mono ~/.nuget/packages/nunit.consolerunner/3.16.3/tools/nunit3-console.exe WorkwearTest.dll --test=<FullyQualifiedName> --workers=0 --labels=Before`
- The NUnit console opens a local TCP socket, and `dotnet test` uses named pipes. Both can fail inside a sandbox, so run them outside it.

`run_operation.sh` is the interactive helper for these same steps. `Workwear.Test/TestResults/` holds generated output, not source.

## Architecture

Layering, from the bottom up:
- **`Workwear.Core`** (netstandard2.0): domain types and logic that don't depend on NHibernate or the UI.
- **`Workwear.Desktop`** (netstandard2.0): the main domain model (`Domain/<Area>`), Fluent NHibernate mappings (`HibernateMapping/<Area>`, `ClassMap<T>`), repositories, models and business services (`Tools/`), and the deletion configuration (`ConfigureDeletion.cs`). Most business logic lives here, because both `Workwear.Test` and the GTK app reference it.
- **`Workwear.Sql`**: DB schema. `Scripts/new_empty.sql` creates a fresh DB, and the versioned `Scripts/X.Y.Z.sql` files are embedded resources, chained in `ScriptsConfiguration.cs` (`MakeCreationScript` holds the current schema version, and `MakeUpdateConfiguration` holds the list of from→to updates). A schema change needs an update script, a matching `AddUpdate` entry, changes to `new_empty.sql`, and a bump of the creation-script version.
- **`Workwear/`** (net462 GTK app, assembly `workwear.exe`): ViewModels and Views (`ViewModels/<Area>` ↔ `Views/<Area>`), journals (`Journal/ViewModels`, `Journal/Filter.ViewModels`/`Filter.Views`, columns in `JournalsColumnsConfigs.cs`), RDL reports (`Reports/`, `ReportsDlg/`), and older dialogs. `gtk-gui/` holds the Stetic designer output.
- `QS.Cloud.*.Client`: gRPC clients for the cloud services (the employee personal account and postomats).
- `Workwear.Gtk`, `Workwear.Startup` and `Workwear.Startup.Core` contain only stale `bin/obj` directories. They are not projects.

Wiring the app together:
- `Program.cs` and `CreateProjectParam.cs` build Autofac containers. `StartupContainer` is used for the login and DB-update stage, and `AppDIContainer` is used for the running app (see `AutofacClassConfig`). New services are registered there.
- MVVM navigation comes from QSProjects: `TdiNavigationManager` opens ViewModels as TDI tabs. Views are found by name convention (`FooViewModel` → `FooView`) through `ClassNamesBaseGtkViewResolver`, and a few views are registered explicitly. Data access goes through `IUnitOfWork` (`QS.DomainModel.UoW`). Entity-change notifications come from `NotifyConfiguration` / `IEntityChangeWatcher`.
- Main menu actions in `MainWindow.cs` open ViewModels through the navigation manager.
- Tests configure NHibernate against in-memory SQLite (`Workwear.Test/ConfigureOneTime.cs`) with `MappingParams.UseIdsForTest = true`, so that entity ids differ across types.

## GTK UI editing (Stetic)

Each view is a partial class. The handwritten part is `Views/.../FooView.cs`, and the generated part is `gtk-gui/Workwear.<Namespace>.FooView.cs`. The generated part is produced from the huge `gtk-gui/gui.stetic`. Any layout change must be made in **both** `gui.stetic` and the generated `.cs`, otherwise regeneration loses it. Find the section with `grep -n "FooView" Workwear/gtk-gui/gui.stetic` and edit that line range. For a worked example, see `docs/ai-guidelines/gtk-gui-editing.md`.

## Documentation

`docs/` holds the Antora (AsciiDoc) user documentation, written in Russian. PDFs are built with `docs/build-pdf.sh`. Writing rules for it are in `docs/AGENTS.md`: use the product name «QS: Спецодежда» in guillemets, use `xref:` for links between pages, and use a narrative style for `modules/practical-guide`.

## Domain notes

- The warehouse forecast has two grouping modes. Grouping by stock nomenclature can split rows by the sizes available in stock. Grouping by norm nomenclature reflects the employee's original demand, so its size and height come from the employee's required size. If no matching stock size is found, rows still show the employee's size.
- `Workwear.Sql/ScriptsConfiguration.cs` may contain local launch changes. Don't touch it unless the change needs it.
