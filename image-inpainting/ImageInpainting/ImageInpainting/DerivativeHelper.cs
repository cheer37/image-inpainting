using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageInpainting
{
  public static class DerivativeHelper
  {
    public static double CalculateSecondDerivative(double[,] img, int x, int y, bool isForward)
    {
      return CalculateSecondXXDerivative(img, x, y, isForward) + CalculateSecondYYDerivative(img, x, y, isForward);
    }

    public static double[,] CalculateSecondDerivative(double[,] img, bool isForward)
    {
      return Helper.Add(CalculateSecondXXDerivative(img, isForward), CalculateSecondYYDerivative(img, isForward));
    }

    public static double CalculateSecondXXDerivative(double[,] img, int x, int y, bool isForward)
    {
      double[,] firstDerivativeAll = img.Select2D((value, i, j) => CalculateDerivativeX(img, i, j, isForward));
      return CalculateDerivativeX(firstDerivativeAll, x, y, isForward);
    }

    public static double[,] CalculateSecondXXDerivative(double[,] img, bool isForward)
    {
      double[,] firstDerivativeAll = img.Select2D((value, i, j) => CalculateDerivativeX(img, i, j, isForward));
      return firstDerivativeAll.Select2D((value, i, j) => CalculateDerivativeX(firstDerivativeAll, i, j, isForward));
    }

    public static double CalculateSecondYYDerivative(double[,] img, int x, int y, bool isForward)
    {
      double[,] firstDerivativeAll = img.Select2D((value, i, j) => CalculateDerivativeY(img, i, j, isForward));
      return CalculateDerivativeY(firstDerivativeAll, x, y, isForward);
    }

    public static double[,] CalculateSecondYYDerivative(double[,] img, bool isForward)
    {
      double[,] firstDerivativeAll = img.Select2D((value, i, j) => CalculateDerivativeY(img, i, j, isForward));
      return firstDerivativeAll.Select2D((value, i, j) => CalculateDerivativeY(firstDerivativeAll, i, j, isForward));
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

    public static double[,] CalculateFirstXDerivative(double[,] img, bool IsForward)
    {
      return img.Select2D((value, x, y) => CalculateDerivativeX(img, x, y, IsForward));
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

    public static double[,] CalculateFirstYDerivative(double[,] img, bool IsForward)
    {
      return img.Select2D((value, x, y) => CalculateDerivativeY(img, x, y, IsForward));
    }
  }
}
