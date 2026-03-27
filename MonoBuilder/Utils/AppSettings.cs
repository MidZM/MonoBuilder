using MonoBuilder.Screens.ScreenUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MonoBuilder.Utils
{
    public class AppSettings
    {
        /// <summary>Gets or sets a collection of file links.</summary>
        private Dictionary<string, string> Files { get; set; } = new Dictionary<string, string>();
        /// <summary>Gets or sets the collection of folder links.</summary>
        private Dictionary<string, string> Folders { get; set; } = new Dictionary<string, string>();

        /// <summary>Gets or sets a collection mapping keys to file names.</summary>
        private Dictionary<string, string> FileNames { get; set; } = new Dictionary<string, string>();
        /// <summary>Gets or sets a collection of folder names mapped by key.</summary>
        private Dictionary<string, string> FolderNames { get; set; } = new Dictionary<string, string>();

        /// <summary>Gets or sets script configuration settings as key-value pairs.</summary>
        private Dictionary<string, object> ScriptSettings { get; set; } = new Dictionary<string, object>
        {
            { "ColorFormatting", true },
            { "AutoSyncLabels", true },
            { "IndentationType", "Spaces" },
            { "IndentationAmount", 4 }
        };

        private bool SynchronicityChecked = false;

        /// <summary>Gets or sets the XML data document.</summary>
        private XDocument Data { get; set; } = new XDocument();

        public AppSettings() {}

        #region XML Methods
        /// <summary>
        /// Loads directory and file settings from the XML configuration file, creating default data if necessary.
        /// </summary>
        /// <remarks>Displays error dialogs if the configuration file is missing or cannot be
        /// read.</remarks>
        public void LoadDirectories()
        {
            try
            {
                if (!Directory.Exists("data"))
                {
                    Directory.CreateDirectory("data");
                }

                if (!File.Exists("data/settings.xml"))
                {
                    SaveDirectories();
                    return;
                }

                Data = XDocument.Load("data/settings.xml");

                var fileDescendants = Data.Descendants("File");
                var folderDescendants = Data.Descendants("Folder");

                Files = fileDescendants.ToDictionary(
                        a => a.Attribute("Key")?.Value ?? "Unknown",
                        a => a.Attribute("Value")?.Value ?? string.Empty
                    );

                FileNames = fileDescendants.ToDictionary(
                        a => a.Attribute("Key")?.Value ?? "Unknown",
                        a => a.Attribute("FileName")?.Value ?? string.Empty
                    );

                Folders = folderDescendants.ToDictionary(
                        a => a.Attribute("Key")?.Value ?? "Unknown",
                        a => a.Attribute("Value")?.Value ?? string.Empty
                    );

                FolderNames = folderDescendants.ToDictionary(
                        a => a.Attribute("Key")?.Value ?? "Unknown",
                        a => a.Attribute("FolderName")?.Value ?? string.Empty
                    );

                var descendents = Data.Descendants("ScriptSetting").ToList();
                foreach (XElement setting in descendents)
                {
                    string? attribute = setting.Attribute("Key")?.Value;
                    string? value = setting.Attribute("Value")?.Value;
                    switch (attribute)
                    {
                        case "ColorFormatting":
                            bool.TryParse(value, out bool colorBoolValue);
                            ScriptSettings[attribute] = colorBoolValue;
                            break;
                        case "AutoSyncLabels":
                            bool.TryParse(value, out bool autoSyncBoolValue);
                            ScriptSettings[attribute] = autoSyncBoolValue;
                            break;
                        case "IndentationType":
                            ScriptSettings[attribute] = value ?? "Spaces";
                            break;
                        case "IndentationAmount":
                            if (int.TryParse(value, out int intValue))
                                ScriptSettings[attribute] = intValue;
                            else
                                ScriptSettings[attribute] = 4;
                            break;
                    }
                }
            }
            catch (FileNotFoundException error)
            {
                DialogBox.Show(
                    $"File Not Found!\r\n{error}",
                    "Error",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
            }
            catch (XmlException error)
            {
                DialogBox.Show(
                    $"Save Data Reading Failure!\r\n{error}",
                    "Error",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
            }
            catch (Exception error)
            {
                DialogBox.Show(
                    $"Something went wrong!\r\n{error}",
                    "Error",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
            }
        }

        /// <summary>
        /// Saves the current files, folders, and other settings to an XML file at 'data/settings.xml'.
        /// </summary>
        /// <remarks>The XML document includes separate elements for files, folders, and script settings,
        /// each with relevant attributes for identification.</remarks>
        public void SaveDirectories()
        {
            Data = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Root",
                    new XElement("Files",
                        Files.Select(f => new XElement("File",
                            new XAttribute("Key", f.Key),
                            new XAttribute("Value", f.Value),
                            new XAttribute("FileName", f.Value
                                .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)
                                .TakeLast(1)
                                .FirstOrDefault() ?? string.Empty)
                        ))
                    ),
                    new XElement("Folders",
                        Folders.Select(f => new XElement("Folder",
                            new XAttribute("Key", f.Key),
                            new XAttribute("Value", f.Value),
                            new XAttribute("FolderName", f.Value
                                .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)
                                .TakeLast(1)
                                .FirstOrDefault() ?? string.Empty)
                        ))
                    ),
                    new XElement("ScriptSettings",
                        ScriptSettings.Select(f => new XElement("ScriptSetting",
                            new XAttribute("Key", f.Key),
                            new XAttribute("Value", f.Value)
                        ))
                    )
                )
            );

            Data.Save("data/settings.xml");
        }
        #endregion

        #region Directory Methods
        /// <summary>
        /// Adds a file path to the collection or replaces the existing entry for the specified key.
        /// </summary>
        /// <param name="key">The key associated with the file path.</param>
        /// <param name="path">The file path to add or replace.</param>
        public void AddReplaceFile(string key, string path)
        {
            FileNames[key] = path;
        }

        /// <summary>
        /// Removes the file associated with the specified key from the collection.
        /// </summary>
        /// <param name="key">The key of the file to remove.</param>
        /// <returns>true if the file was successfully removed; otherwise, false.</returns>
        public bool RemoveFile(string key)
        {
            return FileNames.Remove(key);
        }

        /// <summary>
        /// Retrieves all file names and their corresponding paths.
        /// </summary>
        /// <returns>A dictionary containing file names as keys and their paths as values.</returns>
        public Dictionary<string, string> GetAllFiles()
        {
            return FileNames;
        }

        /// <summary>
        /// Retrieves the file name associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose associated file name is to be returned.</param>
        /// <returns>The file name associated with the specified key, or null if the key does not exist.</returns>
        public string? GetFile(string key)
        {
            return FileNames.TryGetValue(key, out string? value) && value != string.Empty ? value : null;
        }

        /// <summary>
        /// Adds or replaces a file path associated with the specified key.
        /// </summary>
        /// <param name="key">The key identifying the file path.</param>
        /// <param name="path">The file path to associate with the key.</param>
        public void AddReplaceFilePath(string key, string path)
        {
            Files[key] = path;
        }

        /// <summary>
        /// Removes the file path associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the file path to remove.</param>
        /// <returns>true if the file path was successfully removed; otherwise, false.</returns>
        public bool RemoveFilePath(string key)
        {
            return Files.Remove(key);
        }

        /// <summary>
        /// Retrieves all file paths and their associated values.
        /// </summary>
        /// <returns>A dictionary containing file paths as keys and their corresponding values.</returns>
        public Dictionary<string, string> GetAllFilePaths()
        {
            return Files;
        }

        /// <summary>
        /// Retrieves the file path associated with the specified key.
        /// </summary>
        /// <param name="key">The key used to locate the file path.</param>
        /// <returns>The file path if found; otherwise, null.</returns>
        public string? GetFilePath(string key)
        {
            return Files.TryGetValue(key, out string? value) && value != string.Empty ? value : null;
        }

        /// <summary>
        /// Adds or replaces a folder mapping with the specified key and value.
        /// </summary>
        /// <param name="key">The key identifying the folder mapping.</param>
        /// <param name="value">The folder path to associate with the key.</param>
        public void AddReplaceFolder(string key, string value)
        {
            FolderNames[key] = value;
        }

        /// <summary>
        /// Removes the folder associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the folder to remove.</param>
        /// <returns>true if the folder was successfully removed; otherwise, false.</returns>
        public bool RemoveFolder(string key)
        {
            return FolderNames.Remove(key);
        }

        /// <summary>
        /// Retrieves a dictionary containing all folder names and their corresponding values.
        /// </summary>
        /// <returns>A dictionary mapping folder names to their associated values.</returns>
        public Dictionary<string, string> GetAllFolders()
        {
            return FolderNames;
        }

        /// <summary>
        /// Retrieves the folder name associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose associated folder name is to be returned.</param>
        /// <returns>The folder name if the key exists; otherwise, null.</returns>
        public string? GetFolder(string key)
        {
            return FolderNames.TryGetValue(key, out string? value) && value != string.Empty ? value : null;
        }

        /// <summary>
        /// Adds a folder path with the specified key or replaces the existing path for the key.
        /// </summary>
        /// <param name="key">The unique key associated with the folder path.</param>
        /// <param name="path">The folder path to add or replace.</param>
        public void AddReplaceFolderPath(string key, string path)
        {
            Folders[key] = path;
        }

        /// <summary>
        /// Removes the folder path associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the folder path to remove.</param>
        /// <returns>true if the folder path was successfully removed; otherwise, false.</returns>
        public bool RemoveFolderPath(string key)
        {
            return Folders.Remove(key);
        }

        /// <summary>
        /// Retrieves all folder paths as key-value pairs.
        /// </summary>
        /// <returns>A dictionary containing folder names and their corresponding paths.</returns>
        public Dictionary<string, string> GetAllFolderPaths()
        {
            return Folders;
        }

        /// <summary>
        /// Retrieves the folder path associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose associated folder path is to be retrieved.</param>
        /// <returns>The folder path if the key exists; otherwise, null.</returns>
        public string? GetFolderPath(string key)
        {
            return Folders.TryGetValue(key, out string? value) && value != string.Empty ? value : null;
        }
        #endregion

        #region Script Methods
        public void SetSynchronicityCheck(bool set) => SynchronicityChecked = set;
        public bool HasBeenSynchronized() => SynchronicityChecked;

        /// <summary>
        /// Enables or disables color formatting in script settings.
        /// </summary>
        /// <param name="set">true to enable color formatting; false to disable.</param>
        public void SetColorFormatting(bool set)
        {
            ScriptSettings["ColorFormatting"] = set;
        }

        /// <summary>
        /// Gets a value indicating whether color formatting is enabled in the script settings.
        /// </summary>
        /// <returns>true if color formatting is enabled; otherwise, false.</returns>
        public bool GetColorFormatting()
        {
            return (bool)ScriptSettings["ColorFormatting"];
        }

        /// <summary>
        /// Enables or disables automatic synchronization of labels in the script settings.
        /// </summary>
        /// <param name="set">true to enable automatic label synchronization; false to disable.</param>
        public void SetAutoSyncLabels(bool set)
        {
            ScriptSettings["AutoSyncLabels"] = set;
        }

        /// <summary>
        /// gets a value indicating whether automatic synchronization of labels is enabled.
        /// </summary>
        /// <returns>true if automatic label synchronization is enabled; otherwise, false.</returns>
        public bool GetAutoSyncLabels()
        {
            return (bool)ScriptSettings["AutoSyncLabels"];
        }

        /// <summary>
        /// Sets the indentation amount for script formatting.
        /// </summary>
        /// <param name="value">The number of spaces to use for indentation.</param>
        public void SetIndentationAmount(int value)
        {
            ScriptSettings["IndentationAmount"] = value;
        }

        /// <summary>
        /// Retrieves the indentation amount from the script settings.
        /// </summary>
        /// <returns>The indentation amount as an integer.</returns>
        public int GetIndentationAmount()
        {
            return (int)ScriptSettings["IndentationAmount"];
        }

        /// <summary>
        /// Sets the indentation type used in script settings.
        /// </summary>
        /// <param name="type">The indentation type to apply.</param>
        public void SetIndentationType(string type)
        {
            ScriptSettings["IndentationType"] = type;
        }

        /// <summary>
        /// Retrieves the indentation type setting from the script settings.
        /// </summary>
        /// <returns>A string representing the indentation type.</returns>
        public string GetIndentationType()
        {
            return (string)ScriptSettings["IndentationType"];
        }
        #endregion
    }
}