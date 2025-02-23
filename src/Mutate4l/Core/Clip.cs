﻿using System;
using System.Collections.Generic;

namespace Mutate4l.Core
{
    public struct ClipReference
    {
        public int Track;
        public int Clip;

        public ClipReference(int track, int clip)
        {
            Track = track;
            Clip = clip;
        }
    }

    public class Clip : IComparable<Clip>
    {
        public SortedList<NoteEvent> Notes { get; set; }
        public int Count { get { return Notes.Count; } }
        public decimal Length { get; set; }
        public bool IsLooping { get; set; }
        public ClipReference ClipReference { get; set; }
      
        public string RawClipReference { get; set; }
        
        public decimal EndDelta
        {
            get { return Length - Math.Clamp(Notes[Count - 1].Start, 0, Length) + Notes[0].Start; }
        }
        public decimal EndDeltaSilent
        {
            get { return Length - Math.Clamp(Notes[Count - 1].End, 0, Length) + Notes[0].Start; }
        }
        public bool SelectionActive { get; private set; }

        public Clip(decimal length, bool isLooping)
        {
            Notes = new SortedList<NoteEvent>();
            IsLooping = isLooping;
            Length = length;
        }

        public Clip(Clip clip) : this(clip.Length, clip.IsLooping)
        {
            foreach (var note in clip.Notes)
            {
                var clonedNote = new NoteEvent(note);
                Notes.Add(clonedNote);
            }
            ClipReference = clip.ClipReference;
            RawClipReference = clip.RawClipReference;
        }

        public void Add(NoteEvent noteEvent)
        {
            Notes.Add(noteEvent);
        }

        public void AddRange(List<NoteEvent> noteEvents)
        {
            Notes.AddRange(noteEvents);
        }

        public int CompareTo(Clip b)
        {
            if (Length < b.Length)
            {
                return -1;
            }
            if (Length > b.Length)
            {
                return 1;
            }
            return 0;
        }

        public decimal DurationUntilNextNote(int index)
        {
            // todo: warn if index > Notes.Count - 1
            if (index >= Count - 1)
                return EndDelta;
            else
                return Notes[index + 1].Start - Notes[index].Start;
        }
        
        public decimal DurationUntilNextNoteOrEndOfClip(int index)
        {
            // todo: warn if index > Notes.Count - 1
            if (index >= Count - 1)
                return Length - Math.Clamp(Notes[Count - 1].Start, 0, Length);
            else
                return Notes[index + 1].Start - Notes[index].Start;
        }

        public decimal DurationBetweenNotes(int start, int end)
        {
            var endTime = end >= Count ? Length : Notes[end].Start;
            var startTime = start >= Count ? Length : Notes[start].Start;

            return endTime - startTime;
        }

        // useful when the length of an event has changed and you want to consider only the interval of silence (if any) preceding the next event
        public decimal SilentDurationUntilNextNote(int index)
        {
            if (index >= Count - 1)
                return EndDeltaSilent;
            var silentDuration = Notes[index + 1].Start - Notes[index].End;
            return silentDuration > 0 ? silentDuration : 0;
        }

        // returns pitch relative to first note of clip
        public int RelativePitch(int index)
        {
            if (Notes.Count == 0) return 0;
            return Notes[Math.Clamp(index, 0, Count)].Pitch - Notes[0].Pitch;
        }

        /*        public NoteInfo GetNextNoteInfo()
                {
                    Note note;
                    if (Index == Notes.Count - 1)
                    {
                        note = Notes[Index];
                        Index = 0;
                        Retriggered = true;
                        return new NoteInfo { Start = note.Start, Duration = note.Duration, Pitch = note.Pitch, Velocity = note.Velocity, DurationUntilNextNote = EndDelta };
                    }
                    note = Notes[Index];
                    var result = new NoteInfo { Start = note.Start, Duration = note.Duration, Pitch = note.Pitch, Velocity = note.Velocity, DurationUntilNextNote = Notes[Index + 1].Start - Notes[Index].Start };
                    Index++;
                    return result;
                }*/
    }
}
