namespace ClassLibrary.Function.Types
{
    public class FunctionB : FunctionBase
    {
        public override double MakeCalculations(double x) =>
            Math.Sin(x);
    }
}
