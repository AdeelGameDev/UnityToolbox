# Contributing to Unity Toolbox

Thanks for taking the time to contribute. This repository is a collection of
small, focused Unity scripts — the bar is usefulness and clarity, not size.

## Ways to contribute

- **Report a bug** — open an issue with the Unity version, the platform, and what
  you expected to happen.
- **Suggest a tool** — open an issue describing the problem you keep solving by
  hand. Repetitive manual work is exactly what belongs here.
- **Send a fix or a new tool** — open a pull request.

## Ground rules for a tool

1. **One problem per tool.** A script that does one thing well beats a framework.
2. **No unguarded third-party dependencies.** Anything needing Firebase, ZXing,
   Newtonsoft, the Input System or TextMeshPro ships as a sample under
   `Samples~`, not in the core package. The core package must compile in a fresh
   Unity project with no extra packages installed.
3. **`UnityEditor` belongs in `Editor/`.** A file under `Runtime/` that references
   `UnityEditor` breaks player builds. CI fails the build if it finds one.
4. **Namespace everything.** Code in `Runtime/` uses `Adeel.UnityToolbox`, code in
   `Editor/` uses `Adeel.UnityToolbox.EditorTools`.
5. **Class name matches file name** for anything deriving from `MonoBehaviour` or
   `ScriptableObject`.
6. **Document the intent.** A short XML summary on the type, and a README in the
   folder if the tool needs setup.

## Layout

```
Runtime/     Core runtime scripts  -> shipped in the package
Editor/      Core editor tooling   -> shipped in the package
Samples~/    Optional tools        -> imported on demand from Package Manager
Documentation~/  Images and long-form docs (ignored by Unity)
```

## Commit messages

Start with a verb, keep it in the imperative, and make it describe the change:

```
Add QRCodeScanner sample
Fix Singleton duplicate-instance handling
Move AnchorSmartFixer into Editor folder
```

One logical change per commit. It keeps the history readable and the blame useful.

## Pull requests

- Keep the diff focused on one thing.
- Run the **Validate** workflow locally if you can: it checks that no
  `using UnityEditor` crept into `Runtime/`, that `package.json` parses, and that
  every script has a `.cs` extension.
- Note any breaking change in `CHANGELOG.md`.

## License

By contributing you agree that your work is released under the
[MIT License](LICENSE).
