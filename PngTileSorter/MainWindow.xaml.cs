using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PngTileSorter
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private int m_TileWidth_ = 8;
    public int m_TileWidth
    {
      get { return m_TileWidth_; }
      set
      {
        var old = m_TileWidth_;
        m_TileWidth_ = value;

        if (value != old)
          OnPropertyChanged("m_TileWidth");
      }
    }

    private int m_TileHeight_ = 8;
    public int m_TileHeight
    {
      get { return m_TileHeight_; }
      set
      {
        var old = m_TileHeight_;
        m_TileHeight_ = value;

        if (value != old)
          OnPropertyChanged("m_TileHeight");
      }
    }

    public MainWindow()
    {
      InitializeComponent();
    }

    private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new OpenFileDialog();
      dialog.Filter = "PNG Images|*.png";
      dialog.DefaultExt = ".png";
      dialog.InitialDirectory = Environment.CurrentDirectory;

      if (dialog.ShowDialog() == true)
      {
        var path = dialog.FileName;
        m_FilePath.Text = path;
        var fileUri = new Uri(path);
        var bitmap = new BitmapImage(fileUri);
        m_SourceBackground.Width = bitmap.Width;
        m_SourceBackground.Height = bitmap.Height;
        m_SourceImage.Width = bitmap.Width;
        m_SourceImage.Height = bitmap.Height;
        m_SourceImage.Source = bitmap;
        m_OutputBackground.Width = bitmap.Width;
        m_OutputBackground.Height = bitmap.Height;
        m_OutputImage.Width = bitmap.Width;
        m_OutputImage.Height = bitmap.Height;

        var pixels = new BitmapPixelGrid(bitmap);

        var cols = bitmap.PixelWidth / m_TileWidth;
        var rows = bitmap.PixelHeight / m_TileHeight;
        var tileCount = cols * rows;
        var weights = new float[tileCount];

        for (var row = 0; row < rows; ++row)
        {
          for (var col = 0; col < cols; ++col)
          {
            var weightIndex = row * cols + col;

            for (var j = 0; j < m_TileHeight; ++j)
            {
              for (var i = 0; i < m_TileWidth; ++i)
              {
                var x = i + col * m_TileWidth;
                var y = j + row * m_TileHeight;
                var color = pixels[x, y];
                weights[weightIndex] += color.A / 255.0f;
              }
            }

            weights[weightIndex] /= tileCount;
          }
        }

        // Now sort the weights array, repeatedly finding
        // the smallest item and removing it, while also:
        //   - copying that tile to a writeable bitmap
        //   - recording the index and weight for logging

        // Then display the new bitmap in the other image
        // as a preview and enable the button that allows
        // the sorted image to be written to file
      }
    }


    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
