# Unity Library Cleaner (Windows)

Reclaims disk space by deleting the regenerable `Library` folder from every Unity
project under a directory you choose.

A `Library` folder regularly grows to several gigabytes per project and is always
rebuilt by Unity on the next open, so it is pure cache.

## Usage

1. **Close every Unity project** you are about to clean. Deleting `Library` while
   the editor has the project open can corrupt it.
2. Double-click `UnityLibraryCleaner.bat`.
3. Paste the folder that contains your Unity projects, for example
   `D:\Game Development\Projects`.
4. Type `Y` to confirm.

The script only touches folders that contain **all three** of `Assets`,
`ProjectSettings` and `Library`, so pointing it at the wrong directory will not
delete anything unrelated.

## Safety

- It asks for confirmation before deleting anything.
- It checks for the Unity project signature (`Assets` + `ProjectSettings`) per
  sub-folder rather than deleting every `Library` it finds.
- Only the `Library` folder is removed — never `Assets`, `ProjectSettings`,
  `Packages` or your build output.

If a project is still open, Unity keeps its file handles and the delete for that
one project fails harmlessly; close it and run the script again.

## Not a Unity script

This is a standalone Windows batch file. It is not imported by Unity and can live
anywhere on your machine.
