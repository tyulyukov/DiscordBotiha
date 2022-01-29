using System;
using System.Collections.Generic;
using System.Text;
using Victoria;

namespace DiscordBotiha
{
    public class TrackList
    {
        public List<LavaTrack> Tracks { get; private set; }

        private Random random;

        public LavaTrack CurrentTrack
        {
            get
            {
                if (Tracks.Count == 0)
                    return null;

                return Tracks[currentPlayingPosition];
            }
        }

        private int currentPlayingPosition;

        public TrackList()
        {
            Tracks = new();
            random = new Random();
            currentPlayingPosition = -1;
        }

        public LavaTrack Next()
        {
            if (currentPlayingPosition == Tracks.Count - 1)
                return null;

            currentPlayingPosition++;
            return CurrentTrack;
        }

        public LavaTrack Previous()
        {
            if (currentPlayingPosition == 0)
                return null;

            currentPlayingPosition--;
            return CurrentTrack;
        }

        public void FullShuffle()
        {
            int count = Tracks.Count;

            while (count > 1)
            {
                count--;
                int index = random.Next(count + 1);
                var value = Tracks[index];
                Tracks[index] = Tracks[count];
                Tracks[count] = value;
            }

            currentPlayingPosition = 0;
        }

        public void ShuffleQueue()
        {
            int count = Tracks.Count;

            while (count > currentPlayingPosition)
            {
                count--;
                int index = random.Next(currentPlayingPosition, count + 1);
                var track = Tracks[index];
                Tracks[index] = Tracks[count];
                Tracks[count] = track;
            }
        }

        public void Clear() => Tracks.Clear();

        public override string ToString()
        {
            StringBuilder builder = new();

            for (int i = 0; i < Tracks.Count; i++)
            {
                if (CurrentTrack == Tracks[i])
                    builder.Append("-> ");

                builder.AppendLine(i + " " + Tracks[i].Title);
            }

            return builder.ToString();
        }
    }
}
