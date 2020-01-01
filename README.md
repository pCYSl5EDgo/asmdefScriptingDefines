# asmdef Scripting Defines

`asmdef Scripting Defines` is an Unity Editor Extension that let you add/remove `Scripting Define Symbols` to/from each of the `Assembly Definition`s separately.

This repository consists of three C# projects.

 - asmdefDefineSymbols 
 - asmdefDefineSymbols.Editor
 - asmdefScriptingDefines.Extension

# Required software & Supported Unity version(Environments)

 - **Required** : .NET Core 2.1 or above
 - **Supported** : Unity 2018.4

# Background

`Scripting Define Symbols` of Unity Editor PlayerSettings are per platform settings.<br/>
Sometimes I want to add custom `Scripting Define Symbols` to the specific `Assembly Definition File` not to the entire Unity project.

# Easy way to install

If you do not want to build whole solution for yourself. Go to [BOOTH](https://pcysl5edgo.booth.pm/items/1756645) and Buy it with 900 Japanese yen.

# Installation for Unity Editor

Please substitute the paths for your correct paths.<br/>
Open your powershell with appropriate authority.

```powershell
git clone https://github.com/pCYSl5EDgo/asmdefScriptingDefines
cd asmdefScriptingDefines
dotnet new sln
dotnet sln add asmdefDefineSymbols
dotnet sln add asmdefDefineSymbols.Editor
dotnet restore
dotnet build -c Release
Copy-Item "asmdefDefineSymbols.Editor/bin/Release/netstandard2.0/UnityEditor.ForCuteIzmChan.dll" "C:/Program Files/Unity/Hub/Editor/2018.4.14f1/Editor/Data/Managed/UnityEditor.ForCuteIzmChan.dll"
cd asmdefDefineSymbols
dotnet run -c Release execute "C:/Program Files/Unity/Hub/Editor/2018.4.14f1/Editor/Data/Managed"
```

# How to use with your Unity Project

Provide that you have the project that has following structure.

```
- Assets
 - Scripts
  - FolderA
   - A.asmdef
   - Class1_A.cs
   - Class2_A.cs
   - Class3_A.cs
  - FolderB
   - B.asmdef
   - Interface1_B.cs
   - Struct1_B.cs
- Plugins
 - ~~~~~~~~~
```

When you want to add `Scripting Define Symbols` to `A.asmdef`.<br/>
First, you should create a new text file named `A.asmdef_defines` in `Assets/Scripts/FolderA`.<br/>
It must be located in the same directory. It must have the same name without extension and file extension of `asmdef_defines`.<br/>
Sample code of `A.asmdef_defines`.

```
+CSHARP_8_OR_NEWER
-RELEASE
/DEBUG
#This is a comment
```

 - `+` : Added to defines.
 - `-` : Removed from defines.
 - `/` : Place-holder.
 - `#` : Comment.

Multi-line comment is not supported.<br/>
Each line should starts with any of those four `Command Letter`s.<br/>
Next to a `Command Letter`, a `Define Symbol` follows.
