using Mutate4l.Core;

namespace Mutate4l.Options
{
    public enum ConstrainMode
    {
        Pitch,
        Rhythm,
        Both
    }

    public class ConstrainOptions
    {
        public ConstrainMode Mode { get; set; }

        [OptionInfo(min: 1, max: 100)]
        public int Strength { get; set; } = 100;
    }
}
