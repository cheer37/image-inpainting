using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageInpainting
{
  internal static class Constants
  {
    public const double DeltaT = 0.1; // delta of time
    
    // ?
    public const int Epsilon = 5; // radius of dilation Sigma (red space)

    public static int[,] SobelFilterX = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
    public static int[,] SobelFilterY = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
  }
}
