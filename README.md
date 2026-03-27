# MonoBuilder | Monogatari Visual Novel Builder

### Current Version
- 0.1.0

## What is MonoBuilder?
MonoBuilder is a graphical interface development tool designed and developed to assist visual novel developers using the Monogatari game engine.

## Features
- Character Manager
 - Actively manage which characters are listed in your project.
 - Synchronize existing characters from your current project with the program. (*Requires Setup)
 - Bulk add or remove any number of characters to your project without needlessly copying and pasting. (*Requires Setup)
 - Easily update character details, both in the program and in your game project.
- Script Builder
 - Build scripts from raw script data and convert them into engine labels at the push of a button. (**Customizability Options Available)
 - Synchronize existing labels from your current project with the program. (**NOTE**: Synchronized scripts can be automatically converted to raw script data for editing purposes) (*Requires Setup)
 - Add, remove, and update existing labels in your game, or add new ones directly to your script file. (*Requires Setup)
 - Automatically check to see if any changes were made to the script before or during the application's runtime, and decide if those changes should be reflected in the program. (*Requires Setup)

## Known Issues
- Only one instance of the **Script** & **Character** files are currently supported. This will be remedied in the future, allowing for multiple instances of all file types.
- A strange memory leak occurs when opening and closing the "Script Builder" over and over, causing a buildup of memory. The built-up memory is mostly released when the user opens the settings menu, but I could not identify the cause at the time to fix it.

## Planned Features
- Character Builder
 - An all-encompassing character builder that allows users to easily add and manage sprites for all of their existing characters.
- Image Builder
 - Easily import and manage images associated with your game. (Resizing, updating, etc.)
- Scene Builder
 - Similar to the **Image Builder**, import and manage images associated with scenes within your game.
- Audio Builder
 - Track and manage any music, sound, or voice audio that you'll be using in your game.
- Particle Builder
 - A visualizer capable of showing you what your particles will look like when imported into the game.
 - (**Thought in Progress**. The particle system is incredibly complex. As such, accounting for everything it's capable of is a challenge I may not be capable of accomplishing.)
- Message Builder
 - Easily build and manage default messages that will be relayed to players during gameplay.
- Notification Builder
 - Build and manage notifications that will be sent to players during gameplay.
- Credits Manager
 - Build and configure the credits screen to appear and act as you intend.

### *Requires Setup
Many features require the developer to set up their environment. This process is fairly easy to implement and includes automatic fail-safes in case files or folders are changed later.

**(Note: it is not recommended to change the names of, or delete, files and/or folders while the program isn't running. While there are fail-safes that will change the name or remove links if files/folders are renamed or removed while the program is running, there currently aren't any fail-safes when starting the program after the fact. This could result in startup errors, broken links, or weird behavior)**
- Enter the settings screen
- Scroll down to "**Game Directories**"
- Select a "**Base Folder**" (Usually the folder where the **index.html** is housed)
- Select an "**Assets Folder**" (Where images and other assets sit, usually called "**assets**")
- Select a "**Characters File**" (Where you define and add characters. Starts in the "**script.js**" by default)
- Select a "**Script File**" (Where you define and add script labels. Starts in the "**script.js**" by default)

#### Setting up the Characters File for Program Manipulation and Development:
- Insert start and end tags inside of the `monogatari.characters({...});` definition.
 - `// CHARACTERS_INSERTION_POINT`
 - `// END_CHARACTERS_INSERTION_POINT`
- Insert start and end tags on **EACH** character inside of the `monogatari.characters({...});` definition.
 - `// START_CHARACTER`
 - `// END_CHARACTER`
- **It is highly recommended that you add a space between the character beginning and start tag, and the character ending and end tag. There's no guarantee the program will recognize all actions if there is no space there, even though I have painstakingly attempted to adjust it to accommodate as such.*

##### Example:
```js
// Define the Characters
monogatari.characters ({
    // CHARACTERS_INSERTION_POINT
    'y': { // START_CHARACTER
        "name": "Yui",
        "color": "#E36C09",
        "sprites": {
 normal: 'yui.png',
 happy: 'yui-happy.png',
 sad: 'yui-sad.png',
        },
    }, // END_CHARACTER

    "yu": { // START_CHARACTER
        "name": "Yuno",
        "color": "#974806",
        "directory": "Yuno",
    }, // END_CHARACTER
    // END_CHARACTERS_INSERTION_POINT
});
```

#### Setting up the Script File for Program Manipulation and Development:
- Insert start and end tags inside of the `monogatari.script({...});` definition.
 - `// SCRIPT_INSERTION_POINT`
 - `// END_SCRIPT_INSERTION_POINT`
- Insert start and end tags on **EACH** script label inside of the `monogatari.script({...});` definition.
 - `// START_LABEL`
 - `// END_LABEL`
- **It is highly recommended that you add a space between the script label beginning and start tag, and the script label ending and end tag. There's no guarantee the program will recognize all actions if there is no space there, even though I have painstakingly attempted to adjust it to accommodate as such.*

##### Example:
```js
monogatari.script ({
    // SCRIPT_INSERTION_POINT
    'Yes': [ // START_LABEL
        'ell Thats awesome!',
        'ell Then you are ready to go ahead and create an amazing Game!',
        'ell I can’t wait to see what story you’ll tell!',
        'end'
    ], // END_LABEL

    'No': [ // START_LABEL

        'ell You can do it now.',

        'show message Help',

        'ell Go ahead and create an amazing Game!',
        'ell I can’t wait to see what story you’ll tell!',
        'end'
    ], // END_LABEL
    // END_SCRIPT_INSERTION_POINT
});
```

### **Customizability Options Available
- Script Builder
 - Inside the settings screen, there are some customization options for building scripts.
 - The default script character dialog and conversions are as follows:
 - Raw Data:
 - Elliosa - Hi Zaydin~~!
 - Zaydin - NO! NOT TODAY!
 - Zaydin flees
 - Converted Data:
 - `"ell Hi Zaydin~~!",`
 - `"zad NO! NOT TODAY!",`
 - `"Zaydin flees",`
 - Regex:
 - Character Dialog - `^(?<character>.+?)\s*-\s*(?<text>.+)$`
 - Narration - `^(?<text>.+)$`
 - If you wanted to change how the converter works, you would change the Regex:
 - Raw Data:
 - Elliosa: Hi Zaydin~~!
 - Converted Data:
 - `"ell Hi Zaydin~~!",`
 - Regex
 - Character Dialog - `^(?<character>.+?):\s*(?<text>.+)$`
**NOTE: The converter is a bit "All Intensive." So, like with the example above, if you wrote `I have one goal: success.`, the converter would shorten everything behind the colon to a 3 lowercase letters (including the space) and output `"i h success.",`. This was done by design to help individuals who forget/don't set up their characters before using the converter. However, in the future, this may be changed to prevent situations as mentioned.** 

# Installation
- Download a release from the "Releases" section of the GitHub repository. (Preferably the latest release)
- Extract the contents to a location that is easily remembered. (And can be deleted in favor of a new version in the future)
- Run the "Setup.exe"
- Enjoy your new level of productivity!