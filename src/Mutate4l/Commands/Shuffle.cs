using Mutate4l.Core;
using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mutate4l.Commands
{
    public class ShuffleOptions
    {
        public Clip From { get; set; }
    }

    public class Shuffle
    {
        public static ProcessResultArray<Clip> Apply(ShuffleOptions options, params Clip[] clips)
        {
            var c = 0;
            if (options.From == null) options.From = clips[0];
            var targetClips = new Clip[clips.Length];
            foreach (var clip in clips) // we only support one generated clip since these are tied to a specific clip slot. Maybe support multiple clips under the hood, but discard any additional clips when sending the output is the most flexible approach.
            {
                targetClips[c] = new Clip(4m, true);
                // scale notes to indexes
                int maxPitch = options.From.Notes.Max(x => x.Pitch);
                int minPitch = options.From.Notes.Min(x => x.Pitch);
                int range = maxPitch - minPitch;
                if (range == 0) range = 1;

                var numShuffleIndexes = options.From.Notes.Count;
                if (numShuffleIndexes < clip.Notes.Count) numShuffleIndexes = clip.Notes.Count;
                var indexes = new int[numShuffleIndexes];
                for (var i = 0; i < numShuffleIndexes; i++)
                {
                    // Calc shuffle indexes as long as there are notes in the source clip. If the clip to be shuffled contains more events than the source, add zero-indexes so that the rest of the sequence is produced sequentially.
                    if (i < options.From.Notes.Count)
                    {
                        indexes[i] = (int)Math.Floor(((options.From.Notes[i].Pitch - minPitch - 0f) / range) * options.From.Notes.Count);
                    } else
                    {
                        indexes[i] = 0;
                    }
                }

                // do shuffle
                var j = 0;
                decimal pos = 0m;
                while (clip.Notes.Count > 0)
                {
                    int currentIx = indexes[j++] % clip.Notes.Count;
                    var newNote = new NoteEvent(clip.Notes[currentIx]);
                    newNote.Start = pos;
                    targetClips[c].Notes.Add(newNote);
                    pos += clip.DurationUntilNextNote(currentIx);
                    clip.Notes.RemoveAt(currentIx);
                }
                c++;
            }

            return new ProcessResultArray<Clip>(targetClips);
        }
    }
}
