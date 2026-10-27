# Android Keystore Password Automation

Stops you from retyping the keystore and key-alias passwords on every Android
build, without ever putting those passwords in the project or in git.

## The problem

Unity forgets the Android keystore password between sessions. Typing it before
every build is tedious, and the obvious fix — hardcoding it — means a password in
your repository.

## The approach

1. A `.bat` file exports the passwords as environment variables and launches Unity.
2. An editor script reads those variables on load and applies them to
   `PlayerSettings.Android.keystorePass` / `keyaliasPass`.

The passwords live in your shell environment (or in an ignored local file), never
in source control.

## Setup

1. Copy `KeystorePasswordSetter.cs` into an **`Editor`** folder in your project.
2. Copy `LaunchUnity.bat` anywhere outside your `Assets` folder.
3. Edit the two paths at the top of the `.bat`:
   - `UNITY_EDITOR_PATH` — your installed `Unity.exe`
   - `UNITY_PROJECT_PATH` — your project folder
4. Create `local-secrets.bat` next to `LaunchUnity.bat`:

   ```bat
   set "UNITY_KEYSTORE_PASS=your-keystore-password"
   set "UNITY_KEYALIAS_PASS=your-key-alias-password"
   ```

5. Make sure `local-secrets.bat` is covered by your `.gitignore` (Unity Toolbox's
   own `.gitignore` already includes it), then double-click `LaunchUnity.bat`.

## Notes

- **Never commit the passwords.** The earlier version of this sample shipped a
  `.bat` with a password inline — a placeholder there, but a dangerous habit.
  Keep them in `local-secrets.bat` or your environment.
- The editor script only writes values it actually finds, so launching Unity
  normally still works and leaves your settings untouched.
- Unity keeps these values in memory only; you will run the `.bat` again next
  session. That is the intended trade-off.
