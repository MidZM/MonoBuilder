using MonoBuilder.Screens;
using MonoBuilder.Screens.ScreenUtils;
using MonoBuilder.Utils;
using MonoBuilder.Utils.image_management;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MonoBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Characters CharacterData = new();
        private readonly MonoImages ImageData = new();
        private readonly AppSettings ApplicationSettings = new();
        private readonly ActionHelper ActionUtility = new();
        private readonly FileSystemWatcher MainWatcher = new();
        private readonly ScriptConversion Converter;

        public MainWindow()
        {
            ApplicationSettings.LoadDirectories();
            ActionUtility.LoadActions();
            CharacterData.LoadSettings(ApplicationSettings);
            ImageData.LoadSettings(ApplicationSettings);
            Converter = new ScriptConversion(CharacterData, ActionUtility);

            Converter.SetIsFormattingColor(ApplicationSettings.GetColorFormatting());
            Converter.SetAutoSyncLabels(ApplicationSettings.GetAutoSyncLabels());
            Converter.ChangeIndentationAmount(ApplicationSettings.GetIndentationAmount());
            Converter.ChangeIndentationType(ApplicationSettings.GetIndentationType());

            InitializeComponent();
            InitializeWatcher();
        }

        #region Initialization Methods
        private void InitializeWatcher()
        {
            FileWatcher.InitializeWatcher(
                MainWatcher,
                ApplicationSettings,
                Converter,
                CharacterData,
                ImageData);
            FileWatcher.SetCurrentContext(this);

            if (CharacterData.CheckSynchronicity(true))
            {
                FileWatcher.ForciblyUpdateCharactersList(false);
            }

            if (ImageData.CheckSynchronicity(true))
            {
                FileWatcher.ForciblyUpdateImagesList(false);
            }
        }
        #endregion

        #region Utility Methods
        private void SetupEnvironment(Window window)
        {
            window.Owner = this;
            window.Loaded += (s, e) =>
            {
                this.Hide();
                FileWatcher.SetCurrentContext(window);
            };

            window.Closing += (s, e) =>
            {
                this.IsEnabled = true;
                this.Show();
                FileWatcher.SetCurrentContext(this);
            };

            this.IsEnabled = false;
        }

        private void OpenSettings()
        {
            Settings SettingsScreen = new(CharacterData, ApplicationSettings, Converter);

            SetupEnvironment(SettingsScreen);
            SettingsScreen.Show();
        }

        private void OpenScriptBuilder()
        {
            ScriptBuilder ScriptBuilderScreen = new ScriptBuilder(
                ApplicationSettings,
                Converter,
                ActionUtility);

            SetupEnvironment(ScriptBuilderScreen);
            ScriptBuilderScreen.Show();
        }

        private void OpenImageBuilder(string type)
        {
            ImageBuilder ImageBuilderScreen = new ImageBuilder(ApplicationSettings, ImageData, type);

            SetupEnvironment(ImageBuilderScreen);
            ImageBuilderScreen.Show();
        }
        #endregion

        #region Event Handlers
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSettings();
        }

        private void BtnScriptBuilder_Click(object sender, RoutedEventArgs e)
        {
            OpenScriptBuilder();
        }

        private void BtnImageBuilder_Click(object sender, RoutedEventArgs e)
        {
            OpenImageBuilder("Images");
        }

        private void BtnSceneBuilder_Click(object sender, RoutedEventArgs e)
        {
            OpenImageBuilder("Scenes");
        }

        private void BtnGalleryBuilder_Click(object sender, RoutedEventArgs e)
        {
            OpenImageBuilder("Gallery");
        }

        private void VersionInfo_Clicked(object sender, RoutedEventArgs e)
        {
            var title = Properties.Resources.Title;
            var program = Properties.Resources.Program;
            var version = Properties.Resources.Version;
            var description = Properties.Resources.Description;
            var developer = Properties.Resources.Developer;

            DialogBox.Show(
                @$"Title:		{title}
Program:	{program}
Version:		{version}
Description:	{description}

Developer:	{developer}

Originally, this program was designed and developed to make it easier to create the visual novel ""Symbiotic: Invasion"". However, it was decided early in development to turn it into an all-purpose dev tool that would help any developer, using Monogatari, build and manage their project in a user-friendly GUI.",
                "Version Information",
                600,
                DialogButtonDefaults.OK,
                DialogIcon.Information);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion
    }
}