﻿namespace Mutate4l.Cli
{
    public enum TokenType
    {
        _CommandsBegin,
        Interleave,
        Constrain,
        Slice,
        Explode,
        Arpeggiate,
        Sustain,
        Filter,
        Monophonize,
        Ratchet,
        Scan,
        Transpose,
        _CommandsEnd,

        _OptionsBegin,
        Start,
        Pitch,
        Ranges,
        Repeats,
        Mode,
        Strength,
        Mask,
        Lengths,
        Rescale,
        RemoveOffset,
        Min,
        Max,
        Shape,
        AutoScale,
        ControlMin,
        ControlMax,
        VelocityToStrength,
        Count,
        Window,
        Duration,
        ChunkChords,
        EnableMask,
        By,
        _OptionsEnd,

        _EnumValuesBegin,
        Event,
        Time,
        Linear,
        EaseInOut,
        EaseIn,
        Pitches, // todo: Quickfix to avoid conflict with Pitch. Need to find a better solution here...
        Rhythm,
        Both,
        Absolute,
        Relative,
        _EnumValuesEnd,

        _ValuesBegin,
        ClipReference,
        Number,
        MusicalDivision,
        InlineClip,
        _ValuesEnd,

        Colon,
        Destination,
        AddToDestination,
        Unset,

        _TestOptionsBegin,
        GroupOneToggleOne,
        GroupOneToggleTwo,
        GroupTwoToggleOne,
        GroupTwoToggleTwo,
        DecimalValue,
        IntValue,
        EnumValue,
        SimpleBoolFlag,
        _TestOptionsEnd,

        _TestEnumValuesBegin,
        EnumValue1,
        EnumValue2,
        _TestEnumValuesEnd
    }
}
