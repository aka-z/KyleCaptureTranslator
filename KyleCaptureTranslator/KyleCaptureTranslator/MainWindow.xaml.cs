using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp;
using CefSharp.Wpf;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace KyleCaptureTranslator
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private Boolean initialized = false;

    public List<Windows.Globalization.Language> OCRLanguages { get; set; } = new List<Windows.Globalization.Language>();

    public Dictionary<String, String> GoogleLanguageTags { get; set; } = new Dictionary<String, String>();

    public MainWindow()
    {
      DataContext = this;

      OCRLanguages = OcrEngine.AvailableRecognizerLanguages.ToList();

      // Google Defined Languages
      GoogleLanguageTags.Add("English", "en");
      GoogleLanguageTags.Add("Korean", "ko");
      GoogleLanguageTags.Add("Japanese", "ja");
      GoogleLanguageTags.Add("Chinese (Simplified)", "zh-CN");
      GoogleLanguageTags.Add("Chinese (Traditional)", "zh-TW");

      GoogleLanguageTags.Add("French", "fr");
      GoogleLanguageTags.Add("Spanish", "es");
      GoogleLanguageTags.Add("German", "de");
      GoogleLanguageTags.Add("Arabic", "ar");
      GoogleLanguageTags.Add("Thai", "th");

      InitializeComponent();
    }

    private async void OnCaptureButton_Click(Object sender, RoutedEventArgs e)
    {
      Visibility = Visibility.Hidden;
      try
      {
        CaptureWindow captureWindow = new CaptureWindow();
        Boolean result = captureWindow.ShowDialog().Value;
        
        if (result)
        { // The region is captured. Run OCR and Translate
          MemoryStream ms;
          var bitmapSource = CopyScreen(captureWindow.Rect, out ms);
       
          captureImage.Source = bitmapSource;
          Visibility = Visibility.Visible;
          initialized = true;

          InMemoryRandomAccessStream inMemoryStream = new InMemoryRandomAccessStream();
          ms.Position = 0;
          Byte[] byteArray = ms.GetBuffer();
          inMemoryStream.AsStream().Write(byteArray, 0, byteArray.Length);
          inMemoryStream.AsStream().Flush();

          Windows.Graphics.Imaging.BitmapDecoder bitmapDecoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(inMemoryStream);
          SoftwareBitmap softwareBitmap = await bitmapDecoder.GetSoftwareBitmapAsync().AsTask().ConfigureAwait(true);
          captureImage.Tag = softwareBitmap; // for later use

          await RunOcrAndTranslate();
        }
      }
      catch { }
      Visibility = Visibility.Visible;
    }

    private async Task RunOcrAndTranslate()
    {
      SoftwareBitmap softwareBitmap = captureImage.Tag as SoftwareBitmap;
      OcrEngine ocrEngine = OcrEngine.TryCreateFromLanguage(sourceLanguageComboBox.SelectedItem as Windows.Globalization.Language);
      var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);
      String resultString = ocrResult.Text;

      if (ocrEngine.RecognizerLanguage.LanguageTag == "ja")
        resultString = resultString.Replace(" ", "");

      var sourceLanguage = sourceLanguageComboBox.SelectedItem as Windows.Globalization.Language;
      String sourceLanguageTag = sourceLanguage.LanguageTag;
      if (sourceLanguageTag.StartsWith("en"))
        sourceLanguageTag = "en";
      String destinationLanguageTag = GoogleLanguageTags[destinationLanguageComboBox.SelectedItem as String];
      String textURL = HttpUtility.UrlEncode(resultString);
      String navigateURL = $"https://translate.google.com/m/translate#{sourceLanguageTag}/{destinationLanguageTag}/{textURL}";

      WebBrowserExtensions.LoadHtml(browser, "");
      browser.Address = navigateURL;
    }

    private BitmapSource CopyScreen(Int32Rect rect, out MemoryStream ms)
    { 
      using (var screenBmp = new Bitmap(
          rect.Width,
          rect.Height,
          System.Drawing.Imaging.PixelFormat.Format24bppRgb))
      {
        using (var bmpGraphics = Graphics.FromImage(screenBmp))
        {
          bmpGraphics.CopyFromScreen(rect.X, rect.Y, 0, 0, screenBmp.Size);
          ms = new MemoryStream();
          screenBmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
          var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(screenBmp.GetHbitmap(),
                                                                    IntPtr.Zero,
                                                                    Int32Rect.Empty,
                                                                    BitmapSizeOptions.FromEmptyOptions());
          return bitmapSource;
        }
      }
    }

    private async void OnSourceLanguageComboBox_SelectionChanged(Object sender, SelectionChangedEventArgs e)
    {
      if (initialized)
      {
        await RunOcrAndTranslate();
      }
    }

    private async void OnDestinationLanguageComboBox_SelectionChanged(Object sender, SelectionChangedEventArgs e)
    {
      if (initialized)
        await RunOcrAndTranslate();
    }
  }
}
