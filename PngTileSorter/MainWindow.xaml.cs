using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

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

    private (int, int)[] m_Weights;
    private BitmapPixelGrid m_OutputPixels;

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
        var inputStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var decoder = new PngBitmapDecoder(inputStream, BitmapCreateOptions.PreservePixelFormat,
          BitmapCacheOption.Default);
        var inputSource = decoder.Frames[0];
        var pxWidth = inputSource.PixelWidth;
        var pxHeight = inputSource.PixelHeight;
        m_InputBackground.Width = inputSource.Width;
        m_InputBackground.Height = inputSource.Height;
        m_InputImage.Width = inputSource.Width;
        m_InputImage.Height = inputSource.Height;
        m_InputImage.Source = inputSource;

        var inputPixels = new BitmapPixelGrid(inputSource);

        var cols = pxWidth / m_TileWidth;
        var rows = pxHeight / m_TileHeight;
        var tileCount = cols * rows;
        m_Weights = new (int, int)[tileCount];

        for (var row = 0; row < rows; ++row)
        {
          for (var col = 0; col < cols; ++col)
          {
            var weightIndex = row * cols + col;
            m_Weights[weightIndex].Item1 = weightIndex;

            for (var j = 0; j < m_TileHeight; ++j)
            {
              for (var i = 0; i < m_TileWidth; ++i)
              {
                var x = i + col * m_TileWidth;
                var y = j + row * m_TileHeight;
                var color = inputPixels[x, y];
                m_Weights[weightIndex].Item2 += color.A;
              }
            }

            m_Weights[weightIndex].Item2 /= tileCount;
          }
        }

        // Now sort the weights array, repeatedly finding
        // the smallest item and removing it, while also:
        //   - copying that tile to a writeable bitmap
        //   - recording the index and weight for logging

        // Then display the new bitmap in the other image
        // as a preview and enable the button that allows
        // the sorted image to be written to file

        Array.Sort(m_Weights, (a, b) => a.Item2.CompareTo(b.Item2));

        m_OutputPixels = new BitmapPixelGrid(pxWidth, pxHeight);

        for (var outputIndex = 0; outputIndex < m_Weights.Length; ++outputIndex)
        {
          var outputRow = outputIndex / cols;
          var outputCol = outputIndex % cols;

          var item = m_Weights[outputIndex];
          var inputIndex = item.Item1;
          var inputRow = inputIndex / cols;
          var inputCol = inputIndex % cols;

          for (var j = 0; j < m_TileHeight; ++j)
          {
            for (var i = 0; i < m_TileWidth; ++i)
            {
              var inputX = inputCol * m_TileWidth + i;
              var inputY = inputRow * m_TileHeight + j;
              var outputX = outputCol * m_TileWidth + i;
              var outputY = outputRow * m_TileHeight + j;
              m_OutputPixels[outputX, outputY] = inputPixels[inputX, inputY];
            }
          }
        }

        var outputSource = BitmapSource.Create(pxWidth, pxHeight, inputSource.DpiX, inputSource.DpiY,
          inputSource.Format, inputSource.Palette, m_OutputPixels.m_Pixels, m_OutputPixels.m_Stride);

        m_OutputBackground.Width = inputSource.Width;
        m_OutputBackground.Height = inputSource.Height;
        m_OutputImage.Width = inputSource.Width;
        m_OutputImage.Height = inputSource.Height;
        m_OutputImage.Source = outputSource;

        var outputDirectory = Path.GetDirectoryName(path);
        var inputFileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        var outputFileNameWithoutExtension = inputFileNameWithoutExtension + " (sorted)";
        var extension = Path.GetExtension(path);
        var outputFileName = outputFileNameWithoutExtension + extension;
        var outputPath = Path.Combine(outputDirectory, outputFileName);

        using (var outputStream = new FileStream(outputPath, FileMode.Create))
        {
          var encoder = new PngBitmapEncoder();
          encoder.Interlace = PngInterlaceOption.Off;
          encoder.Frames.Add(BitmapFrame.Create(outputSource));
          encoder.Save(outputStream);
        }
      }
    }


    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
