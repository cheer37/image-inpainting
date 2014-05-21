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
      string iPath = @"..\..\..\..\res\0_res.png";
      string tPath = @"..\..\..\..\res\0_temp.png";
      Inpainting inpt = new Inpainting(iPath, tPath);

      while (inpt.Time < 2 || !inpt.AreLastStepsEqual())
      {
        Console.WriteLine("**** Next");
        Console.WriteLine("Step {0}", inpt.Time);
        inpt.Next();
      }

      //Helper.SaveArrayAndOpen(inpt.Step, @"..\..\..\..\res\" + inpt.Time + "_res.png");
    }
  }
}
