using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace Reusable.Synergy.Requests;

public interface IWriteFile
{
    public string Name { get; set; }

    public object Value { get; set; }
    
    public bool Append { get; set; }
}

public class WriteFile : Request<Unit>, IWriteFile
{
    public WriteFile(string name, object value) => (Name, Value) = (name, value);

    public string Name { get; set; }
    
    public object Value { get; set; }

    public bool Append { get; set; }
}
