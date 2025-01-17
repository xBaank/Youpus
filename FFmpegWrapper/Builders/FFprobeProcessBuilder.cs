﻿using System.Globalization;
using System.IO;

using FFmpegWrapper.Formats;

namespace FFmpegWrapper.Builders
{
    public class FFprobeProcessBuilder : FFProcessBuilder<FFprobeProcessBuilder>
    {

        public override FFprobeProcessBuilder From(string input) =>
            AddArguments(input);

        public FFprobeProcessBuilder From(Stream input) =>
            AddArguments("pipe:")
            .SetInput(input);

        public override FFprobeProcessBuilder To(string output) =>
            To(new FileStream(output, FileMode.OpenOrCreate));

        public FFprobeProcessBuilder To(Stream output) =>
            SetOutput(output);

        public FFprobeProcessBuilder ReadIntervals() =>
           AddArguments("-read_intervals");

        public FFprobeProcessBuilder WithInterval(double timeStart, double timeAdded = 0)
        {
            if (timeAdded == 0)
                timeAdded = int.MaxValue;

            return AddArguments($"{timeStart.ToString(CultureInfo.InvariantCulture)}%+{timeAdded.ToString(CultureInfo.InvariantCulture)}");
        }

        public FFprobeProcessBuilder SelectStreams(StreamType streamType, int streamNumber = 0) =>
          AddArguments($"-select_streams {streamType}:{streamNumber}");

        public FFprobeProcessBuilder ShowPackets() =>
          AddArguments($"-show_packets");

        public FFprobeProcessBuilder ShowFrames() =>
          AddArguments($"-show_frames");

        public FFprobeProcessBuilder Reconnect() =>
           AddArguments("-reconnect 1");

        public FFprobeProcessBuilder ShowFormat() =>
            AddArguments("-show_format");

        public FFprobeProcessBuilder AsJson() =>
            AddArguments("-of json");

    }
}
