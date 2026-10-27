# Input System Pinch / Scroll Zoom

Pinch-to-zoom on touchscreens and mouse-wheel zoom on desktop, applied to the main
camera's field of view.

**Requires the [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
package** (`com.unity.inputsystem`). That is why this lives in a sample rather than
the core package — installing the package on a project that still uses the legacy
Input Manager would break its compilation.

## Setup

1. Install **Input System** via Package Manager and set Active Input Handling to
   "Input System Package" or "Both" (Project Settings → Player).
2. Copy `PinchScrollDetection.cs` into your project (outside `Editor/`).
3. Attach it to any GameObject and make sure a camera tagged `MainCamera` exists.

## Settings

| Field | Meaning |
|---|---|
| `speed` | Field-of-view change per scroll unit / pinch pixel. |
| `fieldOfViewLimits` | Clamp range for the camera's field of view. |

## Notes

- Built on the Input System's standalone `InputAction` API, so there is no
  `.inputactions` asset to wire up.
- Actions are disposed in `OnDestroy`, so re-entering Play mode does not leak
  input devices — the original version never released them.
- Two touches are required before a pinch is measured, so a single drag no longer
  nudges the zoom.
- Needs a `Mouse` or `Touchscreen` device; on a device with neither the component
  simply does nothing.
