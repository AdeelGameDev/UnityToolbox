# Audio Manager

Plays named sound effects and background music from one persistent object, with
no third-party dependencies.

## What is in here

| File | Purpose |
|---|---|
| `AudioManager.cs` | Owns every `AudioSource` and exposes `Play(name)` / `Stop(name)`. |
| `Sound.cs` | Serializable entry: clip, volume, pitch, loop, name. |
| `AudioManager.prefab` | Pre-wired object. Drop it into your first scene. |

## Setup

1. Drag `AudioManager.prefab` into the first scene of your project.
2. Select it and expand **Sounds** in the Inspector.
3. Raise the list size and fill in each entry: a **name**, an **AudioClip**, and
   the volume / pitch / loop settings.

## Usage

```csharp
AudioManager.instance.Play("Explosion");
AudioManager.instance.Play("Music");
AudioManager.instance.Stop("Music");
```

The object is marked `DontDestroyOnLoad`, so music keeps playing across scene
loads and duplicate instances destroy themselves.

## Notes

- Each entry gets its own `AudioSource`, so several clips can play at once.
- Entries with no clip are skipped with a warning instead of throwing.
- The `Sound` and `AudioManager` types live in the `Adeel.UnityToolbox.Samples`
  namespace — add `using Adeel.UnityToolbox.Samples;` or fully qualify them.
