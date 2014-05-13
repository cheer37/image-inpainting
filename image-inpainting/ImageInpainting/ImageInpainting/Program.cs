using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace ImageInpainting
{
  public class Program
  {
    public static void Main(string[] args)
    {
      double[,] image = LoadImage(Resourses.Grayscale);
      bool[,] isRed = LoadTemplate(Resourses.GrayscaleTemplate);
      int n = image.GetLength(0); // rename
      int m = image.GetLength(1); // rename
      List<double[,]> inpaintingSteps = new List<double[,]>();
      double[,] step = new double[n, m];
      double[,] previousStep = new double[n, m];
      int time = 0;

      while (inpaintingSteps.Count < 2 && !IsEqualLastSteps(step, previousStep))
      {
        step = new double[n, m];

        for (int x = 0; x < n; x++)
        {
          for (int y = 0; y < m; y++)
          {
            step[x, y] = previousStep[x, y] + Constants.DeltaT * CalculateFactor(image, x, y, time);
          }
        }

        previousStep = (double[,]) step.Clone(); 
        time++;
      }

    }

    private static double[,] LoadImage(string imageName)
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

    // Load red template
    private static bool[,] LoadTemplate(string templateName)
    {
      string fullPath = Path.GetFullPath(templateName);
      Bitmap bmp = new Bitmap(fullPath);
      bool[,] template = new bool[bmp.Height, bmp.Width];
      for (int x = 0; x < bmp.Width; x++)
      {
        for (int y = 0; y < bmp.Height; y++)
        {
          template[y, x] = bmp.GetPixel(x, y).R < 255;
        }
      }
      return template;
    }

    private static bool IsEqualLastSteps(double[,] step, double[,] previousStep)
    {
      for (int x = 0; x < step.GetLength(0); x++)
      {
        for (int y = 0; y < step.GetLength(1); y++)
        {
          if (step[x, y] != previousStep[x, y])
            return false;
        }
      }
      return true;
    }

    // формула (6)
    private static double CalculateFactor(double[,] img, int x, int y, int time)
    {
      double betta = CalculateBetta(x, y, time);
      return betta * CalculateDeltaI(img, x, y, time, betta);
    }

    // формула (10)
    private static double CalculateBetta(int x, int y, int time)
    {
      throw new NotImplementedException();
    }

    // формула (11)
    private static double CalculateDeltaI(double[,] img, int x, int y, int time, double betta)
    {
      double deltaI = 0;
      double d_XB = CalculateDerivative(img, x, y, true, false);
      double d_XF = CalculateDerivative(img, x, y, true, true);
      double d_YB = CalculateDerivative(img, x, y, false, false);
      double d_YF = CalculateDerivative(img, x, y, false, true);
      bool isBettaPositive = betta > 0;

      d_XB = isBettaPositive ? Math.Min(d_XB, 0) : Math.Max(d_XB, 0);
      d_XF = isBettaPositive ? Math.Max(d_XF, 0) : Math.Min(d_XF, 0);
      d_YB = isBettaPositive ? Math.Min(d_YB, 0) : Math.Max(d_YB, 0);
      d_YF = isBettaPositive ? Math.Max(d_YF, 0) : Math.Min(d_YF, 0);
      deltaI = Math.Sqrt(d_XB * d_XB + d_XF * d_XF + d_YB * d_YB + d_YF * d_YF);

      return deltaI;
    }

    private static double CalculateDerivative(double[,] img, int x, int y, bool isX, bool isForward)
    {
      double[,] gX = new double[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
      double[,] gY = new double[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
      double dX = 0, dY = 0;

      if (x < 1 || y < 1 || x >= img.GetLength(0) - 1 || y >= img.GetLength(1) - 1)
      {
        throw new ArgumentException();
      }

      for (int h = 0; h < 3; h++) // это там самая эпсилон (радиус) ?
      {
        for (int w = 0; w < 3; w++)
        {
          double curr = img[x + h - 1, y + w - 1];

          if (isX)
          {
            dX += isForward ? gX[h, w] * curr : (-1) * gX[h, w] * curr;
          }
          else
          {
            dY += isForward ? gY[h, w] * curr : (-1) * gY[h, w] * curr;
          }
        }
      }

      return isX ? dX : dY;
    }
  }
}
