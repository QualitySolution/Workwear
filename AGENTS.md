# Agent Instructions

- Do not run SQL tests without explicit user confirmation and without the database being prepared/fixed for them.
- For routine verification, prefer targeted builds or non-SQL tests unless SQL test execution is specifically requested.
- Edit Gtk UI layout through Stetic (`gtk-gui/gui.stetic`) and keep generated `gtk-gui/*.cs` files in sync; do not add UI widgets only from handwritten view code.
- Prefer the smallest simple fix that reuses existing code and changes as little as possible. Especially for bug fixes, do not broaden the change without a demonstrated need; larger diffs increase regression risk and require correspondingly stronger justification and verification.
