#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Adeel.UnityToolbox.Samples
{
    /// <summary>
    /// Example of migrating a legacy JSON payload ("users" keyed by id) into one
    /// JSON object per line in a new schema.
    /// </summary>
    /// <remarks>
    /// This is a template, not a general converter: replace <see cref="LegacyUser"/>,
    /// <see cref="LegacyDocument"/> and <see cref="MigratedUser"/> with your own
    /// shapes. Requires Newtonsoft Json
    /// (com.unity.nuget.newtonsoft-json) or the package from the Asset Store.
    /// Wrapped in <c>#if UNITY_EDITOR</c> so it stays harmless if placed outside an
    /// Editor folder.
    /// </remarks>
    public static class JsonSchemaMigration
    {
        [MenuItem("Tools/Samples/Migrate JSON Schema")]
        private static void MigrateFromMenu()
        {
            string source = EditorUtility.OpenFilePanel("Legacy JSON file", string.Empty, "json");

            if (string.IsNullOrEmpty(source))
            {
                return;
            }

            string destination = EditorUtility.SaveFilePanel(
                "Save migrated JSON",
                Path.GetDirectoryName(source) ?? string.Empty,
                "migrated",
                "json");

            if (string.IsNullOrEmpty(destination))
            {
                return;
            }

            try
            {
                int count = Migrate(source, destination);
                Debug.Log($"Migrated {count} record(s) to {destination}.");
            }
            catch (Exception exception)
            {
                Debug.LogError($"JSON migration failed: {exception.Message}");
            }
        }

        /// <summary>Reads <paramref name="sourcePath"/> and writes one record per line.</summary>
        /// <returns>The number of records written.</returns>
        public static int Migrate(string sourcePath, string destinationPath)
        {
            string legacyJson = File.ReadAllText(sourcePath);
            LegacyDocument document = JsonConvert.DeserializeObject<LegacyDocument>(legacyJson);

            if (document?.users == null)
            {
                throw new InvalidDataException("The source file has no 'users' object.");
            }

            int written = 0;

            using (StreamWriter writer = new StreamWriter(destinationPath))
            {
                foreach (KeyValuePair<string, LegacyUser> entry in document.users)
                {
                    LegacyUser user = entry.Value;

                    if (user == null)
                    {
                        continue;
                    }

                    var migrated = new MigratedUser
                    {
                        id = entry.Key,
                        email = user.email,
                        language = user.language,
                        parentFirstName = user.parentFirstName,
                        parentLastName = user.parentLastName,
                        noOfChildren = user.noOfChildren,
                        hiddenFieldStatic = user.HiddenfieldStatic,
                        childrenAge = user.childrenAge,
                        country = user.country
                    };

                    writer.WriteLine(JsonConvert.SerializeObject(migrated));
                    written++;
                }
            }

            return written;
        }

        [Serializable]
        private class LegacyUser
        {
            public string email;
            public string language;
            public string parentFirstName;
            public string parentLastName;
            public string noOfChildren;
            public string HiddenfieldStatic;
            public string childrenAge;
            public string country;
        }

        [Serializable]
        private class LegacyDocument
        {
            public Dictionary<string, LegacyUser> users;
        }

        [Serializable]
        private class MigratedUser
        {
            public string id;
            public string email;
            public string language;
            public string parentFirstName;
            public string parentLastName;
            public string noOfChildren;
            public string hiddenFieldStatic;
            public string childrenAge;
            public string country;
        }
    }
}
#endif
