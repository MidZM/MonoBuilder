using System;
using System.Collections.Generic;
using System.Text;

namespace MonoBuilder.Models.generics.enums
{
	/// <summary>
	/// Specifies the types of content boxes that can be used for user input or selection in a user interface.
	/// </summary>
	/// <remarks>This enumeration defines various input controls, including text boxes, file selection buttons,
	/// color pickers, and combo boxes, with variants indicating whether the input is required or editable. Use these
	/// values to configure or identify the type of input control to display or process.</remarks>
	public enum ContentBoxType
	{
		TextBox,
		RequiredTextBox,
		FileButton,
		RequiredFileButton,
		ColorButton,
		ComboBox,
		RequiredComboBox,
		EditableComboBox,
		RequiredEditableComboBox,
		FileBox
	}
}
