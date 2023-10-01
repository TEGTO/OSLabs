using System.Numerics;
namespace Lab1.Function
{
    public interface IFunctionBase
    {
        public T MakeCalculations<T>(T x) where T : INumber<T>;
    }
}
