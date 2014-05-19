using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageInpainting
{
  public static class Helper
  {
    public static double[,] Add(double[,] source, double[,] value)
    {
      var maxX = source.GetLength(0);
      var maxY = source.GetLength(1);
      var result = new double[maxX, maxY];
      for (int x = 0; x < maxX; x++)
      {
        for (int y = 0; y < maxY; y++)
        {
          result[x, y] = source[x, y] + value[x, y];
        }
      }
      return result;
    }

    public static U[,] Select2D<T, U>(this T[,] array, Func<T, U> f)
    {
      var result = new U[array.GetLength(0), array.GetLength(1)];
      Parallel.For(0, array.GetLength(0), new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, (x, state) =>
      {

        for (int y = 0; y < array.GetLength(1); y++)
        {
          result[x, y] = f(array[x, y]);
        }
      });

      return result;
    }

    public static U[,] Select2D<T, U>(this T[,] array, Func<T, int, int, U> f)
    {
      var result = new U[array.GetLength(0), array.GetLength(1)];

      for (int row = 0; row < array.GetLength(0); row++)
      {
        for (int column = 0; column < array.GetLength(1); column++)
        {
          result[row, column] = f(array[row, column], row, column);
        }
      }

      return result;
    }

    public static double[,] LoadImage(string imageName)
    {
      string fullPath = Path.GetFullPath(imageName);
      Bitmap bmp = new Bitmap(fullPath);
      double[,] imgBytes = new double[bmp.Height, bmp.Width];
      for (int x = 0; x < bmp.Width; x++)
      {
        for (int y = 0; y < bmp.Height; y++)
        {
          imgBytes[y, x] = bmp.GetPixel(x, y).R;
        }
      }
      return imgBytes;
    }

    // Load black template
    public static bool[,] LoadTemplate(string templateName)
    {
      string fullPath = Path.GetFullPath(templateName);
      Bitmap bmp = new Bitmap(fullPath);

      bool[,] template = new bool[bmp.Height, bmp.Width];
      for (int x = 0; x < bmp.Width; x++)
      {
        for (int y = 0; y < bmp.Height; y++)
        {
          template[y, x] = (bmp.GetPixel(x, y).R == 0) ;
        }
      }
      return template;
    }
    
    public static void SaveTemplate(bool[,] template, string templateName)
    {
     
      int x = template.GetLength(1);
      int y = template.GetLength(0);
     
      var bmp = new Bitmap(x, y);
      template.Select2D((value, row, column) =>
      {
        if (template[row, column])
        {
          bmp.SetPixel(column, row, Color.FromArgb(0, 0, 0));
        }
        else
        {
          bmp.SetPixel(column, row, Color.FromArgb(255, 255, 255));
        }
        return value;
      });
      
      bmp.Save(templateName);
    }

    

    public static double[,] Normalisation(double[,] img)
    {
      double maxVal = double.MinValue;
      double minVal = double.MaxValue;
      img.Select2D(x =>
      {
        if (x > maxVal)
          maxVal = x;
        if (x<minVal)
        {
          minVal = x;
        }
        return 0;
      });
      minVal = 50000;
      maxVal = 25318199.389405839;
      double factor = 255/(maxVal - minVal);

      return img.Select2D(x =>((x - minVal) / (maxVal - minVal) * 255));

    }

    public static double Max2d(double[,] arr)
    {
      double max = double.NegativeInfinity;
      for (int x = 0; x < arr.GetLength(0); x++)
      {
        for (int y = 0; y < arr.GetLength(1); y++)
        {
          if (arr[x, y] > max) max = arr[x, y];
        }
      }
      return max;
    }

    public static double Min2d(double[,] arr)
    {
      double min = double.PositiveInfinity;
      for (int x = 0; x < arr.GetLength(0); x++)
      {
        for (int y = 0; y < arr.GetLength(1); y++)
        {
          if (arr[x, y] < min) min = arr[x, y];
        }
      }
      return min;
    }

    public static Bitmap SaveArrayToBitmap(double[,] data)
    {
      int x = data.GetLength(1);
      int y = data.GetLength(0);
      var max = double.NegativeInfinity;
      var min = double.PositiveInfinity;
      foreach (var num in data)
      {
        if (num > max) max = num;
        if (num < min) min = num;
      }
      var bmp = new Bitmap(x, y);
      data.Select2D((value, row, column) =>
      {
        var gray = (int)((value - min) / (max - min) * 255);
        lock (bmp)
          bmp.SetPixel(column, row, Color.FromArgb(gray, gray, gray));
        return value;
      });
      return bmp;
    }

    public static Bitmap SaveArrayToBitmapWithoutNorm(double[,] data)
    {
      int x = data.GetLength(1);
      int y = data.GetLength(0);
      var max = double.NegativeInfinity;
      var min = double.PositiveInfinity;
      foreach (var num in data)
      {
        if (num > max) max = num;
        if (num < min) min = num;
      }
      var bmp = new Bitmap(x, y);
      data.Select2D((value, row, column) =>
      {
        var gray = (int)value;
        lock (bmp)
          bmp.SetPixel(column, row, Color.FromArgb(gray, gray, gray));
        return value;
      });
      return bmp;
    }


    public static void SaveArrayAndOpen(double[,] data, string path)
    {
      SaveArrayToBitmapWithoutNorm(data).Save(path);
      Process.Start(path);
    }
  }
}
