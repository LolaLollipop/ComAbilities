// A way to handle coroutine safely (ensures that they don't keep running)
using MEC;

namespace ComAbilities.Types
{
    public struct Range
    {
        public int Max { get; }
        public int Min { get; }
        public Range(int max, int min)
        {
            Max = max;
            Min = min;
        }

    }
}
