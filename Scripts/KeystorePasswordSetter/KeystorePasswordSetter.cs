using System;
using UnityEditor;

public static class KeystorePasswordSetter
{
    // This method runs automatically when Unity starts or the editor loads.
    [InitializeOnLoadMethod]
    private static void SetPasswords()
    {
        // Retrieve keystore and key alias passwords from environment variables.
        // These environment variables are set in the .bat file.
        string keystorePass = Environment.GetEnvironmentVariable("UNITY_KEYSTORE_PASS");
        string keyaliasPass = Environment.GetEnvironmentVariable("UNITY_KEYALIAS_PASS");

        // If the keystore password is found, apply it to Unity's Android build settings.
        if (!string.IsNullOrEmpty(keystorePass))
            PlayerSettings.Android.keystorePass = keystorePass;

        // If the key alias password is found, apply it as well.
        if (!string.IsNullOrEmpty(keyaliasPass))
            PlayerSettings.Android.keyaliasPass = keyaliasPass;
    }
}
