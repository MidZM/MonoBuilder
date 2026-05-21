using System;
using System.Collections.Generic;
using System.Text;

namespace MonoBuilder.Models.generics.enums
{
	/// <summary>Specify default dialog button values.</summary>
	public enum DialogButtonDefaults
	{
		/// <summary>The dialog box is given an OK button.</summary>
		OK,
		/// <summary>The dialog box is given an OK and Cancel button.</summary>
		OKCancel,
		/// <summary>The dialog box is given an Abort, Retry, and Ignore button.</summary>
		AbortRetryIgnore,
		/// <summary>The dialog box is given a Yes, No, and Cancel button.</summary>
		YesNoCancel,
		/// <summary>The dialog box is given a Yes and No button.</summary>
		YesNo,
		/// <summary>The dialog box is given a Retry and Cancel button.</summary>
		RetryCancel,
		/// <summary>The dialog box is given a Try Again, Continue, and Cancel button.</summary>
		CancelTryContinue
	}

	/// <summary>Specify default dialog icon values.</summary>
	public enum DialogIcon
	{
		/// <summary>An image of a white X in the middle of a red circle.</summary>
		Error,
		/// <summary>An image of an exclamation mark in the middle of a yellow triangle.</summary>
		Warning,
		/// <summary>An image of a question mark in the middle of a blue circle.</summary>
		Question,
		/// <summary>An image of a lower case i in the middle of a blue circle.</summary>
		Information
	}

	/// <summary>Specifies identifiers to indicate the return value of a dialog box.</summary>
	public enum DialogBoxResult
	{
		/// <summary>Nothing is returned from the dialog box.</summary>
		None = 0,
		/// <summary>The dialog box return value is OK (usually sent from a button labeled OK).</summary>
		OK = 1,
		/// <summary>The dialog box return value is Cancel (usually sent from a button labeled Cancel).</summary>
		Cancel = 2,
		/// <summary>The dialog box return value is Abort (usually sent from a button labeled Abort).</summary>
		Abort = 3,
		/// <summary>The dialog box return value is Retry (usually sent from a button labeled Retry).</summary>
		Retry = 4,
		/// <summary>The dialog box return value is Ignore (usually sent from a button labeled Ignore).</summary>
		Ignore = 5,
		/// <summary>The dialog box return value is Yes (usually sent from a button labeled Yes).</summary>
		Yes = 6,
		/// <summary>The dialog box return value is No (usually sent from a button labeled No).</summary>
		No = 7,
		/// <summary>The dialog box return value is Try Again (usually sent from a button labeled Try Again).</summary>
		TryAgain = 10,
		/// <summary>The dialog box return value is Continue (usually sent from a button labeled Continue).</summary>
		Continue = 11
	}
}
