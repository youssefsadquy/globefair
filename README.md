# GlobeFair

> **University of Leicester final-year project (October 2023 to May 2024).**
> This is the submitted version, migrated from [GitLab](https://gitlab.com/youssefsadquy/globefair) with its commit history and original dates.
> A newer **web version** (WebGL, hosted on Firebase) is in [globefair-web](https://github.com/youssefsadquy/globefair-web), with a live demo at <https://globefair-46eb2.web.app/>.

## About this copy

Before publishing, these were removed from every commit:

- Firebase configuration files (`google-services.json`, `GoogleService-Info.plist`, `google-services-desktop.json`, `google-services.xml`) and a hard-coded Google Cloud API key in `TranslationManager.cs` (now `YOUR_GOOGLE_TRANSLATE_API_KEY`).
- Unity caches (`Library`, `Temp`, `Logs`, `UserSettings`) and the built `Executable` folder and zip.
- Every file larger than 5 MB, because of GitHub's size limits. This mainly affects large 3D models, textures and audio. The complete project is still on GitLab.

To run it you need your own Firebase project: add its config files under `Assets/` (and `Assets/StreamingAssets/`), and your own translation API key in `TranslationManager.cs`.

## Third-party content

The project uses Unity packages and asset packs, each with its own licence: TextMesh Pro, Unity Starter Assets, the Firebase Unity SDK, UMA, StandaloneFileBrowser, and the Exhibition and MinimalistBedroom packs. The repository owner states that the asset packs were bought with the right to redistribute them as free or open material. Check each pack's own licence terms before reusing it.

## Project Description
Globefair, has been influenced by the fast evolving world and digitalisation of almost every dailay aspect of life. It was born from the idea of creating a powerful software capable of reinventing the traditional career fair into a more sophisticated and enjoyable virtual experience. Unity has been the core system of the project, allowing to build the system and to redefine the different aspect of networking and career opportunities without border limits.

## Folder Structure 

The project files for GlobeFair are organized within several directories under the main folder `GlobeFair_Deliverables`. Here's a breakdown of the main components: 
-  **GlobeFair_Unity/GlobeFair/**: 
   - **Assets/**: Contains all the necessary assets for the project. 
     - **Scenes/**: Includes all the Unity scene files. Key scenes include: 
       - `GlobeFair.unity`: The main scene for the GlobeFair experience. 
       - `Landing.unity`: The initial scene where users land. 
       - `PlayerCustomiser.unity`: The initial scene where users land. 
      - **Scripts**: Organized by scene usage. 
        - `Assets/Scripts`: Scripts for the 'Landing' scene.
        - `Assets/Exhibition/Scripts`: Scripts specific to the 'GlobeFair' scene. 
        - `Assets/Customiser/Scripts`: Scripts for the 'PlayerCustomiser' scene.
       - **AnimationTutorial/**:  
         -  Stores all animations used within the project. 

		- **Audio/**: 
			- Holds audio files used for environmental sounds across different scenes. 
		- **Images/**:
			-  Includes icons, background images, and other media used throughout the application. 

     - **Exhibition/**: 
       - **Models/Prefabs**: Contains prefabs models used throughout the GlobeFair scene. 

- **Executable/**: 
	- `GlobeFair.exe`: Executable file for the project. The whole folder needs to be downloaded and extracted to run the software.

## Running the Software  
1. Clone the repository or download the project to your local machine. 
2. Navigate to the `GlobeFair_Deliverables/Executable.zip` directory within the cloned or downloaded project and extract. 
3. Locate and run `GlobeFair.exe` to launch the application.

## System Requirements
- **Operating System**: Windows 10, x64
- **Unity Hub Version**: 3.7.0
- **Unity Editor Version**: 2021.3.26f1
- **Disk Space**: 6 GB free space required for the executable

## Opening the Project in Unity Hub
To open the GlobeFair project in Unity Hub:

1. **Download the Project**: Ensure that the entire `GlobeFair_Deliverables/GlobeFair_Unity/GlobeFair` folder is downloaded to your local machine.
2. **Open Unity Hub**: Launch Unity Hub on your computer.
3. **Add the Project**: In Unity Hub, navigate to the 'Projects' tab. Click on the 'Add' button located in the upper right corner of the tab.
4. **Locate the Project Folder**: Browse to the location where you downloaded the `GlobeFair_Deliverables/GlobeFair_Unity/GlobeFair` folder and select it. This will add the project to your Unity Hub's project list.
5. **Open the Project**: Click on the project name in Unity Hub to open it in the specified version of Unity (ensure you have Unity Editor Version 2021.3.26f1 installed as it matches the project’s requirements).


## Additional Information
- **Scenes**: Main scenes such as GlobeFair and Landing are located under `Assets/Scenes`.
- **Player Customisation**: The PlayerCustomiser scene can be found in `Assets/Customiser`.
- **Prefabs**: Available in `Assets/Exhibition/Models/Prefabs`.
- **Animations**: Stored in `Assets/AnimationTutorial`.
- **Audio**: Environmental audio files are located in `Assets/Audio`.
- **Media and Icons**: Found in `Assets/Images`.

---