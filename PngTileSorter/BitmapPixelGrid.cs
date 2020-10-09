using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PngTileSorter
{
  public class BitmapPixelGrid
  {
    public int m_Width { get; private set; }
    public int m_Height { get; private set; }
    public byte[] m_Pixels { get; private set; }
    public int m_Stride { get; private set; }

    public BitmapPixelGrid(int width, int height)
    {
      m_Width = width;
      m_Height = height;
      m_Pixels = new byte[width * height * 4];
      m_Stride = width * 4;
    }

    public BitmapPixelGrid(BitmapSource source)
    {
      m_Width = source.PixelWidth;
      m_Height = source.PixelHeight;
      m_Pixels = new byte[m_Width * m_Height * 4];
      m_Stride = m_Width * 4;

      source.CopyPixels(m_Pixels, m_Stride, 0);
    }

    public Color this[int i]
    {
      get
      {
        i *= 4;
        var b = m_Pixels[i + 0];
        var g = m_Pixels[i + 1];
        var r = m_Pixels[i + 2];
        var a = m_Pixels[i + 3];

        return new Color() { R = r, G = g, B = b, A = a };
      }

      set
      {
        i *= 4;
        m_Pixels[i + 0] = value.B;
        m_Pixels[i + 1] = value.G;
        m_Pixels[i + 2] = value.R;
        m_Pixels[i + 3] = value.A;
      }
    }

    public Color this[int x, int y]
    {
      get
      {
        var index = y * m_Stride + x * 4;
        var b = m_Pixels[index + 0];
        var g = m_Pixels[index + 1];
        var r = m_Pixels[index + 2];
        var a = m_Pixels[index + 3];

        return new Color() { R = r, G = g, B = b, A = a };
      }

      set
      {
        var index = y * m_Stride + x * 4;
        m_Pixels[index + 0] = value.B;
        m_Pixels[index + 1] = value.G;
        m_Pixels[index + 2] = value.R;
        m_Pixels[index + 3] = value.A;
      }
    }
  }
}
