using System;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

namespace Adeel.UnityToolbox.Samples
{
    /// <summary>
    /// Email/password authentication with a per-user role kept in the Realtime
    /// Database.
    /// </summary>
    /// <remarks>
    /// Requires the Firebase Unity SDK. Every callback is marshalled back onto the
    /// main thread, so Unity APIs are safe to use inside them.
    /// </remarks>
    public class FirebaseManager : MonoBehaviour
    {
        public static FirebaseManager Instance { get; private set; }

        private FirebaseAuth _auth;
        private bool _ready;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result != Firebase.DependencyStatus.Available)
                {
                    Debug.LogError($"FirebaseManager: dependencies unavailable — {task.Result}");
                    return;
                }

                _auth = FirebaseAuth.DefaultInstance;
                _ready = true;
            });
        }

        /// <summary>Creates an account and stores its role in the database.</summary>
        public void RegisterUser(
            string email,
            string password,
            string role,
            Action<string> onSuccess = null,
            Action<string> onError = null)
        {
            if (!_ready)
            {
                onError?.Invoke("Firebase is not ready yet. Wait for initialisation to finish.");
                return;
            }

            _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    onError?.Invoke(Describe(task.Exception));
                    return;
                }

                string userId = task.Result.User.UserId;
                SetUserRole(userId, role, () => onSuccess?.Invoke(userId), onError);
            });
        }

        /// <summary>Signs in and reads the account's role from the database.</summary>
        public void SignInUser(
            string email,
            string password,
            Action<string, string> onSuccess = null,
            Action<string> onError = null)
        {
            if (!_ready)
            {
                onError?.Invoke("Firebase is not ready yet. Wait for initialisation to finish.");
                return;
            }

            _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    onError?.Invoke(Describe(task.Exception));
                    return;
                }

                string userId = task.Result.User.UserId;
                GetUserRole(userId, role => onSuccess?.Invoke(userId, role), onError);
            });
        }

        private void SetUserRole(string userId, string role, Action onSuccess, Action<string> onError)
        {
            UserReference(userId).Child("role").SetValueAsync(role).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    onError?.Invoke(Describe(task.Exception));
                    return;
                }

                onSuccess?.Invoke();
            });
        }

        private void GetUserRole(string userId, Action<string> onSuccess, Action<string> onError)
        {
            UserReference(userId).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    onError?.Invoke(Describe(task.Exception));
                    return;
                }

                DataSnapshot snapshot = task.Result;

                if (!snapshot.HasChildren || snapshot.Child("role").Value == null)
                {
                    onError?.Invoke("This account has no role stored in the database.");
                    return;
                }

                onSuccess?.Invoke(snapshot.Child("role").Value.ToString());
            });
        }

        // Uses the default app from your Firebase config file, so no URL is hardcoded.
        private static DatabaseReference UserReference(string userId)
        {
            return FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(userId);
        }

        private static string Describe(AggregateException exception)
        {
            return exception?.InnerException?.Message ?? exception?.Message ?? "Unknown Firebase error.";
        }
    }
}
