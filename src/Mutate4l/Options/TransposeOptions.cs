using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Options
{
    public enum TransposeMode
    {
        Absolute,
        Relative
    }

    public class TransposeOptions
    {
        public TransposeMode Mode { get; set; } = TransposeMode.Relative;

        //public ClipReference By { get; set; } // Allows syntax like a1 transpose -by a2 -mode relative. This syntax makes it much clearer which clip is being affected, and which is used as the source.
    }
}
