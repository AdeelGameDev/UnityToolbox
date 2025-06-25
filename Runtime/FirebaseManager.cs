using UnityEngine;
using Firebase;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using Firebase.Database;

public class FirebaseManager : SingletonPersistent<FirebaseManager>
{
    private FirebaseAuth auth;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
        });
    }

    public void RegisterUser(string email, string password, string role)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Failed to register user");
                return;
            }

            Debug.Log("User registered successfully!");

            // Store the user's role in the database
            string userId = task.Result.User.UserId;
            Debug.Log("user id " +userId);
            SetUserRole(userId, role);
            Debug.Log("user id " +userId);

            // Perform role-specific logic here
            if (role.Equals("Student"))
            {
                // Do something for students
            }
            else if (role.Equals("Teacher"))
            {
                // Do something for teachers
            }

            SceneManager.LoadScene(2);
        });
    }

    public void SignInUser(string email, string password, System.Action<string, string> callback)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                callback.Invoke(null, "Failed to sign in");
                return;
            }

            FirebaseUser user = task.Result.User;
            string userId = user.UserId;

            GetRoleFromUserId(userId, role =>
            {
                callback.Invoke(userId, role);
            });
        });
    }

    private void SetUserRole(string userId, string role)
    {
        // Assume you're using Firebase Realtime Database
        string databaseUrl = "https://projectar-80ba0-default-rtdb.firebaseio.com/"; // Replace with your Firebase Realtime Database URL
        DatabaseReference userRef = FirebaseDatabase.GetInstance(databaseUrl).RootReference.Child("users").Child(userId);
        userRef.Child("role").SetValueAsync(role);
    }

    private void GetRoleFromUserId(string userId, System.Action<string> callback)
    {
        // Assume you're using Firebase Realtime Database
        string databaseUrl = "https://projectar-80ba0-default-rtdb.firebaseio.com/"; // Replace with your Firebase Realtime Database URL
        DatabaseReference userRef = FirebaseDatabase.GetInstance(databaseUrl).RootReference.Child("users").Child(userId);


        userRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.Result.HasChildren)
            {
                Debug.LogError("User role not found in the database");
                callback.Invoke("");
                return;
            }

            DataSnapshot snapshot = task.Result;
            string role = snapshot.Child("role").Value.ToString();

            // Now you have the user's role
            // Perform any necessary logic based on the role

            callback.Invoke(role);
        });
    }
}
