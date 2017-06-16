using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KyleCaptureTranslator
{
  /// <summary>
  /// Interaction logic for CaptureWindow.xaml
  /// </summary>
  public partial class CaptureWindow : Window
  {
    public Int32Rect Rect { get; private set; }
    private Int32 downPointX;
    private Int32 downPointY;

    public CaptureWindow()
    {
      InitializeComponent();
    }

    private void OnWindow_KeyDown(Object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape)
      {
        DialogResult = false;
        Close();
      }
    }

    private void ManipulateCaptureRectangle(Int32 pointX, Int32 pointY)
    {
      if (captureRect.Visibility == Visibility.Hidden)
      { // Capture begins
        downPointX = pointX;
        downPointY = pointY;
        captureRect.Margin = new Thickness(pointX, pointY, 0, 0);
        captureRect.Width = 5;
        captureRect.Height = 5;
        Rect = new Int32Rect(pointX, pointY, 5, 5);
        captureRect.Visibility = Visibility.Visible;
      }
      else
      { // Mouse Move
        Int32 left = Math.Min(pointX, downPointX);
        Int32 top = Math.Min(pointY, downPointY);
        Int32 right = Math.Max(pointX, downPointX);
        Int32 bottom = Math.Max(pointY, downPointY);
        captureRect.Margin = new Thickness(left, top, 0, 0);
        captureRect.Width = right - left;
        captureRect.Height = bottom - top;
        Rect = new Int32Rect(left, top, right - left, bottom - top);
      }      
    }

    private void OnWindow_MouseDown(Object sender, MouseButtonEventArgs e)
    { // Make a small rectangle to give a user que that the mouse down is registered.
      var mouseDownPosition = e.GetPosition(this);
      ManipulateCaptureRectangle((Int32)mouseDownPosition.X, (Int32)mouseDownPosition.Y);
    }

    private void OnWindow_MouseMove(Object sender, MouseEventArgs e)
    {
      if (captureRect.Visibility == Visibility.Visible)
      { // Only if the capture has begun
        var mouseDownPosition = e.GetPosition(this);
        ManipulateCaptureRectangle((Int32)mouseDownPosition.X, (Int32)mouseDownPosition.Y);
      }
    }

    private void OnWindow_MouseUp(Object sender, MouseButtonEventArgs e)
    {
      if (captureRect.Visibility == Visibility.Visible)
      {
        var mouseDownPosition = e.GetPosition(this);
        ManipulateCaptureRectangle((Int32)mouseDownPosition.X, (Int32)mouseDownPosition.Y);
        DialogResult = true;
        Close();
      }
    }
  }
}
