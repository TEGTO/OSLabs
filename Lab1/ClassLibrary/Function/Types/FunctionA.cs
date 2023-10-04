namespace ClassLibrary.Function.Types
{
    public class FunctionA : FunctionBase
    {
        private static int amountOfSoftErrors = 0;
        public override double MakeCalculations(double x)
        {
            if (amountOfSoftErrors < 1)
            {
                amountOfSoftErrors++;
                throw new InvalidOperationException();
            }
            return x * x;
        }
    }
}
