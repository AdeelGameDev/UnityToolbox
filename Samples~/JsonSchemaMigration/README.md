# JSON Schema Migration

An **example**, not a general-purpose converter. It reads a legacy JSON file laid
out as `{ "users": { "<id>": { ... } } }` and writes one flattened object per
line in a new schema.

**Requires Newtonsoft Json** — either `com.unity.nuget.newtonsoft-json` (add by
name in the Package Manager) or the Asset Store package.

## Why it is a sample and not part of the package

The field names (`parentFirstName`, `childrenAge`, `noOfChildren`, …) come from one
specific project. Nothing about them is reusable, so shipping this inside the
package would only bloat it and mislead you.

## How to adapt it

1. Copy `JsonSchemaMigration.cs` into any `Editor` folder in your project.
2. Replace the three private classes at the bottom with your own source and
   destination shapes.
3. Update the mapping block inside `Migrate`.
4. Run **Tools → Samples → Migrate JSON Schema** and pick your files.

## Notes

- The whole file is wrapped in `#if UNITY_EDITOR`, so it is harmless even if it
  ends up outside an `Editor` folder.
- The class is called `JsonSchemaMigration` rather than `JsonConverter` on
  purpose: `JsonConverter` collides with `Newtonsoft.Json.JsonConverter` and
  produces ambiguous-reference errors.
- Read errors surface as a single logged message instead of an unhandled
  exception from `Start()` — the original version could take down the scene load.
