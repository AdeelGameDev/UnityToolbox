# 🚀 Unity Keystore Password Automation Guide  

This guide explains how to automate setting Android keystore passwords in Unity using a **C# script** and a **batch file (`.bat`)**.  

---

## 🛠 Step 1: Import the Script into Unity  
1️⃣ Open your **Unity project**.  
2️⃣ Navigate to the **Assets folder** and create a new folder named **Editor** (if not already there).  
3️⃣ Copy **`KeystorePasswordSetter.cs`** into the `Editor` folder.  
4️⃣ Unity will automatically detect and compile the script.  

---

## 🖥 Step 2: Configure the Batch File (`LaunchUnity.bat`)  

### ✏️ Edit the `.bat` File  
1️⃣ Open **`LaunchUnity.bat`** in a text editor.  
2️⃣ Update the **Unity Editor path** to match your installed version.  
3️⃣ Update the **Unity project path** to match your project directory.  
4️⃣ Save the file and double-click to launch Unity with automated keystore settings.  

---

## 🔄 How It Works  
✅ **Import `KeystorePasswordSetter.cs`** into Unity under `Editor/`.  
✅ **Run `LaunchUnity.bat`**, which:  
   - Sets environment variables (`UNITY_KEYSTORE_PASS`, `UNITY_KEYALIAS_PASS`).  
   - Launches Unity with the specified project.  
✅ **Unity Executes the Script**, which:  
   - Reads the environment variables.  
   - Applies the passwords to **Android Player Settings**.  
✅ **Your Android keystore is now automatically set up!** 🎉  

---

## 📌 Notes  
✔ **The script must be in the `Editor` folder** to work correctly.  
✔ **Always update the Unity path** in the `.bat` file before running it.  
✔ **This method keeps passwords secure** by avoiding hardcoding them in Unity.  

Now you can build Android apps without manually entering keystore passwords! 🚀
