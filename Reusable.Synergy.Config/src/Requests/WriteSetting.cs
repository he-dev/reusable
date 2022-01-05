namespace Reusable.Synergy.Requests;

public interface IWriteSetting : IRequest
{
    public string Name { get; set; }

    public object Value { get; set; }
}

public class WriteSetting : Request<Unit>, IWriteSetting
{
    public WriteSetting(string name, object value) => (Name, Value) = (name, value);

    public string Name { get; set; }
    
    public object Value { get; set; }
}
