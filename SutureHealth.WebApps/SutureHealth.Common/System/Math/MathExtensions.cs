namespace System
{
    public static class MathExtensions
    {
        //compare floating point numbers
        public static bool NearlyEqual(this float a, float b, float epsilon)
        {
            const float FLOATNORMAL = (1 << 23) * float.Epsilon;

            if (a == b)
            {
                // Shortcut, handles infinities
                return true;
            }

            var diff = System.Math.Abs(a - b);
            if (a == 0.0f || b == 0.0f || diff < FLOATNORMAL)
            {
                // a or b is zero, or both are extremely close to it.
                // relative error is less meaningful here
                return diff < (epsilon * FLOATNORMAL);
            }

            var absA = System.Math.Abs(a);
            var absB = System.Math.Abs(b);
            // use relative error
            return diff / System.Math.Min((absA + absB), float.MaxValue) < epsilon;
        }
    }
}
