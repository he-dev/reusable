namespace Reusable.MQLite
{
    public interface IBodySerializer
    {
        byte[] Serialize(object obj);

        object Deserialize(byte[] body);
    }
}