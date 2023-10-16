using Lab1.Function;

namespace ClassLibrary.Function.Types
{
    public class FunctionA : IFunctionBase
    {
        private static int amountOfSoftErrors = 0;
        public double MakeCalculations(double x)
        {
            if (amountOfSoftErrors < 2)
            {
                amountOfSoftErrors++;
                throw new InvalidOperationException();
            }
            return x*x;
        }
    }
}
