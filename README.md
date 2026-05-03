# MonoBuilder | Monogatari Visual Novel Builder

### Current Version
- 0.4.0

## What is MonoBuilder?
MonoBuilder is a graphical interface development tool designed and developed to assist visual novel developers using the Monogatari game engine.

# Installation
- Download a release from the "Releases" section of the GitHub repository. (Preferably the latest release)
- Extract the contents to a location that is easily remembered. (And can be deleted in favor of a new version in the future)
- Run the "Setup.exe"
- Enjoy your new level of productivity!

## Features
- Character Manager
  - Actively manage which characters are listed in your project.
  - Synchronize existing characters from your current project with the program. (*Requires Setup)
  - Bulk add or remove any number of characters to your project without needlessly copying and pasting. (*Requires Setup)
  - Easily update character details, both in the program and in your game project.
- Script Builder
  - Build scripts from raw script data using an *LPE-like (Literate Programming Environment) writer and convert them into engine labels at the push of a button. (**Customizability Options Available)
    - *Literate Programming Environment (In this context): An IDE (Integrated Development Environment) styled writer that makes use of basic document level tools you might see in Microsoft Word or LibreOffice Writer. Shortcut keys for bold, italic, big, and small, as well as a toolbar to achieve a similar result.
  - Synchronize existing labels from your current project with the program. (**NOTE**: Synchronized scripts can be automatically converted to raw script data for editing purposes) (*Requires Setup)
  - Add, remove, and update existing labels in your game, or add new ones directly to your script file. (*Requires Setup)
  - Automatically check to see if any changes were made to the script before or during the application's runtime, and decide if those changes should be reflected in the program. (*Requires Setup)
  - Shortcut keys for implementing certain script actions while building your script.
    - Most of these shortcuts are list inside of tooltips in the program, but the ones that aren't will be listed here:
	- Ctrl + / — Comment or uncomment a line or selection of lines.
	- Ctrl + D — Duplicate a line or selection of lines. (Caret/Selection will follow the duplication)
	- Shift + Alt + Up Arrow — Duplicate a line or selection of lines. (Caret/Selection will NOT follow the duplication)
	- Alt + Up Arrow — Move a line or a selection of lines up one line.
    - Alt + Down Arrow — Move a line or a selection of lines down one line.
    - [ (Open Bracket) — Adds opening and closing braces in contextual situations: (Overall, it acts similar to an IDE, but not the exact same)
      - When the next character is an empty space or end of the line.
      - When a selection is made on a single line. (If you try to add braces on multiple lines using a selection, it will only add the brace to the start of the selection.)
    - { (Open Curly Brace) — Opens the action list. (Selecting an action will add tags and place the caret in a position best suited for continuing the script building process)
- Image Builder, Scene Builder, and Gallery Builder
  - Track and manage image assets added to the game's image, scene, and gallery directories.
  - Synchronize existing image assets, both in the program and in your game project.
  - View which image asset is which directly in the program. (No more guessing based on image name classification.)


### How To Use
- Script Builder
  - Set up characters you want to be converted in the `Settings Menu`. (Name and tag are required: Name = Character Name (Zaydin) | Tag = Character Tag (zad))
  - Write the label you wish the script to be attached to. (Top input box)
  - Write or copy/paste the script you wish to be converted into a label (Large input box)
  - Raw script data has a specific format that is used to convert properly into a label. (The only format that is **REQUIRED** is character dialog, all others should convert properly regardless and are only made to make reading and viewing easier)
    - Character Dialog - `Character Name - Character Text` (`Zaydin - Hello`)
    - String Actions - `[show character zad at left with SlideInLeft duration 0.25s]`
    - Object Actions (Enclosed) - `{ Function: { Apply: function() { return "cheese" } } }`
    - Object Actions (Open):
	```js
	{
		Function: {
			Apply: function() {
				return "cheese"
			}
		}
    }
	```
	- Comments - `// This is a comment`
	- Narration - `This is narration`
	- Known Issue with Character Dialog:
	  - `This would be narration - if it weren't for that pesky space dash space...`
  - Click `Convert`
  - Copy and paste the new label into your game project, or use the automatic script adder (*Requires Setup)

## Planned Features
- Character Builder
  - An all-encompassing character builder that allows users to easily add and manage sprites for all of their existing characters.
- Audio Builder
  - Track and manage any music, sound, or voice audio that you'll be using in your game.
- ~~Particle Builder~~ (As of 0.4.0, the particle builder will likely be a feature that links to a **proper** particle builder, rather than rigging up my own and running into various issues)
- Message Builder
  - Easily build and manage default messages that will be relayed to players during gameplay.
- Notification Builder
  - Build and manage notifications that will be sent to players during gameplay.
- Credits Manager
  - Build and configure the credits screen to appear and act as you intend.
- Main Menu Manager (**Being Considered, Not Final**)
  - Manage and manipulate aspects of the main menu (adding buttons, changing certain layouts with presets, etc.)

### *Requires Setup
Many features require the developer to set up their environment. This process is fairly easy to implement and includes automatic fail-safes in case files or folders are changed later.

**(Note: it is not recommended to change the names of, or delete, files and/or folders while the program isn't running. While there are fail-safes that will change the name or remove links if files/folders are renamed or removed while the program is running, there currently aren't any fail-safes when starting the program after the fact. This could result in startup errors, broken links, or weird behavior)**
- Enter the settings screen
- Scroll down to "**Game Directories**"
- Select a "**Base Folder**" (Usually the folder where the **index.html** is housed) *Not Technically "Required" Anymore, but please just set it to avoid bugs...
- Select an "**Assets Folder**" (Where images and other assets sit, usually called "**assets**") *Not Required, but may be helpful in future updates.
- Select an "**Image Asset Folder**" (Where images and other assets sit, usually called "**images**") *Required to use the `Image Builder`
- Select a "**Scene Asset Folder**" (Where images and other assets sit, usually called "**scenes**") *Required to use the `Scene Builder`
- Select a "**Gallery Asset Folder**" (Where images and other assets sit, usually called "**gallery**") *Required to use the `Gallery Builder` 
- Select (a) "**Characters File(s)**" (Where you define and add characters. Starts in the "**script.js**" by default) *Required to use a number of systems.
- Select (a) "**Script File(s)**" (Where you define and add script labels. Starts in the "**script.js**" by default) *Not Required, but heavily advised.
- Select (an) "**Image File(s)**" (Where you define and add image files. Starts in the "**script.js**" by default) *Required to use the `Image Builder`
- Select (a) "**Scene File(s)**" (Where you define and add scene image files. Starts in the "**script.js**" by default) *Required to use the `Scene Builder`
- Select (a) "**Gallery File(s)**" (Where you define and add gallery image files. Starts in the "**script.js**" by default) *Required to use the `Gallery Builder`

#### Setting up the Characters File for Program Manipulation and Development:
- Insert start and end tags inside of the `monogatari.characters({...});` definition(s).
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
- Insert start and end tags inside of the `monogatari.script({...});` definition(s).
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

#### Setting up the Image, Scene, and/or Gallery File for Program Manipulation and Development:
- Insert start and end tags inside of the different `monogatari.assets(...)` declarations.
  - Image Tags:
    - `// IMAGES_INSERTION_POINT`
	- `// END_IMAGES_INSERTION_POINT`
  - Scene Tags:
    - `// SCENES_INSERTION_POINT`
    - `// END_SCENES_INSERTION_POINT`
  - Gallery Tags:
    - `// GALLERY_INSERTION_POINT`
    - `// END_GALLERY_INSERTION_POINT`

```js
monogatari.assets ('images', {
  // IMAGES_INSERTION_POINT
  "ImAnImage": "Im/An/Image/Path.jpg"
  // END_IMAGES_INSERTION_POINT
});

monogatari.assets ('scenes', {
  // SCENES_INSERTION_POINT
  "ImAnImage": "Im/An/Image/Path.jpg"
  // END_SCENES_INSERTION_POINT
});

monogatari.assets ('gallery', {
  // GALLERY_INSERTION_POINT
  "ImAnImage": "Im/An/Image/Path.jpg"
  // END_GALLERY_INSERTION_POINT
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
  - *NOTE: The script builder's raw data input coloring runs on an .xshd hosted inside of your "data" folder. The folder and file is generated when the application first runs.* 

**NOTE: The converter is a bit "All Intensive." So, like with the example above, if you wrote `I have one goal: success.`, the converter would shorten everything behind the colon to a 3 lowercase letters (including the space) and output `"i h success.",`. This was done by design to help individuals who forget/don't set up their characters before using the converter. However, in the future, this may be changed to prevent situations as mentioned.**

## Known Issues
#### Version — 0.1.0
- ~~Only one instance of the **Script** & **Character** files are currently supported. This will be remedied in the future, allowing for multiple instances of all file types.~~ (Multi-file manipulation and management is now available as of version 0.4.0)
- ~~A strange memory leak occurs when opening and closing the "Script Builder" over and over, causing a buildup of memory. The built-up memory is mostly released when the user opens the settings menu, but I could not identify the cause at the time to fix it.~~ (As of 0.4.0, the WPF update seems to have resolved many of the memory leak issues. While WPF is more memory intensive off the bat, this version handles memory in a much better manner.)
#### Version — 0.4.0
- Currently, you are unable to swap the positions of synced scripts in the script loader.
  - This system will be either revamped or adjusted to help with better management of labels. 