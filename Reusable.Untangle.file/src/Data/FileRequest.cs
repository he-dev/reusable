namespace Reusable.Translucent.Data;

public class FileRequest : Request
{
    public class Text : FileRequest { }

    public class Stream : FileRequest { }
        
    public class Binary : FileRequest { }
}