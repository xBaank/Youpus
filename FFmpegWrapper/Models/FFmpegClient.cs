﻿using System.IO;
using System.Threading.Tasks;

using FFmpegWrapper.Builders;
using FFmpegWrapper.Formats;
using FFmpegWrapper.Helpers;

namespace FFmpegWrapper.Models
{
    public class FFmpegClient : Client
    {
        private readonly FFmpegProcessBuilder _builder = new FFmpegProcessBuilder();

        public FFmpegClient(string path) : base(path) => _builder.CreateFFBuilder(path);
        public FFmpegClient() : base(PathUtils.TryGetFFmpegPath()) { }

        public Task ConvertToStreamAsync(string input, Stream output, IFormat outputType) => _builder
            .CreateFFBuilder(Path)
            .RedirectError(true)
            .RaiseErrorEvents(ErrorRecieved)
            .RaiseExitErrorEvent(ExitWithErrorRecieved)
            .From(input)
            .To(output, outputType)
            .Build()
            .StartAsync();

        public Task ConvertToStreamAsync(Stream input, IFormat inputType, Stream output, IFormat outputType) => _builder
            .CreateFFBuilder(Path)
            .RedirectError(true)
            .RaiseErrorEvents(ErrorRecieved)
            .RaiseExitErrorEvent(ExitWithErrorRecieved)
            .From(input, inputType)
            .To(output, outputType)
            .Build()
            .StartAsync();

        public Task ConvertAsync(string input, string output) => _builder.CreateFFBuilder(Path)
            .RedirectError(true)
            .RaiseErrorEvents(ErrorRecieved)
            .RaiseExitErrorEvent(ExitWithErrorRecieved)
            .From(input)
            .To(output)
            .Build()
            .StartAsync();
        public Task ConvertAsync(Stream input, string output, IFormat inputType) => _builder
            .CreateFFBuilder(Path)
            .RedirectError(true)
            .RaiseErrorEvents(ErrorRecieved)
            .RaiseExitErrorEvent(ExitWithErrorRecieved)
            .From(input, inputType)
            .To(output)
            .Build()
            .StartAsync();
    }

}
