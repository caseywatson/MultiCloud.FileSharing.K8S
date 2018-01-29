using System;
using System.IO;

namespace MultiCloud.FileSharing.K8S.Storage.Extensions
{
    public static class StreamExtensions
    {
        public static bool TryToRewind(this Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stream.CanSeek)
            {
                stream.Position = 0;
                return true;
            }

            return false;
        }
    }
}
