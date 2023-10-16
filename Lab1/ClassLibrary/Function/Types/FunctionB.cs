using Lab1.Function;

namespace ClassLibrary.Function.Types
{
    public class FunctionB : IFunctionBase
    {
        public double MakeCalculations(double x)
        {
            return Math.Sin(x);
        }
    }
}
