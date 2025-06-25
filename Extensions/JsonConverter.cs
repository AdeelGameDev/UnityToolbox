using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

public class JsonConverter : MonoBehaviour
{
    [SerializeField] private string oldJsonFilePath = "path/to/old_json_file.json";
    [SerializeField] private string newJsonFilePath = "path/to/new_json_file.json";

    void Start()
    {
        ConvertJsonFormat();
    }

    void ConvertJsonFormat()
    {
        // Read the old JSON file
        string oldJsonData = File.ReadAllText(oldJsonFilePath);
        var oldJsonDict = JsonConvert.DeserializeObject<OldJsonFormat>(oldJsonData);

        // Convert to new format and write directly to the file
        using (StreamWriter writer = new StreamWriter(newJsonFilePath))
        {
            foreach (var user in oldJsonDict.users)
            {
                NewJsonFormat newUser = new NewJsonFormat
                {
                    id = user.Key,
                    email = user.Value.email,
                    language = user.Value.language,
                    parentFirstName = user.Value.parentFirstName,
                    parentLastName = user.Value.parentLastName,
                    noOfChildren = user.Value.noOfChildren,
                    HiddenfieldStatic = user.Value.HiddenfieldStatic,
                    childrenAge = user.Value.childrenAge,
                    country = user.Value.country
                };

                // Convert new object to JSON string and write it to the file in a single line
                string newJson = JsonConvert.SerializeObject(newUser);
                writer.WriteLine(newJson);
            }
        }

        Debug.Log("JSON data converted and overwritten in the new format.");
    }

    // Old JSON format classes
    [System.Serializable]
    private class OldUser
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

    [System.Serializable]
    private class OldJsonFormat
    {
        public Dictionary<string, OldUser> users;
    }

    // New JSON format class
    [System.Serializable]
    private class NewJsonFormat
    {
        public string id;
        public string email;
        public string language;
        public string parentFirstName;
        public string parentLastName;
        public string noOfChildren;
        public string HiddenfieldStatic;
        public string childrenAge;
        public string country;
    }
}
