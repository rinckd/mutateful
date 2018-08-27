using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Commands
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

    public class Transpose
    {
        /*public static ProcessResultArray<Clip> Apply(TransposeOptions options, params Clip[] clips)
        {
            if (clips.Length < 2)
            {
                clips = new Clip[] { clips[0], clips[0] };
            }

            int basePitch = 60;
            if (options.Mode == TransposeMode.Relative)
            {
//                basePitch = 
            }


            return new ProcessResultArray<Clip>(new Clip[] { resultClip });
        }*/
    }
}
