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
      double[,] image = Helper.LoadImage(@"D:\Source\image-inpainting\circules.PNG");
      bool[,] isRed = Helper.LoadTemplate(@"D:\Source\image-inpainting\circulesTemplate.PNG");
      int xLength = image.GetLength(0); // rename
      int yLength = image.GetLength(1); // rename

      List<double[,]> inpaintingSteps = new List<double[,]>();
      double[,] step = image;
      double[,] previousStep = image;
      double[,] factor = new double[xLength, yLength];
      int time = 0;

      
      string name = "_temp.png";
      inpaintingSteps.Add(image);

      step = step.Select2D((a, x, y) => ((isRed[x, y]) ? 0 : image[x, y]));
      
      while ((time <=2 || !IsEqualLastSteps(step, previousStep))&&time<20)
      {
        previousStep = (double[,])step.Clone();
        factor = CalculateFactor(step);
        factor = Helper.Normalisation(factor);
        //Helper.SaveArrayAndOpen(factor, Path.GetTempPath() + "factor.png");

        // Helper.SaveArrayAndOpen(step, Path.GetTempPath() + "step.png");
        

        double sum = 0;
        factor.Select2D(x => sum += x);

        step = step.Select2D((value, x, y) =>
        {
          if (!isRed[x, y])
          {
            return value;
          }
          double a = value + Constants.DeltaT*factor[x, y];
          if (a!=0)
          {
            isRed[x, y] = false;
          }
          if (a > 255)
          {
            return 255;
          }
          if (a < 0)
          {
            return 0;
          }
          return a;
          
        });
           
        
        inpaintingSteps.Add(step);
        // Helper.SaveArrayAndOpen(step, Path.GetTempPath() + "step.png");
        Helper.SaveArrayAndOpen(step, Path.GetTempPath() + time + name);
        time++;
      }
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
    private static double[,] CalculateFactor(double[,] img)
    {
      double[,] newImg = (double[,]) img.Clone();
      double[,] betta = CalculateBeta(newImg);
      double[,] deltaI = CalculateDeltaI(newImg, betta);

      return betta.Select2D((value, x, y) => value * deltaI[x, y]);
    }

    // формула (11)
    private static double[,] CalculateDeltaI(double[,] img, double[,] betta)
    {
      double[,] d_XB = CalculateFirstXDerivative(img, false);
      double[,] d_XF = CalculateFirstXDerivative(img, true);
      double[,] d_YB = CalculateFirstYDerivative(img, false);
      double[,] d_YF = CalculateFirstYDerivative(img, true);

      return img.Select2D((value, x, y) => DeltaIForPixel(d_XB[x, y], d_XF[x, y], d_YB[x, y], d_YF[x, y], betta[x, y] > 0));
    }

    private static double DeltaIForPixel(double d_XB, double d_XF, double d_YB, double d_YF, bool condition)
    {
      if (condition)
      {
        d_XB = Math.Min(d_XB, 0);
        d_XF = Math.Max(d_XF, 0);
        d_YB = Math.Min(d_YB, 0);
        d_YF = Math.Max(d_YF, 0);
      }
      else
      {
        d_XB = Math.Max(d_XB, 0);
        d_XF = Math.Min(d_XF, 0);
        d_YB = Math.Max(d_YB, 0);
        d_YF = Math.Min(d_YF, 0);
      }

      return Math.Sqrt(d_XB * d_XB + d_XF * d_XF + d_YB * d_YB + d_YF * d_YF);
    }


    private static double[,] CalculateFirstXDerivative(double[,] img, bool IsForward)
    {
      return Helper.Select2D(img, (a, x, y) => CalculateDerivativeX(img, x, y, IsForward));
    }

    private static double[,] CalculateFirstYDerivative(double[,] img, bool IsForward)
    {
      return Helper.Select2D(img, (a, x, y) => CalculateDerivativeY(img, x, y, IsForward));
    }

    /// <summary>
    /// Ixx  in Image Inpainting 
    /// </summary>
    /// <param name="img"></param>
    /// <param name="IsForward"></param>
    /// <returns></returns>
    private static double[,] CalculateSecondXXDerivative(double[,] img, bool IsForward)
    {
      double[,] firstDerivative = CalculateFirstXDerivative(img, IsForward);

      return Helper.Select2D(firstDerivative, (a, x, y) => CalculateDerivativeX(firstDerivative, x, y, IsForward));
    }

    /// <summary>
    /// Iyy  in Image Inpainting 
    /// </summary>
    /// <param name="img"></param>
    /// <param name="IsForward"></param>
    /// <returns></returns>
    private static double[,] CalculateSecondYYDerivative(double[,] img, bool IsForward)
    {
      double[,] firstDerivative = CalculateFirstYDerivative(img, IsForward);

      return Helper.Select2D(firstDerivative, (a, x, y) => CalculateDerivativeY(firstDerivative, x, y, IsForward));
    }

    /// <summary>
    /// L in Image Inpainting 
    /// </summary>
    /// <param name="img"></param>
    /// <param name="IsForward"></param>
    /// <returns></returns>
    private static double[,] CalculateSecondDerivative(double[,] img, bool IsForward)
    {
      return Helper.Add(CalculateSecondXXDerivative(img, IsForward), CalculateSecondYYDerivative(img, IsForward));
    }

    /// <summary>
    /// DirectionOfSecondDerivative
    /// </summary>
    /// <param name="img"></param>
    /// <returns></returns>
    private static Tuple<double, double>[,] CalulateDirectionOfSecondDerivative(double[,] img)
    {
      double[,] SecondDerivative = CalculateSecondDerivative(img, true);
      Tuple<double, double>[,] res = new Tuple<double, double>[img.GetLength(0), img.GetLength(1)];

      res = res.Select2D((a, x, y) =>
      {
        if (x < 1 || y < 1 || x >= img.GetLength(0) - 1 || y >= img.GetLength(1) - 1)
        {
          return new Tuple<double, double>(0, 0);
        }

        double i = SecondDerivative[x + 1, y] - SecondDerivative[x - 1, y];
        double j = SecondDerivative[x, y + 1] - SecondDerivative[x, y - 1];
        Tuple<double, double> newTuple = new Tuple<double, double>(i, j);
        return newTuple;
      });

      return res;
    }



    private static Tuple<double, double>[,] CalulateN(double[,] img)
    {
      double[,] xDerivative = CalculateFirstXDerivative(img, true);
      double[,] yDerivative = CalculateFirstYDerivative(img, true);

      double[,] denominator = xDerivative.Select2D((a,x,y) => Math.Sqrt(x*x + yDerivative[x,y]*yDerivative[x,y] + Constants.Epsilon));

      Tuple<double, double>[,] res = new Tuple<double, double>[img.GetLength(0), img.GetLength(1)];

      res = res.Select2D((a, x, y) =>
      {
        double i = -yDerivative[x,y]/denominator[x,y];
        double j = xDerivative[x, y] / denominator[x, y];
        Tuple<double, double> newTuple = new Tuple<double, double>(i, j);
        return newTuple;
      });
      return res;

    }

    static private double[,] CalculateBeta(double[,] img)
    {
      Tuple<double, double>[,] a = CalulateDirectionOfSecondDerivative(img);
      Tuple<double, double>[,] b = CalulateN(img);
      double[,] res = new double[img.GetLength(0),img.GetLength(1)];
      res = res.Select2D((z, x, y) => a[x, y].Item1*b[x, y].Item1 + a[x, y].Item2*b[x, y].Item2);
      return res;
    }

    /// <summary>
    /// Calculating first derivativ by convolving with sobel filter for one pixel of image
    /// </summary>
    /// <param name="img">Image on current(n) step (I(n) in Image Inpainting </param>
    /// <param name="x">x coordinate of pixel </param>
    /// <param name="y">y coordinate of pixel</param>
    /// <param name="isX"> </param>
    /// <param name="isForward"></param>
    /// <returns></returns>

    private static double CalculateDerivative(double[,] img, int x, int y, bool isX, bool isForward)
    {
      double[,] gX = new double[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
      double[,] gY = new double[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
      double dX = 0, dY = 0;

      if (x < 1 || y < 1 || x >= img.GetLength(0) - 1 || y >= img.GetLength(1) - 1)
      {
        return 0;
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

    public static double CalculateDerivativeX(double[,] image, int x, int y, bool isForward)
    {
      double value = 0;
      int x_New = 0;
      int y_New = 0;
      double color = 0;

      for (int dy = -1; dy < 2; dy++)
      {
        for (int dx = -1; dx < 2; dx++)
        {
          x_New = x + dx;
          y_New = y + dy;
          color = 0;

          if (x_New < 0 || x_New >= image.GetLength(0) || y_New < 0 || y_New >= image.GetLength(1))
          {
            color = image[x, y];
          }
          else
          {
            color = image[x_New, y_New];
          }

          value += color * Constants.SobelFilterX[dy + 1, dx + 1] * (isForward ? 1 : -1);
        }
      }

      return value;
    }

    public static double CalculateDerivativeY(double[,] image, int x, int y, bool isForward)
    {
      double value = 0;
      int x_New = 0;
      int y_New = 0;
      double color = 0;

      for (int dy = -1; dy < 2; dy++)
      {
        for (int dx = -1; dx < 2; dx++)
        {
          y_New = y + dx;
          x_New = x + dy;
          color = 0;

          if (x_New < 0 || x_New >= image.GetLength(0) || y_New < 0 || y_New >= image.GetLength(1))
          {
            color = image[x, y];
          }
          else
          {
            color = image[x_New, y_New];
          }

          value += color * Constants.SobelFilterY[dy + 1, dx + 1] * (isForward ? 1 : -1);
        }
      }

      return value;
    }
  }
}
