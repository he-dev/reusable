using System;

namespace Reusable.IO
{
    public class CreateDirectoryException : Exception
    {
        public CreateDirectoryException(string path, Exception innerException)
            : base($"Could not create directory: {path}", innerException)
        { }
    }

    public class CreateFileException : Exception
    {
        public CreateFileException(string path, Exception innerException)
            : base($"Could not create file: {path}", innerException)
        { }
    }

    public class DeleteDirectoryException : Exception
    {
        public DeleteDirectoryException(string path, Exception innerException)
            : base($"Could not delete directory: {path}", innerException)
        { }
    }

    public class DeleteFileException : Exception
    {
        public DeleteFileException(string path, Exception innerException)
            : base($"Could not delete file: {path}", innerException)
        { }
    }
}
