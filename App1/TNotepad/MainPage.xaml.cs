using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TNotepad
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Instance;
        public Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        public bool status = false;
        public int encoding = 0;
        public int codePage = 0;

        public MainPage()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Instance = this;
            this.InitializeComponent();
            if (localSettings.Values["status"] != null)
            {
                status = (bool)localSettings.Values["status"];
            }
            else
            {
                localSettings.Values["status"] = false;
            }
            toggleSwitch.IsOn = status;

            if (localSettings.Values["encoding"] != null)
            {
                encoding = (int)localSettings.Values["encoding"];
            }
            else
            {
                localSettings.Values["encoding"] = 0;
            }
            encodingBox.SelectedIndex = encoding;

            if (localSettings.Values["page"] != null)
            {
                codePage = 0;
            }
            else
            {
                codePage = 0;
            }
            localSettings.Values["page"] = codePage;
        }






        public async void OnFileActivated(Windows.ApplicationModel.Activation.FileActivatedEventArgs e)
        {
            bool one = true;
            if (((Pad)(((PivotItem)pivot.SelectedItem).Content)).file != null)
            {
                one = false;
            }
            foreach (Windows.Storage.StorageFile file in e.Files)
            {
                if (one)
                {
                    await ((Pad)(((PivotItem)pivot.SelectedItem).Content)).OpenFileAct(file);
                    ((PivotItem)pivot.SelectedItem).Header = ((Pad)(((PivotItem)pivot.SelectedItem).Content)).FileName;
                    one = false;
                }
                else
                {
                    Pad p = new Pad();
                    await p.OpenFileAct(file);
                    PivotItem i = new PivotItem();
                    i.Content = p;

                    TextBlock block = new TextBlock();
                    block.Text = "Unnamed File";
                    block.FontSize = 18;

                    i.Header = block;

                    pivot.Items.Add(i);
                    pivot.SelectedItem = i;
                }
            }
        }

        private void Hamburger_Click(object sender, RoutedEventArgs e)
        {
            pane.IsPaneOpen = !pane.IsPaneOpen;
        }

        private async void OpenBut_Click(object sender, RoutedEventArgs e)
        {
            if ((((Pad)(((PivotItem)pivot.SelectedItem).Content)).file != null) && (((Pad)(((PivotItem)pivot.SelectedItem).Content)).changed))
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Do you want to save changes to " + ((Pad)(((PivotItem)pivot.SelectedItem).Content)).FileName  + "?", "Open File");
                dialog.Commands.Add(new Windows.UI.Popups.UICommand { Label = "Yes", Id = 0});
                dialog.Commands.Add(new Windows.UI.Popups.UICommand { Label = "No", Id = 1 });
                dialog.Commands.Add(new Windows.UI.Popups.UICommand { Label = "Open in New Tab", Id = 2 });
                Windows.UI.Popups.IUICommand result = await dialog.ShowAsync();
                if (result.Label == "No")
                {
                    await ((Pad)(((PivotItem)pivot.SelectedItem).Content)).SaveFile();
                    await ((Pad)(((PivotItem)pivot.SelectedItem).Content)).OpenFile();
                    ((PivotItem)pivot.SelectedItem).Header = ((Pad)(((PivotItem)pivot.SelectedItem).Content)).FileName;
                    pane.IsPaneOpen = false;
                }
                else if (result.Label == "Yes")
                {
                    await ((Pad)(((PivotItem)pivot.SelectedItem).Content)).OpenFile();
                    ((PivotItem)pivot.SelectedItem).Header = ((Pad)(((PivotItem)pivot.SelectedItem).Content)).FileName;
                    pane.IsPaneOpen = false;
                }
                else if (result.Label == "Open in New Tab")
                {
                    Pad p = new Pad();
                    await p.OpenFile();
                    PivotItem i = new PivotItem();
                    i.Content = p;
                    i.Header = p.FileName;
                    pivot.Items.Add(i);
                    pivot.SelectedItem = i;
                    pane.IsPaneOpen = false;
                }
            }
            else
            {
                await ((Pad)(((PivotItem)pivot.SelectedItem).Content)).OpenFile();
                ((PivotItem)pivot.SelectedItem).Header = ((Pad)(((PivotItem)pivot.SelectedItem).Content)).FileName;
                pane.IsPaneOpen = false;
            }
        }

        private async void SaveBut_Click(object sender, RoutedEventArgs e)
        {
            await ((Pad)(((PivotItem)pivot.SelectedItem).Content)).SaveFile();
            ((PivotItem)pivot.SelectedItem).Header = ((Pad)(((PivotItem)pivot.SelectedItem).Content)).FileName;
            pane.IsPaneOpen = false;
        }

        private async void SaveAsBut_Click(object sender, RoutedEventArgs e)
        {
            await ((Pad)(((PivotItem)pivot.SelectedItem).Content)).SaveAsFile();
            ((PivotItem)pivot.SelectedItem).Header = ((Pad)(((PivotItem)pivot.SelectedItem).Content)).FileName;
            pane.IsPaneOpen = false;
        }

        private async void CloseBut_Click(object sender, RoutedEventArgs e)
        {
            await ((Pad)(((PivotItem)pivot.SelectedItem).Content)).Close();
            if (((Pad)(((PivotItem)pivot.SelectedItem).Content)).canClose)
            {
                pivot.Items.Remove(pivot.SelectedItem);
                if (pivot.Items.Count == 0)
                {
                    NewBut_Click(sender, e);
                }
                pane.IsPaneOpen = false;
            }
        }

        private void NewBut_Click(object sender, RoutedEventArgs e)
        {
            Pad p = new Pad();
            PivotItem i = new PivotItem();
            i.Content = p;
            i.Header = p.FileName;
            pivot.Items.Add(i);
            pivot.SelectedItem = i;
            pane.IsPaneOpen = false;
        }

        private void UndoBut_Click(object sender, RoutedEventArgs e)
        {
            ((Pad)(((PivotItem)pivot.SelectedItem).Content)).Undo();
            pane.IsPaneOpen = false;
        }

        private void RedoBut_Click(object sender, RoutedEventArgs e)
        {
            ((Pad)(((PivotItem)pivot.SelectedItem).Content)).Redo();
            pane.IsPaneOpen = false;
        }

        private void SelAllBut_Click(object sender, RoutedEventArgs e)
        {
            ((Pad)(((PivotItem)pivot.SelectedItem).Content)).SelectAll();
            pane.IsPaneOpen = false;
        }

        private void CopyBut_Click(object sender, RoutedEventArgs e)
        {
            ((Pad)(((PivotItem)pivot.SelectedItem).Content)).Copy();
            pane.IsPaneOpen = false;
        }

        private void CutBut_Click(object sender, RoutedEventArgs e)
        {
            ((Pad)(((PivotItem)pivot.SelectedItem).Content)).Cut();
            pane.IsPaneOpen = false;
        }

        private void PasteBut_Click(object sender, RoutedEventArgs e)
        {
            ((Pad)(((PivotItem)pivot.SelectedItem).Content)).Paste();
            pane.IsPaneOpen = false;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.argsAfterStart != null)
            {
                OnFileActivated(App.argsAfterStart);
            }
        }

        private void SettingsBut_Click(object sender, RoutedEventArgs e)
        {
            settingsPane.IsPaneOpen = !settingsPane.IsPaneOpen;
            pane.IsPaneOpen = false;
        }

        private void CalendarBut_Click(object sender, RoutedEventArgs e)
        {
            ((Pad)(((PivotItem)pivot.SelectedItem).Content)).InsertDate();
            pane.IsPaneOpen = false;
        }

        private void toggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            status = toggleSwitch.IsOn;
            localSettings.Values["status"] = status;
        }

        private void encodingBox_DropDownClosed(object sender, object e)
        {
            if (encodingBox.SelectedIndex != 4)
            {
                encoding = encodingBox.SelectedIndex;
                localSettings.Values["encoding"] = encoding;
                codePageCanvas.Visibility = Visibility.Collapsed;
            }
            else
            {
                codePageCanvas.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(codePageSetBox.Text, out int page))
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Invalid Codepage Number:\nNot a number", "Error");
                dialog.ShowAsync();
            }
            else
            {
                try
                {
                    System.Text.Encoding.GetEncoding(page);
                    codePage = page;
                    localSettings.Values["page"] = codePage;
                    encoding = 4;
                    localSettings.Values["encoding"] = encoding;
                }
                catch (Exception ex)
                {
                    encodingBox.SelectedIndex = encoding;
                    codePageCanvas.Visibility = Visibility.Collapsed;
                    Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Invalid Codepage Number:\n"+ ex.Message, "Error");
                    dialog.ShowAsync();
                }
            }
        }
    }
}