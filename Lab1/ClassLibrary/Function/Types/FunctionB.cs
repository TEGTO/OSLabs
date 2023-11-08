using Lab1.Function;

namespace ClassLibrary.Function.Types
{
    public class FunctionB : IFunctionBase
    {
        public double MakeCalculations(double x)
        {
           // Thread.Sleep(10000);
            return Math.Sin(x);
        }
    }
}
