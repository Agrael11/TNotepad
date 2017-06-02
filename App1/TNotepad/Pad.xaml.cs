using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TNotepad
{
    public sealed partial class Pad : UserControl
    {
        public string FileName = "Unnamed File";

        public StorageFile file = null;

        public bool changed = false;

        public bool canClose = false;

        public Pad()
        {
            this.InitializeComponent();
        }

        public async System.Threading.Tasks.Task OpenFileAct(StorageFile file)
        {
            try
            {
                if (file != null)
                {
                    this.file = file;
                    textBox.TextChanged -= textBox_TextChanged;
                    textBox.Document.SetText(Windows.UI.Text.TextSetOptions.None,await ReadFile(file));
                    FileName = file.DisplayName;
                    changed = false;
                    textBox.TextChanged += textBox_TextChanged;
                }
            }
            catch (Exception ex)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Error : " + ex.Message, "Notepad T");
                await dialog.ShowAsync();
            }
        }

        public async System.Threading.Tasks.Task OpenFile()
        {
            try
            {
                Windows.Storage.Pickers.FileOpenPicker picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
                picker.FileTypeFilter.Add(".txt");
                StorageFile file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    this.file = file;
                    textBox.TextChanged -= textBox_TextChanged;
                    string txt = await ReadFile(file);
                    textBox.Document.SetText(Windows.UI.Text.TextSetOptions.None, txt);
                    FileName = file.DisplayName;
                    changed = false;
                    textBox.TextChanged += textBox_TextChanged;
                }
            }
            catch (Exception ex)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Error : " + ex.Message, "Notepad T");
                await dialog.ShowAsync();
            }
        }

        public async System.Threading.Tasks.Task SaveFile()
        {
            try
            {
                if (file != null)
                {
                    string text;
                    textBox.Document.GetText(Windows.UI.Text.TextGetOptions.None, out text);
                    WriteFile(file, text);
                    FileName = file.DisplayName;
                    changed = false;
                }
                else await SaveAsFile();
            }
            catch (Exception ex)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Error : " + ex.Message, "Notepad T");
                await dialog.ShowAsync();
            }
        }

        public async System.Threading.Tasks.Task SaveAsFile()
        {
            try
            {
                Windows.Storage.Pickers.FileSavePicker picker = new Windows.Storage.Pickers.FileSavePicker();
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                picker.DefaultFileExtension = ".txt";
                picker.FileTypeChoices.Add("Text", new string[] { ".txt" });
                StorageFile file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    string text;
                    textBox.Document.GetText(Windows.UI.Text.TextGetOptions.None, out text);
                    WriteFile(file, text);
                    this.file = file;
                    FileName = file.DisplayName;
                    changed = false;
                }
            }
            catch (Exception ex)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Error : " + ex.Message, "Notepad T");
                await dialog.ShowAsync();
            }
        }

        public async System.Threading.Tasks.Task Close()
        {
            try
            {
                if (changed)
                {
                    Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Do you want to save changes to " + FileName + "?", "Close File");
                    dialog.Commands.Add(new Windows.UI.Popups.UICommand { Label = "Yes", Id = 0 });
                    dialog.Commands.Add(new Windows.UI.Popups.UICommand { Label = "No", Id = 0 });
                    dialog.Commands.Add(new Windows.UI.Popups.UICommand { Label = "Cancel", Id = 0 });
                    Windows.UI.Popups.IUICommand result = await dialog.ShowAsync();
                    if (result.Label == "Yes")
                    {
                        await SaveFile();
                        canClose = true;
                    }
                    else if (result.Label=="No")
                    {
                        canClose = true;
                    }
                    else
                    {
                        canClose = false;
                    }
                }
                else
                {
                    canClose = true;
                }
            }
            catch (Exception ex)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Error : " + ex.Message, "Notepad T");
                await dialog.ShowAsync();
            }
        }


        private async System.Threading.Tasks.Task<string> ReadFile(StorageFile file)
        {
            string text = "";
            byte[] data;

            Windows.Storage.Streams.IRandomAccessStream str = await file.OpenAsync(FileAccessMode.Read);
            Windows.Storage.Streams.IInputStream istr = str.GetInputStreamAt(0);
            Windows.Storage.Streams.DataReader reader = new Windows.Storage.Streams.DataReader(istr);
            data = new byte[(uint)str.Size];
            await reader.LoadAsync((uint)str.Size);
            try
            {
                reader.ReadBytes(data);
                switch (MainPage.Instance.encoding)
                {
                    case 0: text = System.Text.Encoding.ASCII.GetString(data); break;
                    case 1: text = System.Text.Encoding.Unicode.GetString(data); break;
                    case 2: text = System.Text.Encoding.BigEndianUnicode.GetString(data); break;
                    case 3: text = System.Text.Encoding.UTF8.GetString(data); break;
                    case 4: text = System.Text.Encoding.GetEncoding(MainPage.Instance.codePage).GetString(data); break;
                }
                //System.Text.Encoding.GetEncoding()
            }
            catch
            {
            }
            reader.DetachStream();
            return text;
        }


        public void Undo()
        {
            textBox.Document.Undo();
            textBox.Focus(FocusState.Programmatic);
        }


        public void Redo()
        {
            textBox.Document.Redo();
            textBox.Focus(FocusState.Programmatic);
        }

        public void SelectAll()
        {
            string text = "";
            textBox.Document.GetText(Windows.UI.Text.TextGetOptions.None, out text);
            textBox.Document.Selection.SetRange(0, text.Length);
            textBox.Focus(FocusState.Programmatic);
        }

        public void Copy()
        {
            textBox.Document.Selection.Copy();
            textBox.Focus(FocusState.Programmatic);
        }

        public void Cut()
        {
            textBox.Document.Selection.Cut();
            textBox.Focus(FocusState.Programmatic);
        }

        public void Paste()
        {
            textBox.Document.Selection.Paste(0);
            textBox.Focus(FocusState.Programmatic);
        }


        public void InsertDate()
        {
            textBox.Document.Selection.SetText(Windows.UI.Text.TextSetOptions.None, DateTime.Now.ToString());
            textBox.Document.Selection.StartPosition = textBox.Document.Selection.EndPosition;
            textBox.Focus(FocusState.Programmatic);
        }

        public void ReloadSettings()
        {
            
        }

        private async void WriteFile(StorageFile file, string text)
        {
            try
            {
                Windows.Storage.Streams.IRandomAccessStream str = await file.OpenAsync(FileAccessMode.ReadWrite);
                Windows.Storage.Streams.IOutputStream ostr = str.GetOutputStreamAt(0);
                Windows.Storage.Streams.DataWriter writer = new Windows.Storage.Streams.DataWriter(ostr);
                //await reader.LoadAsync((uint)str.Size);
                writer.WriteString(text);
                //text = reader.ReadString((uint)str.Size);
                await writer.StoreAsync();
                writer.DetachStream();
                //reader.DetachStream();
                await ostr.FlushAsync();
            }
            catch (Exception ex)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Error : " + ex.Message, "Notepad T");
                await dialog.ShowAsync();
            }
        }

        private void textBox_TextChanged(object sender, RoutedEventArgs e)
        {
            changed = true;
        }

        bool controlDown = false;

        private void textBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.F5: InsertDate(); e.Handled = true; break;
                case Windows.System.VirtualKey.Control: controlDown = true; break;
            }
            if (controlDown) e.Handled = true;
        }

        private void textBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Control) controlDown = false;
            if (controlDown)
            {
                switch (e.Key)
                {
                    case Windows.System.VirtualKey.S: SaveFile(); break;
                    case Windows.System.VirtualKey.O: OpenFile(); break;
                }
            }
            if (controlDown) e.Handled = true;
        }
    }
}
