using System;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Configuration;

namespace FileParser.Views
{
    public partial class ConverterControl
    {
        public ConverterControl()
        {
            InitializeComponent();
        }



		private void Hyperlink_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			
			// Create OpenFileDialog
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			// Set filter for file extension and default file extension
			dlg.DefaultExt = ".xml";
			dlg.Filter = "Text documents (.xml)|*.xml";

			// Display OpenFileDialog by calling ShowDialog method
			Nullable<bool> result = dlg.ShowDialog();

			// Get the selected file name and display in a TextBox
			if (result == true)
			{
				// Open document
				string filename = dlg.FileName;
				TxtBlk.Text= filename;
				FileNameTextBox.Text = filename;

			}
		}

		

	}
}
