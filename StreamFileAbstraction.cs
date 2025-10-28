/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

namespace PodcastWebApp.Controllers
{
    public class StreamFileAbstraction : TagLib.File.IFileAbstraction
    {
        private readonly string _fileName;
        private readonly Stream _readStream;

        public StreamFileAbstraction(string fileName, Stream readStream, Stream? writeStream = null)
        {
            _fileName = fileName;
            _readStream = readStream;
        }

        public string Name => _fileName;

        public Stream ReadStream => _readStream;

        public Stream WriteStream => throw new NotSupportedException("Writing is not supported");

        public void CloseStream(Stream stream)
        {
            // Don't close the stream here - let the caller manage it
        }
    }
}