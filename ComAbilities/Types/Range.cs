namespace ComAbilities.Types
{
    public class Range {
        public int Min { get; set; }

        public int Max { get; set; }

        public Range()
        {
        }

        public Range(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public void Deconstruct(out int min, out int max)
        {
            min = Min;
            max = Max;
        }
    };
}