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
        bool setupDone = false;
        public bool status = false;
        public int encoding = 0;
        public int codePage = 0;
        public string lineBreak = "\\r\\n";
        public string font;
        public int fontSize = 15;
        public bool fontBold = false;
        public bool fontItalic = false;

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
                localSettings.Values["encoding"] = 4;
            }
            encodingBox.SelectedIndex = encoding;

            if (encoding == 4)
            {
                codePageCanvas.Visibility = Visibility.Visible;
            }

            if (localSettings.Values["page"] != null)
            {
                codePage = (int)localSettings.Values["page"];
            }
            else
            {
                codePage = 1250;
            }
            codePageSetBox.Text = codePage.ToString();

            string[] fonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
            int segoe = 0;
            for (int i = 0; i < fonts.Length; i++)
            {
                string font = fonts[i];
                if (font == "Segoe UI")
                {
                    segoe = i;
                }
                try
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.FontFamily = new FontFamily(font);
                    item.Content = font;
                    fontBox.Items.Add(item);
                }
                catch (Exception ex)
                {
                    Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Error : " + ex.Message, "Notepad T");
                    dialog.ShowAsync();
                }
            }


            if (localSettings.Values["font"] != null)
            {
                font = (string)localSettings.Values["font"];
                for (int i = 0; i < fontBox.Items.Count; i++)
                {
                    if ((string)((ComboBoxItem)fontBox.Items[i]).Content == font)
                    {
                        fontBox.SelectedIndex = i;
                    }
                }
            }
            else
            {
                font = "Segoe UI";
                fontBox.SelectedItem = segoe;
            }

            if (localSettings.Values["fontSize"] != null)
            {
                fontSize = (int)localSettings.Values["fontSize"];
            }
            else
            {
                localSettings.Values["fontSize"] = 15;
            }
            sizeBox.Text = fontSize.ToString();

            if (localSettings.Values["fontBold"] != null)
            {
                fontBold = (bool)localSettings.Values["fontBold"];
            }
            else
            {
                localSettings.Values["fontBold"] = false;
            }
            boldBox.IsOn = fontBold;

            if (localSettings.Values["fontItalic"] != null)
            {
                fontItalic = (bool)localSettings.Values["fontItalic"];
            }
            else
            {
                localSettings.Values["fontItalic"] = false;
            }
            italicBox.IsOn = fontItalic;

            if (localSettings.Values["lineBreak"] != null)
            {
                lineBreak = (string)localSettings.Values["lineBreak"];
            }
            else
            {
                lineBreak = "\\r\\n";
            }
            lineBox.Text = lineBreak;

            setupDone = true;
            SetAllPads();
            
        }

        public void SetAllPads()
        {
            if (setupDone)
            {
                foreach (PivotItem i in pivot.Items)
                {
                    Pad p = i.Content as Pad;
                    if (status) p?.ShowStatus();
                    else p?.HideStatus();

                    p.SetFont();
                }
            }
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
            SetAllPads();
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

        private void fontBox_DropDownClosed(object sender, object e)
        {
            font = (string)((ComboBoxItem)fontBox.SelectedItem).Content;
            localSettings.Values["font"] = font;
            SetAllPads();
        }

        private void sizeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int i = sizeBox.SelectionStart;
            string text = sizeBox.Text;
            string rtext = "";
            foreach(char c in text)
            {
                if (c >= '0' && c <= '9') rtext += c;
                else i--;
            }
            sizeBox.Text = rtext;
            sizeBox.SelectionStart = i;
        }

        private void sizeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            fontSize = int.Parse(sizeBox.Text);
            localSettings.Values["fontSize"] = fontSize;
            SetAllPads();
        }

        private void boldBox_Checked(object sender, RoutedEventArgs e)
        {
            fontBold = (bool)boldBox.IsOn;
            localSettings.Values["fontBold"] = fontBold;
            SetAllPads();
        }

        private void italicBox_Checked(object sender, RoutedEventArgs e)
        {
            fontItalic = (bool)italicBox.IsOn;
            localSettings.Values["fontItalic"] = fontItalic;
            SetAllPads();
        }

        private void Button_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {

        }

        private void sizeBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                fontSize = int.Parse(sizeBox.Text);
                localSettings.Values["fontSize"] = fontSize;
                SetAllPads();
            }
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                searchNext_Click(null, null);
            }
        }

        private void TextBox_KeyDown_1(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Replace_Click(null, null);
            }
        }

        private void searchNext_Click(object sender, RoutedEventArgs e)
        {
            ((Pad)(((PivotItem)pivot.SelectedItem).Content)).SearchNext(searchBox.Text);
        }

        private void searchPrev_Click(object sender, RoutedEventArgs e)
        {
            ((Pad)(((PivotItem)pivot.SelectedItem).Content)).SearchPrev(searchBox.Text);
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            ((Pad)(((PivotItem)pivot.SelectedItem).Content)).Replace(searchBox.Text, replaceBox.Text);
        }

        private void ReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            ((Pad)(((PivotItem)pivot.SelectedItem).Content)).ReplaceAll(searchBox.Text, replaceBox.Text);
        }

        private void lineBox_LostFocus(object sender, RoutedEventArgs e)
        {
            lineBreak = lineBox.Text;
            localSettings.Values["lineBreak"] = lineBreak;
        }
    }
}