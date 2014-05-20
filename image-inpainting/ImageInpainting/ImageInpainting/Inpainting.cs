using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageInpainting
{
  // Do inpainting step by step
  public class Inpainting
  {
    private double[,] prevStep;
    private double[,] step;
    private bool[,] template;
    private int time;

    public int Time
    {
      get { return time; }
    }

    public double[,] Step
    {
      get { return step; }
    }

    public Inpainting(string imagePath, string templatePath)
    {
      double[,] image = Helper.LoadImage(imagePath);
      template = Helper.LoadTemplate(templatePath);

      // template area => black area (with value = 0)
      prevStep = image.Select2D((a, x, y) => ((template[x, y]) ? 0 : image[x, y]));
      step = prevStep.Select2D(x => x);

      time = 0;
    }

    public bool AreLastStepsEqual()
    {
      for (int x = 0; x < step.GetLength(0); x++)
      {
        for (int y = 0; y < step.GetLength(1); y++)
        {
          if (step[x, y] != prevStep[x, y])
          {
            return false;
          }
        }
      }
      return true;
    }

    // Calculate changed values in Sigma area
    public void Next()
    {
      prevStep = step.Select2D(x => x);

      double[,] secondDerivative = DerivativeHelper.CalculateSecondDerivative(prevStep, true);
      double[,] firstXDerivativeF = DerivativeHelper.CalculateFirstXDerivative(prevStep, true);
      double[,] firstYDerivativeF = DerivativeHelper.CalculateFirstYDerivative(prevStep, true);
      double[,] firstXDerivativeB = DerivativeHelper.CalculateFirstXDerivative(prevStep, false);
      double[,] firstYDerivativeB = DerivativeHelper.CalculateFirstYDerivative(prevStep, false);
      double betta, deltaI, factor;

      for (int x = 0; x < prevStep.GetLength(0); x++)
      {
        for (int y = 0; y < prevStep.GetLength(1); y++)
        {
          if (template[x, y])
          {
            betta = CalculateBeta(prevStep, x, y, firstXDerivativeF, firstYDerivativeF, secondDerivative);
            deltaI = CalculateDeltaI(firstXDerivativeB[x, y], firstXDerivativeF[x, y], firstYDerivativeB[x, y], firstYDerivativeF[x, y], betta > 0);
            factor = betta * deltaI;
            step[x, y] += Constants.DeltaT * factor;
          }
        }
      }

      NormalizeStep();
      time++;

      Helper.SaveArrayAndOpen(step, @"..\..\..\..\res\" + time + "_res.png");
      //Helper.WriteAndOpen(step, "Step " + time);
    }

    // How to do..?
    private void NormalizeStep()
    {
      double maxVal = double.MinValue;
      double minVal = double.MaxValue;
      step.Select2D((value, x, y) =>
      {
        if (template[x, y])
        {
          if (value > maxVal)
            maxVal = value;
          if (value < minVal)
          {
            minVal = value;
          }
        }
        return 0;
      });

      step = step.Select2D((value, x, y) =>
        {
          if (template[x, y])
          {
            if (255 * (value - minVal) / (maxVal - minVal) < 0 || 255 * (value - minVal) / (maxVal - minVal) > 255)
            {

            }
            double f = 1.0 * Math.Ceiling((value - minVal) / (maxVal - minVal) * 100) / 100;

            return 255 * f;
          }
          return value;
        });
    }

    private double CalculateBeta(double[,] img, int x, int y, double[,] firstXDerivative, double[,] firstYDerivative, double[,] secondDerivative)
    {
      Tuple<double, double> a = CalulateDirectionOfSecondDerivative(img, x, y, secondDerivative);
      Tuple<double, double> b = CalculateN(img, x, y, firstXDerivative, firstYDerivative);
      return a.Item1 * b.Item1 + a.Item2 * b.Item2;
    }

    private Tuple<double, double> CalulateDirectionOfSecondDerivative(double[,] img, int x, int y, double[,] secondDerivative)
    {
      if (x < 1 || y < 1 || x >= img.GetLength(0) - 1 || y >= img.GetLength(1) - 1)
      {
        return new Tuple<double, double>(0, 0);
      }

      double i = secondDerivative[x + 1, y] - secondDerivative[x - 1, y];
      double j = secondDerivative[x, y + 1] - secondDerivative[x, y - 1];
      return new Tuple<double, double>(i, j); ;
    }

    private Tuple<double, double> CalculateN(double[,] img, int x, int y, double[,] firstXDerivative, double[,] firstYDerivative)
    {
      double denominator = Math.Sqrt(firstXDerivative[x, y] * firstXDerivative[x, y] + firstYDerivative[x, y] * firstYDerivative[x, y] + Constants.Epsilon);
      double i = -1 * firstYDerivative[x, y] / denominator;
      double j = firstXDerivative[x, y] / denominator;
      return new Tuple<double, double>(i, j);
    }

    private double CalculateDeltaI(double d_XB, double d_XF, double d_YB, double d_YF, bool condition)
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
  }
}
