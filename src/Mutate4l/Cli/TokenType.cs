﻿namespace Mutate4l.Cli
{
    public enum TokenType
    {
        CommandName,
        OptionHeader,
        OptionValue,

        _CommandsBegin,
        Arpeggiate,
        Constrain,
        Explode,
        Filter,
        Interleave,
        InterleaveEvent,
        Monophonize,
        Ratchet,
        Relength,
        Resize,
        Scan,
        Shuffle,
        Slice,
        Sustain,
        Take,
        Transpose,
        _CommandsEnd,

        _OptionsBegin,
        AutoScale,
        By,
        ChunkChords,
        ControlMax,
        ControlMin,
        Count,
        Duration,
        EnableMask,
        Factor,
        Lengths,
        Max,
        Min,
        Mode,
        Pitch,
        Ranges,
        RemoveOffset,
        Repeats,
        Rescale,
        Shape,
        Skip,
        Solo,
        Start,
        Strength,
        Strict,
        VelocityToStrength,
        Window,
        With,
        _OptionsEnd,

        _EnumValuesBegin,
        Absolute,
        Both,
        EaseIn,
        EaseInOut,
        Event,
        Linear,
        Overwrite,
        Pitches, // todo: Quickfix to avoid conflict with Pitch. Need to find a better solution here...
        Relative,
        Rhythm,
        Time,
        _EnumValuesEnd,

        _ValuesBegin,
        ClipReference,
        Decimal,
        InlineClip,
        MusicalDivision,
        Number,
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
