/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

namespace PodcastWebApp.Controllers
{
    internal class StreamFileAbstraction : TagLib.File.IFileAbstraction
    {
        private string fileName;
        private Stream stream;
        private object value;
        private Stream metadataStream;

        public StreamFileAbstraction(string fileName, Stream metadataStream)
        {
            this.fileName = fileName;
            this.metadataStream = metadataStream;
        }

        public StreamFileAbstraction(string fileName, Stream stream, object value)
        {
            this.fileName = fileName;
            this.stream = stream;
            this.value = value;
        }

        public string Name => throw new NotImplementedException();

        public Stream ReadStream => throw new NotImplementedException();

        public Stream WriteStream => throw new NotImplementedException();

        public void CloseStream(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}