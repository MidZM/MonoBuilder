using Krypton.Toolkit;
using MonoBuilder.Screens.ScreenUtils;
using MonoBuilder.Utils;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MonoBuilder.Screens
{
    public partial class Settings : KryptonForm
    {
        private Characters CharacterData { get; set; }
        private AppSettings ApplicationSettings { get; set; }
        private ScriptConversion ScriptConverter { get; set; }
        private record SettingSection(string Header, List<SettingItem> Items);
        private record SettingItem(string Label, Control Control);
        private bool IsRegexInvalid { get; set; }
        private string SelectedBasePath { get; set; }

        private List<SettingSection> Sections { get; set; } = [];

        public Settings(Characters characterList, AppSettings settings, ScriptConversion converter)
        {
            CharacterData = characterList;
            ApplicationSettings = settings;
            ScriptConverter = converter;
            SelectedBasePath = settings.GetFolderPath("Base") ?? string.Empty;
            InitializeComponent();
            Elements.RegisterScope(this);
        }

        private void InitializeSettings()
        {
            #region Programmatic Elements
            var ruleMap = ScriptConverter.ConversionRules
                .Where(r => r.Name != null)
                .ToDictionary(r => r.Name!);

            Sections = new List<SettingSection>
            {
                new SettingSection("General Settings", new List<SettingItem>
                {
                    new SettingItem("", Elements.Create<KryptonLabel>("dgvCharactersLabel", lbl =>
                    {
                        lbl.Text = "Characters";
                        lbl.StateCommon.ShortText.Font = new Font(this.Font.FontFamily, 10);
                    })),
                    new SettingItem("", Elements.Create<KryptonSeparator>("sepGeneralCharacters", sep =>
                    {
                        sep.AllowMove = false;
                        sep.TabStop = false;
                        sep.Orientation = Orientation.Horizontal;
                        sep.StateCommon.Separator.Back.Color1 = Color.Maroon;
                        sep.StateCommon.Separator.Back.ColorStyle = PaletteColorStyle.Solid;
                    })),
                    new SettingItem("", Elements.Create<KryptonDataGridView>("dgvCharacters", dgv =>
                    {
                        dgv.ReadOnly = true;
                        dgv.Height = 200;
                        dgv.RowHeadersVisible = false;
                        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        dgv.DataSource = CharacterData.AllCharactersSource;
                        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                        dgv.CellDoubleClick += UpdateCharacter;
                        dgv.AllowUserToDeleteRows = false;
                        dgv.AllowUserToResizeRows = false;
                        dgv.CellMouseEnter += (s, e) =>
                        {
                            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                                dgv.Cursor = Cursors.Hand;
                        };
                        dgv.CellMouseLeave += (s, e) => dgv.Cursor = Cursors.Default;
                        dgv.SelectionChanged += (s, e) =>
                        {
                            var boxPath = Elements.Query<KryptonTextBox>("inputCharactersPath");
                            var btnAdd = Elements.Query<KryptonButton>("btnSaveToScript");
                            if (btnAdd != null && boxPath?.Text.Length > 0)
                            {
                                if (dgv.SelectedRows.Count > 0)
                                {
                                    btnAdd.Enabled = true;
                                }
                                else
                                {
                                    btnAdd.Enabled = false;
                                }
                            }
                        };

                    })),
                    new SettingItem("", Elements.Create<KryptonTableLayoutPanel>("pnlCharacterBtns", pnl =>
                    {
                        var AddButton = Elements.Create<KryptonButton>("btnAddCharacter", btn =>
                        {
                            btn.ButtonStyle = ButtonStyle.Alternate;
                            btn.Text = "Add Character";
                            btn.Dock = DockStyle.Fill;
                            btn.Click += AddCharacter;
                            btn.Cursor = Cursors.Hand;
                        });
                        var UpdateButton = Elements.Create<KryptonButton>("btnUpdateCharacter", btn =>
                        {
                            btn.ButtonStyle = ButtonStyle.Alternate;
                            btn.Text = "Update Character";
                            btn.Dock = DockStyle.Fill;
                            btn.Click += UpdateCharacter;
                            btn.Cursor = Cursors.Hand;
                        });
                        var RemoveButton = Elements.Create<KryptonButton>("btnRemoveCharacter", btn =>
                        {
                            btn.ButtonStyle = ButtonStyle.Alternate;
                            btn.Text = "Remove Character";
                            btn.Dock = DockStyle.Fill;
                            btn.Click += RemoveCharacter;
                            btn.Cursor = Cursors.Hand;
                        });
                        var FlexPanel = Elements.Create<KryptonTableLayoutPanel>("pnlCharacterBtnsFlex", panel =>
                        {
                            var SaveToScriptButton = Elements.Create<KryptonButton>("btnSaveToScript", btn =>
                            {
                                btn.ButtonStyle = ButtonStyle.Alternate;
                                btn.Text = "Save Characters to Script";
                                btn.Dock = DockStyle.Fill;
                                btn.Enabled = false;
                                btn.Click += SaveCharactersToScript;
                                btn.Cursor = Cursors.Hand;
                            });
                            var SyncButton = Elements.Create<KryptonButton>("btnSyncCharacters", btn =>
                            {
                                btn.ButtonStyle = ButtonStyle.Alternate;
                                btn.Text = "Sync Characters";
                                btn.Dock = DockStyle.Fill;
                                btn.Enabled = false;
                                btn.Click += SyncCharacters;
                                btn.Cursor = Cursors.Hand;
                            });

                            panel.Dock = DockStyle.Fill;

                            panel.ColumnCount = 0;
                            panel.ColumnStyles.Clear();

                            panel.ColumnCount++;
                            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                            panel.ColumnCount++;
                            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

                            panel.Margin = new Padding(0);

                            panel.Controls.Add(SaveToScriptButton, 0, 0);
                            panel.Controls.Add(SyncButton, 1, 0);
                        });

                        pnl.Height = 80;

                        pnl.RowCount = 1;
                        pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
                        pnl.RowCount++;
                        pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

                        pnl.ColumnCount = 0;
                        pnl.ColumnStyles.Clear();

                        pnl.ColumnCount++;
                        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)33.33));
                        pnl.ColumnCount++;
                        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)33.33));
                        pnl.ColumnCount++;
                        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)33.33));

                        pnl.Controls.Add(AddButton, 0, 0);
                        pnl.Controls.Add(UpdateButton, 1, 0);
                        pnl.Controls.Add(RemoveButton, 2, 0);
                        pnl.Controls.Add(FlexPanel, 0, 1);
                        pnl.SetColumnSpan(FlexPanel, 3);
                        //pnl.Controls.Add(SyncButton, 0, 1);
                        //pnl.SetColumnSpan(SyncButton, 3);
                    })),
                    new SettingItem("", Elements.Create<KryptonLabel>("lblCharacterPath", lbl =>
                    {
                        lbl.Text = "Game Directories";
                        lbl.StateCommon.ShortText.Font = new Font(this.Font.FontFamily, 10);
                    })),
                    new SettingItem("", Elements.Create<KryptonSeparator>("sepGameDirs", sep =>
                    {
                        sep.AllowMove = false;
                        sep.TabStop = false;
                        sep.Orientation = Orientation.Horizontal;
                        sep.StateCommon.Separator.Back.Color1 = Color.Maroon;
                        sep.StateCommon.Separator.Back.ColorStyle = PaletteColorStyle.Solid;
                    })),
                    new SettingItem("Base Folder", Elements.Create<KryptonPanel>("btnBaseFolderPanel", pnl =>
                    {
                        var btn = Elements.Create<KryptonButton>("btnBaseFolder");
                        btn.Tag = new Dictionary<string, string>
                        {
                            { "Type", "Base" },
                            { "Link", "inputBaseFolder" }
                        };
                        btn.ButtonStyle = ButtonStyle.Alternate;
                        btn.Width = 150;
                        btn.Text = "Base Folder";
                        btn.Dock = DockStyle.Right;
                        btn.Click += SelectFolder;
                        btn.Click += CheckPathSelectors;
                        btn.Cursor = Cursors.Hand;

                        pnl.Height = 30;
                        pnl.Controls.Add(btn);
                    })),
                    new SettingItem("", Elements.Create<KryptonTextBox>("inputBaseFolder", input =>
                    {
                        var path = ApplicationSettings.GetFolderPath("Base");
                        if (path != null)
                        {
                            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                            string? result = "...\\" +
                                Path.GetRelativePath(userProfile, path);
                            input.Text = result;
                        }
                        else
                        {
                            input.Text = string.Empty;
                        }
                        input.ReadOnly = true;
                        input.TabStop = false;
                        input.Cursor = Cursors.No;
                        input.MouseMove += DisabledTextBoxCursor;
                    })),
                    new SettingItem("Assets Folder", Elements.Create<KryptonPanel>("btnAssetsFolderPanel", pnl =>
                    {
                        var baseIsActive = ApplicationSettings.GetFolderPath("Base");
                        var btn = Elements.Create<KryptonButton>("btnAssetsFolder");
                        btn.Tag = new Dictionary<string, string>
                        {
                            { "Type", "Assets" },
                            { "Link", "inputAssetsFolder" }
                        };
                        btn.ButtonStyle = ButtonStyle.Alternate;
                        btn.Enabled = baseIsActive != null;
                        btn.Width = 150;
                        btn.Text = "Assets Folder";
                        btn.Dock = DockStyle.Right;
                        btn.Click += SelectFolder;
                        btn.Cursor = Cursors.Hand;

                        pnl.Height = 30;
                        pnl.Controls.Add(btn);
                    })),
                    new SettingItem("", Elements.Create<KryptonTextBox>("inputAssetsFolder", input =>
                    {
                        var baseIsActive = ApplicationSettings.GetFolderPath("Base");
                        var path = ApplicationSettings.GetFolderPath("Assets");
                        if (path != null && path != string.Empty)
                        {
                            var userProfile = baseIsActive ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                            string? result = "...\\" +
                                Path.GetRelativePath(userProfile, path);
                            input.Text = result;
                        }
                        else
                        {
                            input.Text = string.Empty;
                        }
                        input.Enabled = baseIsActive != null;
                        input.ReadOnly = true;
                        input.TabStop = false;
                        input.Cursor = Cursors.No;
                        input.MouseMove += DisabledTextBoxCursor;
                    })),
                    new SettingItem("Characters File", Elements.Create<KryptonPanel>("btnCharactersPathPanel", pnl =>
                    {
                        var baseIsActive = ApplicationSettings.GetFolderPath("Base");
                        var btn = Elements.Create<KryptonButton>("btnCharactersPath");
                        btn.Tag = new Dictionary<string, string>
                        {
                            { "Type", "Characters" },
                            { "Link", "inputCharactersPath" }
                        };
                        btn.ButtonStyle = ButtonStyle.Alternate;
                        btn.Enabled = baseIsActive != null;
                        btn.Width = 150;
                        btn.Text = "Characters File";
                        btn.Dock = DockStyle.Right;
                        btn.Click += SelectDirectory;
                        btn.Cursor = Cursors.Hand;

                        pnl.Height = 30;
                        pnl.Controls.Add(btn);
                    })),
                    new SettingItem("", Elements.Create<KryptonTextBox>("inputCharactersPath", input =>
                    {
                        input.TextChanged += (s, e) =>
                        {
                            var btnAdd = Elements.Query<KryptonButton>("btnSaveToScript");
                            var btnSync = Elements.Query<KryptonButton>("btnSyncCharacters");
                            var grid = Elements.Query<KryptonDataGridView>("dgvCharacters");

                            if (btnAdd != null && grid?.SelectedRows.Count > 0)
                            {
                                if (input.Text.Length > 0)
                                {
                                    btnAdd.Enabled = true;
                                }
                                else
                                {
                                    btnAdd.Enabled = false;
                                }
                            }

                            if (btnSync != null)
                            {
                                if (input.Text.Length > 0)
                                    btnSync.Enabled = true;
                                else
                                    btnSync.Enabled = false;
                            }
                        };

                        var baseIsActive = ApplicationSettings.GetFolderPath("Base");
                        var path = ApplicationSettings.GetFilePath("Characters");
                        if (path != null && path != string.Empty)
                        {
                            var userProfile = baseIsActive ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                            string? result = "...\\" +
                                Path.GetRelativePath(userProfile, path);
                            input.Text = result;
                        }
                        else
                        {
                            input.Text = String.Empty;
                        }
                        input.Enabled = baseIsActive != null;
                        input.ReadOnly = true;
                        input.TabStop = false;
                        input.Cursor = Cursors.No;
                        input.MouseMove += DisabledTextBoxCursor;
                    })),
                    new SettingItem("Script File", Elements.Create<KryptonPanel>("btnScriptPathPanel", pnl =>
                    {
                        var baseIsActive = ApplicationSettings.GetFolderPath("Base");
                        var btn = Elements.Create<KryptonButton>("btnScriptPath");
                        btn.Tag = new Dictionary<string, string>
                        {
                            { "Type", "Script" },
                            { "Link", "inputScriptsPath" }
                        };
                        btn.ButtonStyle = ButtonStyle.Alternate;
                        btn.Enabled = baseIsActive != null;
                        btn.Width = 150;
                        btn.Text = "Script File";
                        btn.Dock = DockStyle.Right;
                        btn.Click += SelectDirectory;
                        btn.Cursor = Cursors.Hand;

                        pnl.Height = 30;
                        pnl.Controls.Add(btn);
                    })),
                    new SettingItem("", Elements.Create<KryptonTextBox>("inputScriptsPath", input =>
                    {
                        var baseIsActive = ApplicationSettings.GetFolderPath("Base");
                        var path = ApplicationSettings.GetFilePath("Script");
                        if (path != null && path != string.Empty)
                        {
                            var userProfile = baseIsActive ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                            string? result = "...\\" +
                                Path.GetRelativePath(userProfile, path);
                            input.Text = result;
                        }
                        else
                        {
                            input.Text = String.Empty;
                        }
                        input.Enabled = baseIsActive != null;
                        input.ReadOnly = true;
                        input.TabStop = false;
                        input.Cursor = Cursors.No;
                        input.MouseMove += DisabledTextBoxCursor;
                    })),
                }),
                new SettingSection("Script Builder Settings", new List<SettingItem>
                {
                    new SettingItem("", Elements.Create<KryptonLabel>("lblConversions", lbl =>
                    {
                        lbl.Text = "Text Conversions";
                        lbl.StateCommon.ShortText.Font = new Font(this.Font.FontFamily, 10);
                    })),
                    new SettingItem("", Elements.Create<KryptonSeparator>("sepConversions", sep =>
                    {
                        sep.AllowMove = false;
                        sep.TabStop = false;
                        sep.Orientation = Orientation.Horizontal;
                        sep.StateCommon.Separator.Back.Color1 = Color.Maroon;
                        sep.StateCommon.Separator.Back.ColorStyle = PaletteColorStyle.Solid;
                    })),
                    new SettingItem("General Options", Elements.Create<KryptonTableLayoutPanel>("genConversionPanel", pnl =>
                    {
                        var lblEnabled = Elements.Create<KryptonLabel>("lblConversionColorEnabled");
                        var btnEnabled = Elements.Create<KryptonCheckButton>("btnConversionColorEnabled");

                        var lblAutoSync = Elements.Create<KryptonLabel>("lblAutoSync");
                        var chkAutoSync = Elements.Create<KryptonCheckBox>("chkAutoSync");

                        var lblIndentType = Elements.Create<KryptonLabel>("lblIndentType");
                        var slctIndent = Elements.Create<KryptonComboBox>("lstConversionIndentType");

                        var lblIndentAmnt = Elements.Create<KryptonLabel>("lblIndentAmnt");
                        var boxIndent = Elements.Create<KryptonTextBox>("boxConversionIndentAmount");

                        bool SyncIsChecked = ScriptConverter.CheckIsAutoSyncLabels();
                        bool ColorIsChecked = ScriptConverter.CheckIsFormattingColor();
                        string IndentType = ScriptConverter.GetIndentationType();

                        // -------------------------------- \\
                        // --------- Color Button --------- \\
                        // -------------------------------- \\
                        lblEnabled.Dock = DockStyle.Fill;
                        lblEnabled.Text = "Color Converted Scripts";
                        lblEnabled.Margin = new Padding(0);

                        btnEnabled.Dock = DockStyle.Fill ;
                        btnEnabled.Text = ColorIsChecked ? "Color Script" : "No Color";
                        btnEnabled.Margin = new Padding(3, 0, 3, 10);
                        btnEnabled.Checked = ColorIsChecked;
                        btnEnabled.ButtonStyle = ButtonStyle.Custom1;
                        btnEnabled.StateCommon.Content.ShortText.Color1 = ColorIsChecked ? Color.Black : Color.White;
                        btnEnabled.StateCommon.Back.Color1 = ColorIsChecked ? Color.LimeGreen: Color.Maroon;
                        btnEnabled.Cursor = Cursors.Hand;
                        btnEnabled.Click += (s, e) =>
                        {
                            bool IsChecked = ScriptConverter.CheckIsFormattingColor();
                            ScriptConverter.SetIsFormattingColor(!IsChecked);
                            ApplicationSettings.SetColorFormatting(!IsChecked);
                            btnEnabled.Text = !IsChecked ? "Color Script" : "No Color";
                            btnEnabled.StateCommon.Content.ShortText.Color1 = !IsChecked ? Color.Black : Color.White;
                            btnEnabled.StateCommon.Back.Color1 = !IsChecked ? Color.LimeGreen: Color.Maroon;
                            ShouldEnableSaveButton(true);
                        };

                        // --------------------------------------------- \\
                        // --------- Checkbox Should Auto-Sync --------- \\
                        // --------------------------------------------- \\
                        lblAutoSync.Dock = DockStyle.Fill;
                        lblAutoSync.Text = "Auto-Sync Labels";
                        lblAutoSync.Margin = new Padding(0);

                        chkAutoSync.Dock = DockStyle.Fill;
                        chkAutoSync.Text = SyncIsChecked ? "Enabled" : "Disabled";
                        chkAutoSync.Margin = new Padding(3, 0, 3, 10);
                        chkAutoSync.Checked = SyncIsChecked;
                        chkAutoSync.Cursor = Cursors.Hand;
                        chkAutoSync.Click += (s, e) =>
                        {
                            bool IsChecked = ScriptConverter.CheckIsAutoSyncLabels();
                            ScriptConverter.SetAutoSyncLabels(!IsChecked);
                            ApplicationSettings.SetAutoSyncLabels(!IsChecked);
                            chkAutoSync.Text = !IsChecked ? "Enabled" : "Disabled";
                            ShouldEnableSaveButton(true);
                        };
                        
                        // -------------------------------------- \\
                        // --------- Select Indent Type --------- \\
                        // -------------------------------------- \\
                        lblIndentType.Dock = DockStyle.Fill;
                        lblIndentType.Text = "Indentation Type";
                        lblIndentType.Margin = new Padding(0);

                        slctIndent.Dock = DockStyle.Fill;
                        slctIndent.Margin = new Padding(3, 0, 3, 40);
                        slctIndent.DropDownStyle = ComboBoxStyle.DropDownList;
                        slctIndent.Items.AddRange([
                            "Spaces",
                            "Tab"
                        ]);
                        slctIndent.SelectedIndex = IndentType == "Spaces" ? 0 : 1;
                        slctIndent.Cursor = Cursors.Hand;
                        slctIndent.SelectedIndexChanged += (s, e) =>
                        {
                            ApplicationSettings.SetIndentationType(slctIndent.SelectedIndex == 0 ? "Spaces" : "Tab");
                            ScriptConverter.ChangeIndentationType(slctIndent.SelectedIndex == 0 ? "Spaces" : "Tab");
                            ShouldEnableSaveButton(true);
                        };
                        
                        // ------------------------------------- \\
                        // --------- Box Indent Amount --------- \\
                        // ------------------------------------- \\
                        lblIndentAmnt.Dock = DockStyle.Fill;
                        lblIndentAmnt.Text = "Indentation Amount (Spaces)";
                        lblIndentAmnt.Margin = new Padding(0);

                        boxIndent.Dock = DockStyle.Fill;
                        boxIndent.Margin = new Padding(3, 0, 3, 40);
                        boxIndent.Text = ScriptConverter.GetIndentationAmount().ToString();
                        boxIndent.KeyPress += (s, e) =>
                        {
                            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                            {
                                e.Handled = true;
                            }
                        };
                        boxIndent.TextChanged += (s, e) =>
                        {
                            if (boxIndent.Text == String.Empty) return;

                            int.TryParse(boxIndent.Text, out int value);
                            if (value < 2)
                            {
                                value = 2;
                                boxIndent.Text = "2";
                                boxIndent.SelectionStart = 1;
                                boxIndent.SelectionLength = 0;
                            }
                            else if (value > 8)
                            {
                                value = 8;
                                boxIndent.Text = "8";
                                boxIndent.SelectionStart = 1;
                                boxIndent.SelectionLength = 0;
                            }
                            else boxIndent.Text = value.ToString();

                            ScriptConverter.ChangeIndentationAmount(value);
                            ApplicationSettings.SetIndentationAmount(value);
                            ShouldEnableSaveButton(true);
                        };
                        boxIndent.LostFocus += (s, e) =>
                        {
                            if (boxIndent.Text == String.Empty)
                            {
                                boxIndent.Text = ScriptConverter.GetIndentationAmount().ToString();
                            }
                        };

                        pnl.Width = 400;
                        pnl.Height = 140;

                        pnl.RowCount = 0;
                        pnl.ColumnCount = 0;

                        pnl.RowCount++;
                        pnl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        pnl.RowCount++;
                        pnl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        pnl.RowCount++;
                        pnl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        pnl.RowCount++;
                        pnl.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                        pnl.ColumnCount++;
                        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                        pnl.ColumnCount++;
                        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

                        pnl.Controls.Add(lblEnabled);
                        pnl.Controls.Add(btnEnabled);
                        pnl.SetRow(lblEnabled, 0);
                        pnl.SetRow(btnEnabled, 1);
                        pnl.SetColumn(btnEnabled, 0);
                        pnl.SetColumn(lblEnabled, 0);

                        pnl.Controls.Add(lblAutoSync);
                        pnl.Controls.Add(chkAutoSync);
                        pnl.SetRow(lblEnabled, 0);
                        pnl.SetRow(btnEnabled, 1);
                        pnl.SetColumn(btnEnabled, 1);
                        pnl.SetColumn(lblEnabled, 1);

                        pnl.Controls.Add(lblIndentType);
                        pnl.Controls.Add(slctIndent);
                        pnl.SetRow(lblIndentType, 2);
                        pnl.SetRow(slctIndent, 3);
                        pnl.SetColumn(lblIndentType, 0);
                        pnl.SetColumn(slctIndent, 0);

                        pnl.Controls.Add(lblIndentAmnt);
                        pnl.Controls.Add(boxIndent);
                        pnl.SetRow(lblIndentAmnt, 2);
                        pnl.SetRow(boxIndent, 3);
                        pnl.SetColumn(lblIndentAmnt, 1);
                        pnl.SetColumn(boxIndent, 1);
                    })),
                    new SettingItem("Comments", Elements.Create<KryptonPanel>("pnlConversionComments", pnl =>
                    {
                        var box = Elements.Create<KryptonTextBox>("inputConversionComments");
                        var check = Elements.Create<KryptonCheckButton>("chkConversionComments");
                        var rule = ruleMap.GetValueOrDefault("Comment");

                        pnl.Width = 400;
                        pnl.Height = 25;

                        SetCheckAndBoxElementSettings(box, check, rule);

                        pnl.Controls.Add(check);
                        pnl.Controls.Add(box);
                    })),
                    new SettingItem("Empty Lines", Elements.Create<KryptonPanel>("pnlConversionEmpty", pnl =>
                    {
                        var box = Elements.Create<KryptonTextBox>("inputConversionEmpty");
                        var check = Elements.Create<KryptonCheckButton>("chkConversionEmpty");
                        var rule = ruleMap.GetValueOrDefault("Empty");

                        pnl.Width = 400;
                        pnl.Height = 25;

                        SetCheckAndBoxElementSettings(box, check, rule);

                        pnl.Controls.Add(check);
                        pnl.Controls.Add(box);
                    })),
                    new SettingItem("String Action", Elements.Create<KryptonPanel>("pnlConversionStringAction", pnl =>
                    {
                        var box = Elements.Create<KryptonTextBox>("inputConversionStringAction");
                        var check = Elements.Create<KryptonCheckButton>("chkConversionStringAction");
                        var rule = ruleMap.GetValueOrDefault("StringAction");

                        pnl.Width = 400;
                        pnl.Height = 25;

                        SetCheckAndBoxElementSettings(box, check, rule);

                        pnl.Controls.Add(check);
                        pnl.Controls.Add(box);
                    })),
                    new SettingItem("Object Action (Enclosed)", Elements.Create<KryptonPanel>("pnlConversionObjectEnclosedAction", pnl =>
                    {
                        var box = Elements.Create<KryptonTextBox>("inputConversionObjectEnclosedAction");
                        var check = Elements.Create<KryptonCheckButton>("chkConversionObjectEnclosedAction");
                        var rule = ruleMap.GetValueOrDefault("ObjectEnclosedAction");

                        pnl.Width = 400;
                        pnl.Height = 25;

                        SetCheckAndBoxElementSettings(box, check, rule);

                        pnl.Controls.Add(check);
                        pnl.Controls.Add(box);
                    })),
                    new SettingItem("Object Action (Opening)", Elements.Create<KryptonPanel>("pnlConversionObjectActionOpen", pnl =>
                    {
                        var box = Elements.Create<KryptonTextBox>("inputConversionObjectActionOpen");
                        var check = Elements.Create<KryptonCheckButton>("chkConversionObjectActionOpen");
                        var rule = ruleMap.GetValueOrDefault("ObjectActionOpen");

                        pnl.Width = 400;
                        pnl.Height = 25;

                        SetCheckAndBoxElementSettings(box, check, rule);

                        pnl.Controls.Add(check);
                        pnl.Controls.Add(box);
                    })),
                    new SettingItem("Object Action (Closing)", Elements.Create<KryptonPanel>("pnlConversionObjectActionClose", pnl =>
                    {
                        var box = Elements.Create<KryptonTextBox>("inputConversionObjectActionClose");
                        var check = Elements.Create<KryptonCheckButton>("chkConversionObjectActionClose");
                        var rule = ruleMap.GetValueOrDefault("ObjectActionClose");

                        pnl.Width = 400;
                        pnl.Height = 25;

                        SetCheckAndBoxElementSettings(box, check, rule);

                        pnl.Controls.Add(check);
                        pnl.Controls.Add(box);
                    })),
                    new SettingItem("Character Dialog", Elements.Create<KryptonPanel>("pnlConversionCharacterLine", pnl =>
                    {
                        var box = Elements.Create<KryptonTextBox>("inputConversionCharacterLine");
                        var check = Elements.Create<KryptonCheckButton>("chkConversionCharacterLine");
                        var rule = ruleMap.GetValueOrDefault("CharacterLine");

                        pnl.Width = 400;
                        pnl.Height = 25;

                        SetCheckAndBoxElementSettings(box, check, rule);

                        pnl.Controls.Add(check);
                        pnl.Controls.Add(box);
                    })),
                    new SettingItem("Narration", Elements.Create<KryptonPanel>("pnlConversionNarration", pnl =>
                    {
                        var box = Elements.Create<KryptonTextBox>("inputConversionNarration");
                        var check = Elements.Create<KryptonCheckButton>("chkConversionNarration");
                        var rule = ruleMap.GetValueOrDefault("Narration");

                        pnl.Width = 400;
                        pnl.Height = 25;

                        SetCheckAndBoxElementSettings(box, check, rule);

                        pnl.Controls.Add(check);
                        pnl.Controls.Add(box);
                    })),
                    new SettingItem("", Elements.Create<KryptonPanel>("pnlConversionSave", pnl =>
                    {
                        var ConversionSaveButton = Elements.Create<KryptonButton>("btnConversionSave");
                        ConversionSaveButton.ButtonStyle = ButtonStyle.Alternate;
                        ConversionSaveButton.Text = "Save Changes";
                        ConversionSaveButton.Height = 40;
                        ConversionSaveButton.Width = 150;
                        ConversionSaveButton.Dock = DockStyle.Right;
                        ConversionSaveButton.Enabled = false;
                        ConversionSaveButton.Cursor = Cursors.Hand;
                        ConversionSaveButton.Click += ConversionSaveButton_Click;

                        pnl.Height = 40;
                        pnl.Controls.Add(ConversionSaveButton);
                    })),
                })
            };
            #endregion
        }

        private void SetCheckAndBoxElementSettings(KryptonTextBox box, KryptonCheckButton button, ConversionRule? rule)
        {
            box.Text = rule?.Pattern.ToString();
            box.Width = 290;
            box.Dock = DockStyle.Right;
            box.TextChanged += ValidateRegex;

            button.Dock = DockStyle.Left;
            button.Width = 100;
            button.Text = rule?.IsEnabled ?? true ? "Enabled" : "Disabled";
            button.Checked = rule?.IsEnabled ?? true;
            button.ButtonStyle = ButtonStyle.Custom1;
            button.StateCommon.Content.ShortText.Color1 = rule?.IsEnabled ?? true ? Color.Black : Color.White;
            button.StateCommon.Back.Color1 = rule?.IsEnabled ?? true ? Color.LimeGreen : Color.Maroon;
            button.Cursor = Cursors.Hand;
            button.Click += (s, e) =>
            {
                if (rule != null)
                {
                    rule.IsEnabled = !rule.IsEnabled;
                }
                button.Text = rule?.IsEnabled ?? true ? "Enabled" : "Disabled";
                button.StateCommon.Content.ShortText.Color1 = rule?.IsEnabled ?? true ? Color.Black : Color.White;
                button.StateCommon.Back.Color1 = rule?.IsEnabled ?? true ? Color.LimeGreen : Color.Maroon;
            };
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            InitializeSettings();

            int scrollWidth = SystemInformation.VerticalScrollBarWidth;
            tlpSettings.RowCount = 0;
            tlpSettings.RowStyles.Clear();
            tlpSettings.Padding = new Padding(scrollWidth, 0, scrollWidth, 0);

            foreach (var section in Sections)
            {
                AddHeader(section.Header);

                foreach (var item in section.Items)
                {
                    if (string.IsNullOrEmpty(item.Label))
                    {
                        AddFullWidthControl(item.Control);
                    }
                    else
                    {
                        AddInputRow(item.Control, item.Label);
                    }
                }
            }
        }

        private int GetNextRowIndex()
        {
            tlpSettings.RowCount++;
            tlpSettings.RowStyles.Add(new RowStyle(SizeType.Absolute, 0F));
            return tlpSettings.RowCount - 1;
        }

        private void AddHeader(string text)
        {
            int row = GetNextRowIndex();

            KryptonLabel lbl = new KryptonLabel
            {
                Text = text,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font(this.Font, FontStyle.Bold),
                Margin = new Padding(3, 10, 3, 5),
                StateCommon =
                {
                    ShortText =
                    {
                        Color1 = Color.White,
                        Font = new Font(this.Font.FontFamily, 14, FontStyle.Bold)
                    }
                }
            };

            tlpSettings.RowStyles[row].Height = lbl.PreferredSize.Height + lbl.Margin.Vertical;

            tlpSettings.Controls.Add(lbl, 0, row);
            tlpSettings.SetColumnSpan(lbl, 2);
        }

        private void AddFullWidthControl(Control control)
        {
            int row = GetNextRowIndex();

            control.Margin = new Padding(3);
            control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            tlpSettings.RowStyles[row].Height = control.Height + control.Margin.Vertical;

            tlpSettings.Controls.Add(control, 0, row);
            tlpSettings.SetColumnSpan(control, 2);
        }

        private void AddInputRow(Control inputControl, string labelText)
        {
            int row = GetNextRowIndex();

            int intendedInputHeight = inputControl.Height;

            KryptonLabel lbl = new KryptonLabel
            {
                Text = labelText,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Margin = new Padding(6),
                StateCommon = { ShortText = { Color1 = Color.White } }
            };
            tlpSettings.Controls.Add(lbl, 0, row);

            inputControl.Margin = new Padding(3);
            inputControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tlpSettings.Controls.Add(inputControl, 1, row);

            int labelHeight = lbl.PreferredSize.Height + lbl.Margin.Vertical;
            int inputHeight = intendedInputHeight + inputControl.Margin.Vertical;
            tlpSettings.RowStyles[row].Height = Math.Max(labelHeight, inputHeight);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ConversionSaveButton_Click(object? sender, EventArgs e)
        {
            var SaveButton = Elements.Query<KryptonButton>("btnConversionSave");
            if (SaveButton != null)
            {
                var CommentsBox = Elements.Query<KryptonTextBox>("inputConversionComments");
                var EmptyBox = Elements.Query<KryptonTextBox>("inputConversionEmpty");
                var StringActionBox = Elements.Query<KryptonTextBox>("inputConversionStringAction");
                var ObjectEnclosedActionBox = Elements.Query<KryptonTextBox>("inputConversionObjectEnclosedAction");
                var ObjectActionOpenBox = Elements.Query<KryptonTextBox>("inputConversionObjectActionOpen");
                var ObjectActionCloseBox = Elements.Query<KryptonTextBox>("inputConversionObjectActionClose");
                var CharacterDialogBox = Elements.Query<KryptonTextBox>("inputConversionCharacterLine");
                var NarrationBox = Elements.Query<KryptonTextBox>("inputConversionNarration");

                if (
                    CommentsBox != null &&
                    EmptyBox != null &&
                    StringActionBox != null &&
                    ObjectEnclosedActionBox != null &&
                    ObjectActionOpenBox != null &&
                    ObjectActionCloseBox != null &&
                    CharacterDialogBox != null &&
                    NarrationBox != null
                )
                {
                    Dictionary<string, Regex> dict = new()
                    {
                        { "Comment", new Regex(CommentsBox.Text) },
                        { "Empty", new Regex(EmptyBox.Text) },
                        { "StringAction", new Regex(StringActionBox.Text) },
                        { "ObjectEnclosedAction", new Regex(ObjectEnclosedActionBox.Text) },
                        { "ObjectActionOpen", new Regex(ObjectActionOpenBox.Text) },
                        { "ObjectActionClose", new Regex(ObjectActionCloseBox.Text) },
                        { "CharacterLine", new Regex(CharacterDialogBox.Text) },
                        { "Narration", new Regex(NarrationBox.Text) },
                    };
                    ScriptConverter.ConversionRules.ForEach(r =>
                    {
                        if (r.Name != null && dict.TryGetValue(r.Name, out Regex? value))
                        {
                            r.Pattern = value;
                        }
                    });

                    ScriptConverter.SaveSettings();
                    SaveButton.Enabled = false;
                }
                else
                {
                    DialogBox.Show(
                        "Something went wrong while saving...\nIf the issue persists, please contact the developer.",
                        "Error",
                        DialogButtonDefaults.OK,
                        DialogIcon.Error);
                    return;
                }

                    ApplicationSettings.SaveDirectories();
            }

        }

        private void AddCharacter(object? sender, EventArgs e)
        {
            using (AddModifyCharacter CharacterScreen = new AddModifyCharacter(CharacterData))
            {
                CharacterScreen.ShowDialog();
            }
        }

        private void RemoveCharacter(object? sender, EventArgs e)
        {
            var dgv = Elements.Query<KryptonDataGridView>("dgvCharacters");

            if (dgv?.SelectedRows.Count > 0)
            {
                int selectedCount = dgv.SelectedRows.Count;
                int maxAmount = 10;
                string phrasing = dgv.SelectedRows.Count > 1 ? "these characters" : "this character";
                var characterNames = string.Join("\n", dgv.SelectedRows
                    .Cast<DataGridViewRow>()
                    .Take(maxAmount)
                    .Select(row => $"- {row.Cells[1].Value}"));

                if (selectedCount > maxAmount)
                {
                    characterNames += $"\n- And {selectedCount - maxAmount} more...";
                }

                var question = DialogBox.Show($"Are you sure you wish to remove {phrasing}?\r\n{characterNames}",
                    "Confirm Removal",
                    600,
                    DialogIcon.Question,
                    new DialogButton("From Program", DialogResult.Continue),
                    new DialogButton("From Script", DialogResult.Retry),
                    new DialogButton("From Both", DialogResult.Yes, ButtonStyle.Custom2),
                    new DialogButton("Cancel", DialogResult.No, ButtonStyle.Custom2));
                if (question == DialogResult.Continue || question == DialogResult.Retry || question == DialogResult.Yes)
                {
                    try
                    {
                        List<int> ids = new List<int>();
                        foreach (DataGridViewRow row in dgv.SelectedRows)
                        {
                            int id = (int)row.Cells[0].Value;

                            if (question != DialogResult.Yes &&
                                question != DialogResult.Retry)
                                CharacterData.RemoveCharacter(id);

                            ids.Add(id);
                        }

                        if (question == DialogResult.Retry)
                        {
                            foreach (int id in ids)
                            {
                                if (CharacterData.CharacterExistsInScript(id))
                                {
                                    CharacterData.RemoveCharacterFromScript(id, false);
                                }
                            }
                        }

                        if (question == DialogResult.Yes)
                        {
                            foreach (int id in ids)
                            {
                                if (CharacterData.CharacterExistsInScript(id))
                                {
                                    CharacterData.RemoveCharacterFromScript(id, false);
                                }
                                CharacterData.RemoveCharacter(id);
                            }
                        }

                        CharacterData.SaveCharacters();
                    }
                    catch (Exception error)
                    {
                        DialogBox.Show(
                            $"Something went wrong when removing characters!\n\n{error}",
                            "Failed to Remove Characters",
                            DialogButtonDefaults.OK,
                            DialogIcon.Error);
                    }
                }
            }
        }

        private void UpdateCharacter(object? sender, EventArgs e)
        {
            var dgv = Elements.Query<KryptonDataGridView>("dgvCharacters");

            if (dgv?.SelectedRows.Count > 0)
            {
                var selectedRow = dgv.SelectedRows[0];
                int selectedIndex = selectedRow.Index;
                int characterId = (int)selectedRow.Cells[0].Value;
                Character? character = CharacterData.CheckCharacter(characterId);
                if (character != null)
                {
                    using (AddModifyCharacter form = new AddModifyCharacter(CharacterData)
                    {
                        IsModifying = true,
                        CurrentCharacter = character
                    })
                    {
                        form.ShowDialog();
                        dgv.ClearSelection();
                        dgv.CurrentCell = dgv.Rows[selectedIndex].Cells[0];
                        dgv.Rows[selectedIndex].Selected = true;
                    }
                }
            }
        }

        private void SyncCharacters(object? sender, EventArgs e)
        {
            var characterData = CharacterData.SyncCharacters();
            var characterStructList = new List<CharacterStructure>();
            var characterList = new List<Character>();
            var duplicates = new List<string>();

            foreach (CharacterStructure character in characterData.Values)
            {
                if (CharacterData.CheckedDuplicates(character))
                {
                    duplicates.Add(character.Tag);
                }
            }

            if (duplicates.Count > 0)
            {
                int selectedCount = duplicates.Count;
                int maxAmount = 10;
                string phrasing = duplicates.Count > 1 ? "multiple characters" : "a character";
                string tags = string.Join("\n", duplicates.Take(maxAmount).Select(c => $"- {c}"));

                if (selectedCount > maxAmount)
                {
                    tags += $"\n- And {selectedCount - maxAmount} more...";
                }

                var result = DialogBox.Show(
                    $"Found {phrasing} with a similar tag that already exist...\nDo you want to merge them?\r\n{tags}",
                    "Confirm Merge Status",
                    DialogButtonDefaults.YesNo,
                    DialogIcon.Question);

                if (result == DialogResult.Yes)
                {
                    foreach (string character in duplicates)
                    {
                        int characterId = CharacterData.AllCharacters.First(c => c.Tag == character).CharacterID;
                        string name = characterData[character].Name;
                        string tag = characterData[character].Tag;
                        string? color = characterData[character].Color;
                        string? directory = characterData[character].Directory;

                        if (color != null && directory != null)
                        {
                            CharacterData.UpdateCharacter(
                                characterId,
                                new Normal(name, tag, color, directory));
                        }
                        else if (color != null || directory != null)
                        {
                            CharacterData.UpdateCharacter(
                                characterId,
                                new Normal(name, tag, color ?? directory ?? string.Empty));
                        }
                        else
                        {
                            CharacterData.UpdateCharacter(
                                characterId,
                                new Normal(name, tag));
                        }

                        characterData.Remove(character);
                    }

                    characterStructList.AddRange(characterData.Values.ToList());
                    characterList = NewCharactersFromData(characterStructList);
                }
                else
                {
                    foreach (string character in duplicates)
                    {
                        characterData.Remove(character);
                    }

                    characterStructList.AddRange(characterData.Values.ToList());
                    characterList = NewCharactersFromData(characterStructList);
                }
            }

            foreach (Character character in characterList)
            {
                CharacterData.AddCharacter(character);
            }
        }

        private void SaveCharactersToScript(object? sender, EventArgs e)
        {
            var grid = Elements.Query<KryptonDataGridView>("dgvCharacters");
            var selectedRows = grid?.SelectedRows;
            if (selectedRows != null && selectedRows.Count > 0)
            {
                var result = DialogBox.Show(
                    $"Would you like to update the selected characters, or all characters?",
                    "Save Characters To Script",
                    DialogIcon.Question,
                    new DialogButton("Selected", DialogResult.Continue, ButtonStyle.Alternate),
                    new DialogButton("All", DialogResult.Yes, ButtonStyle.Alternate),
                    new DialogButton("Cancel", DialogResult.No, ButtonStyle.Custom2));
                if (result == DialogResult.Continue || result == DialogResult.Yes)
                {
                    try
                    {
                        List<(int, string, bool)> tags = new List<(int, string, bool)>();
                        int maxCharacterMerge = 10;
                        int currentCharacterMerge = 0;
                        string mergeDialog = "";
                        int index = 0;

                        foreach (DataGridViewRow row in selectedRows)
                        {
                            var inScript = CharacterData.CharacterExistsInScript((string)row.Cells[2].Value);
                            (int characterId, string characterTag, bool isInScript) = (
                                (int)row.Cells[0].Value,
                                (string)row.Cells[2].Value,
                                inScript);

                            if (inScript)
                            {
                                if (currentCharacterMerge < maxCharacterMerge)
                                {
                                    mergeDialog += $"- {row.Cells[1].Value} ({row.Cells[2].Value})\n";
                                }
                                currentCharacterMerge++;
                            }

                            tags.Add((characterId, characterTag, isInScript));

                            if (index == selectedRows.Count - 1)
                            {
                                if (currentCharacterMerge > maxCharacterMerge)
                                {
                                    mergeDialog += $"- And {currentCharacterMerge - maxCharacterMerge} more...";
                                }
                            }

                            index++;
                        }

                        if (currentCharacterMerge > 0)
                        {
                            if (DialogBox.Show(
                                $"Detected characters that already exist within the script.\nWould you like to merge existing characters?\n(Regardless of the answer, any new characters will be added)\n\n{mergeDialog}",
                                "Existing Characters Detected",
                                DialogButtonDefaults.YesNo,
                                DialogIcon.Warning) == DialogResult.No)
                            {
                                Predicate<(int, string, bool)> value = tuple => tuple.Item3;
                                tags.RemoveAll(value);
                            }
                        }

                        foreach (var (id, tag, inScript) in tags)
                        {
                            var character = CharacterData.CheckCharacter(id);
                            if (character != null)
                            {
                                var content = CharacterData.ConvertToScriptContent(character);
                                if (inScript)
                                {
                                    CharacterData.UpdateCharacterInScript(tag, content);
                                }
                                else
                                {
                                    CharacterData.AddCharacterToScript(tag, content);
                                }
                            }
                        }

                        DialogBox.Show(
                            "Characters successfully saved to script!",
                            "Success",
                            DialogButtonDefaults.OK,
                            DialogIcon.Information);
                    }
                    catch (Exception error)
                    {
                        DialogBox.Show(
                            $"Something went wrong when adding characters!\n\n{error}",
                            "Failed to Add Characters",
                            DialogButtonDefaults.OK,
                            DialogIcon.Error);
                    }
                }
            }
        }

        private List<Character> NewCharactersFromData(List<CharacterStructure> characters)
        {
            var list = new List<Character>();
            foreach (CharacterStructure character in characters)
            {
                Normal? newCharacter = null;
                if (character.Color != null && character.Directory != null)
                {
                    newCharacter = new Normal(character.Name, character.Tag, character.Color, character.Directory);
                }
                else if (character.Color != null || character.Directory != null)
                {
                    string? colorOrDirectory = character.Color ?? character.Directory;
                    if (colorOrDirectory != null)
                    {
                        newCharacter = new Normal(character.Name, character.Tag, colorOrDirectory);
                    }
                }
                else
                {
                    newCharacter = new Normal(character.Name, character.Tag);
                }

                if (newCharacter != null)
                {
                    list.Add(newCharacter);
                }
            }

            return list;
        }

        private void SelectDirectory(object? sender, EventArgs e)
        {
            var baseIsActive = ApplicationSettings.GetFolderPath("Base");
            FilePath.Filter = "JavaScript files (*.js)|*.js|All files (*.*)|*.*";
            FilePath.InitialDirectory = baseIsActive ?? string.Empty;
            FilePath.FilterIndex = 1;

            KryptonButton? btn = sender as KryptonButton;
            if (btn != null && FilePath.ShowDialog() == DialogResult.OK)
            {
                // Technically the "Tag" should never be empty, but the compiler doesn't know that,
                // so it constantly throws warnings that muddy the Error List.
                var tag = (Dictionary<string, string>?)btn.Tag;
                var input = Elements.Query<KryptonTextBox>(tag?["Link"] ?? string.Empty);
                if (input != null)
                {
                    string filePath = FilePath.FileName;
                    var userProfile = baseIsActive ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    string? result = "...\\" +
                        Path.GetRelativePath(userProfile, filePath);
                    input.Text = result;
                    ApplicationSettings.AddReplaceFilePath(tag?["Type"] ?? string.Empty, filePath);
                    ApplicationSettings.SaveDirectories();
                }
            }

            FilePath.Dispose();
        }

        private void SelectFolder(object? sender, EventArgs e)
        {
            KryptonButton? btn = sender as KryptonButton;
            if (btn != null && FolderPath.ShowDialog() == DialogResult.OK)
            {
                var btnTag = btn.Tag as Dictionary<string, string>;
                bool btnIsBase = btnTag != null && btnTag.TryGetValue("Type", out string? value) && value == "Base";
                var baseIsActive = ApplicationSettings.GetFolderPath("Base");
                FolderPath.InitialDirectory = baseIsActive ?? string.Empty;

                var tag = (Dictionary<string, string>?)btn.Tag;
                var input = Elements.Query<KryptonTextBox>(tag?["Link"] ?? string.Empty);
                if (input != null)
                {
                    string folderPath = FolderPath.SelectedPath;
                    var userProfile = !btnIsBase ?
                        baseIsActive ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) :
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    string? result = "...\\" +
                        Path.GetRelativePath(userProfile, folderPath);
                    input.Text = result;
                    ApplicationSettings.AddReplaceFolderPath(tag?["Type"] ?? string.Empty, folderPath);
                    ApplicationSettings.SaveDirectories();
                }
            }

            FolderPath.Dispose();
        }

        private void CheckPathSelectors(object? sender, EventArgs e)
        {
            var baseIsActive = ApplicationSettings.GetFolderPath("Base");

            var btnAssets = Elements.Query<KryptonButton>("btnAssetsFolder");
            var btnCharacters = Elements.Query<KryptonButton>("btnCharactersPath");
            var btnScript = Elements.Query<KryptonButton>("btnScriptPath");

            var boxAssets = Elements.Query<KryptonTextBox>("inputAssetsFolder");
            var boxCharacters = Elements.Query<KryptonTextBox>("inputCharactersPath");
            var boxScript = Elements.Query<KryptonTextBox>("inputScriptsPath");
            if (baseIsActive != null)
            {
                if (baseIsActive != SelectedBasePath)
                {
                    if (boxAssets != null)
                    {
                        boxAssets.Text = "";
                        ApplicationSettings.AddReplaceFolderPath("Assets", string.Empty);
                    }
                    if (boxCharacters != null)
                    {
                        boxCharacters.Text = "";
                        ApplicationSettings.AddReplaceFilePath("Characters", string.Empty);
                    }
                    if (boxScript != null)
                    {
                        boxScript.Text = "";
                        ApplicationSettings.AddReplaceFilePath("Script", string.Empty);
                    }
                }

                if (btnAssets != null) btnAssets.Enabled = true;
                if (btnCharacters != null) btnCharacters.Enabled = true;
                if (btnScript != null) btnScript.Enabled = true;

                if (boxAssets != null) boxAssets.Enabled = true;
                if (boxCharacters != null) boxCharacters.Enabled = true;
                if (boxScript != null) boxScript.Enabled = true;

                ApplicationSettings.SaveDirectories();
            }
            else
            {
                if (btnAssets != null) btnAssets.Enabled = false;
                if (btnCharacters != null) btnCharacters.Enabled = false;
                if (btnScript != null) btnScript.Enabled = false;

                if (boxAssets != null)
                {
                    boxAssets.Enabled = false;
                    boxAssets.Text = "";
                    ApplicationSettings.AddReplaceFolderPath("Assets", string.Empty);
                }
                if (boxCharacters != null)
                {
                    boxCharacters.Enabled = false;
                    boxCharacters.Text = "";
                    ApplicationSettings.AddReplaceFilePath("Characters", string.Empty);
                }
                if (boxScript != null)
                {
                    boxScript.Enabled = false;
                    boxScript.Text = "";
                    ApplicationSettings.AddReplaceFilePath("Script", string.Empty);
                }
            }
        }

        private void ValidateRegex(object? sender, EventArgs e)
        {
            var Savebutton = Elements.Query<KryptonButton>("btnConversionSave");
            var input = sender as KryptonTextBox;
            if (input != null && Savebutton != null)
            {
                if (string.IsNullOrEmpty(input.Text))
                {
                    input.StateCommon.Back.Color1 = Color.MistyRose;
                    IsRegexInvalid = true;
                    ShouldEnableSaveButton(false);
                }
                else
                {
                    try
                    {
                        _ = new Regex(input.Text);
                        input.StateCommon.Back.Color1 = SystemColors.Window;
                        IsRegexInvalid = false;
                        ShouldEnableSaveButton(true);
                    }
                    catch (ArgumentException)
                    {
                        input.StateCommon.Back.Color1 = Color.Orchid;
                        IsRegexInvalid = true;
                        ShouldEnableSaveButton(false);
                    }
                }
            }
        }

        private void ShouldEnableSaveButton(bool value)
        {
            var SaveButton = Elements.Query<KryptonButton>("btnConversionSave");
            if (SaveButton != null)
            {
                if (IsRegexInvalid)
                {
                    SaveButton.Enabled = false;
                }
                else
                {
                    SaveButton.Enabled = value;
                }
            }
        }

        private void DisabledTextBoxCursor(object? sender, MouseEventArgs e)
        {
            var box = sender as KryptonTextBox;
            if (box != null)
            {
                if (!box.ClientRectangle.Contains(e.Location))
                {
                    box.Capture = false;
                }
                else if (!box.Capture)
                {
                    box.Capture = true;
                }
            }
        }
    }
}