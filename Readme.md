# PvzA11y Beta 1.17.2
#### A blind and motor accessibility mod for Plants Vs Zombies.

### Blind accessibility
With this mod, almost 100% of the game is completely blind accessible, making the game much more enjoyable for blind players.  
It features a completely new input system, allowing the game to be played with a keyboard or controller.  
Audio cues have been added for most of the gameplay elements.  
It features full text narration, with support for NVDA, JAWS, and SAPI.  
There are many gameplay tutorials to relay information that's normally only represented visually.  
There's a freeze mode, which allows you to take as long as you need to process the current state of the game.  
Plus a long list of other accessibility features, which can all be adjusted and toggled in the included accessibility settings menu.  

### Motor accessibility
This mod allows the game to be played without a mouse, improving playability for anyone who struggles with quick/precise mouse movements.  
All inputs can be remapped to suit your ideal ergnomic layout.  
Sun and coins can be collected automatically, reducing repetitive strain and fatigue.  
Freeze mode allows you to take as long as you need to perform any inputs.  
Timing windows for double-tap actions can be greatly extended.  

### For able gamers
This mod also doubles as a way for sighted and able gamers to play the game with a keyboard or controller.  
You could play comfortably from your couch, without needing any precise mouse movements.  
Or it could be useful for portable devices, where playing with the built-in controller could be more ergonomic than using a touchscreen.  

You can disable narration by setting the screen reader engine to 'Off', and you can disable the audio cues by setting the master audio cue volume to 0.  
You'll probably also want to disable gameplay tutorials, which can be found in the accessibility settings menu, along with the other mentioned options.  
The accessibility settings menu can be found near the bottom of the options menu.  
This menu is not visible within the game, but the contents are printed to the console window that the mod opens.  




## Installation
This mod requires the [Microsoft .Net 8.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.0-windows-x86-installer) to be installed on your system.  
This mod is completely portable, and makes no permanent changes to any of your game files.  
All modifications are performed in-memory, meaning they can be completely removed by restarting the game.  

To get started, extract the latest release zip anywhere on your system.  
Launch both PvZA11y.exe and your game, in any order, and the mod will automatically hook into the game.  
The mod can also automatically launch the game for you, if you enable the automatic launching option in the accessibility settings menu.  

### Updater
This mod includes an [easy updater tool](https://github.com/CG8516/PvZA11y-Updater), to make installing and updating easier.  
Launch PvZA11y-Updater.exe and specify a directory to install, or leave blank to use the current directory.  
The updater will automatically download and extract the latest mod update to your specified directory.  

### Supported game versions  
This mod currently supports the GOTY dvd release (1.2.0.1073), and [the latest steam release](https://store.steampowered.com/app/3590/Plants_vs_Zombies_GOTY_Edition/) (last checked: Jan 12, 2024).  
No other game versions will work with this mod.  

### Input system
All inputs can be remapped in the accessibility settings menu.  
The default keybinds are listed in the readme.txt file included in the release zip.  
This mod uses xinput for controller support. To use a non-xbox controller, you'll need to use a program like [DS4Windows.](https://github.com/Ryochan7/DS4Windows)  

### Known issues  
Some users have reported that they are unable to interact with some things when 3D Acceleration or fullscreen mode is enabled.  
You may be able to work around this issue by disabling either of those options, or by setting the display/dpi scale to 100% for all connected monitors (this can be found in windows display settings).  
If you're unable to interact with any of the checkboxes in the options menus; close the game and open/import any of the included registry files in the RegistryKeys folder.   
These registry files only affect the game's settings, and will not impact any other functionality of your system.  

### Special thanks
This mod wouldn't be where it is without these awesome people:  
[azurejoga](https://github.com/azurejoga): For creating and helping to revise the updater.  
[Cyrax2001](https://github.com/Cyrax2001): For contributing the Spanish translation.  
[The amazing people on the audiogames.net forum](https://forum.audiogames.net/post/822297): For testing, providing feedback, and being a great source of motivation.  
[Everybody who's left feedback and bug reports on github](https://github.com/CG8516/PvZA11y/issues?q=): Same as above, but better, because your feedback is much easier to keep track of.  

#### Libraries used:
[memory.dll](https://github.com/erfg12/memory.dll). For accessing the game's memory.  
[Accessible Output](https://github.com/SaqibS/AccessibleOutput). For implementing screenreader support.  
[NAudio](https://github.com/naudio/NAudio). Generates and plays all sound effects that the mod uses.  
[YamlDotNet](https://github.com/aaubry/YamlDotNet). For loading the translation files.  
[Vortice.Xinput](https://github.com/amerkoleci/Vortice.Windows). For adding controller support.

