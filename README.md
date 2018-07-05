# Multimodal Interaction Assignment: Speech Modality

## Counter-Strike: Global Offensive Voice Assistant

Installation steps:

* Clone the repository.

* Place **gamestate_integration_quickstartguide.cfg** and **autoexec.cfg** in your ```SteamLibrary\steamapps\common\Counter-Strike Global Offensive\csgo\cfg``` folder.

* Make sure you have **Microsoft.Speech** and **mmisharp.dll** added to the Project References in Visual Studio.

* Make sure you have the [CSGSI](https://github.com/rakijah/CSGSI), [InputSimulator](https://www.nuget.org/packages/InputSimulator/) and [Newtonsoft.json](https://www.nuget.org/packages/Newtonsoft.Json/) NuGet packages installed.

To run:

* Open **AppGUI** and **SpeechModality** Visual Studio projects, and execute each one of them.

* Run **mmiframeworkV2.jar** with ```java -jar mmiframeworkV2.jar```.

* Open **Counter-Stike:Global Offensive** in *borderless fullscreen*.