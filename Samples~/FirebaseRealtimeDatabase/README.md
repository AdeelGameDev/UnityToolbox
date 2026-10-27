# Firebase Auth + Realtime Database

Email/password registration and sign-in where each account also carries a role
stored in the Realtime Database.

**Requires the [Firebase Unity SDK](https://firebase.google.com/docs/unity/setup)**
(`FirebaseAuth` and `FirebaseDatabase`). Nothing else in Unity Toolbox depends on
Firebase — this sample is opt-in for exactly that reason.

## Setup

1. Follow the Firebase Unity setup guide and drop the SDK into your project.
2. Add your `google-services.json` / `GoogleService-Info.plist` and let the
   Firebase SDK create the default app.
3. Enable **Email/Password** in Firebase Console → Authentication → Sign-in method.
4. Create a Realtime Database and set rules that let an authenticated user write
   their own `/users/{uid}/role` node.
5. Copy `FirebaseManager.cs` into your project (any folder outside `Editor/`).

No database URL is hardcoded: `FirebaseDatabase.DefaultInstance` uses the app
configured from your Firebase config file.

## Usage

```csharp
FirebaseManager.Instance.RegisterUser(
    email, password, role,
    onSuccess: userId => Debug.Log($"Registered {userId}"),
    onError:   error  => Debug.LogError(error));

FirebaseManager.Instance.SignInUser(
    email, password,
    onSuccess: (userId, role) => Debug.Log($"{userId} signed in as {role}"),
    onError:   error  => Debug.LogError(error));
```

## Notes

- All Firebase callbacks are marshalled onto the main thread with
  `ContinueWithOnMainThread`, so touching Unity APIs inside them is safe.
- Roles are strings you define. The sample does not assume "Student"/"Teacher"
  like the original version did.
- Network failures return a message through the callbacks instead of silently
  throwing on a background thread.
