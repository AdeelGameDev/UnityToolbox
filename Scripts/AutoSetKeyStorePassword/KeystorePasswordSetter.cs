using System;
using UnityEditor;

public static class KeystorePasswordSetter
{
    [InitializeOnLoadMethod]
    private static void SetPasswords()
    {
        // Retrieve passwords from environment variables
        string keystorePass = Environment.GetEnvironmentVariable("UNITY_KEYSTORE_PASS");
        string keyaliasPass = Environment.GetEnvironmentVariable("UNITY_KEYALIAS_PASS");

        if (!string.IsNullOrEmpty(keystorePass))
            PlayerSettings.Android.keystorePass = keystorePass;

        if (!string.IsNullOrEmpty(keyaliasPass))
            PlayerSettings.Android.keyaliasPass = keyaliasPass;
    }
}