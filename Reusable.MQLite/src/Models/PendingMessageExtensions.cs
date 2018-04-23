namespace Reusable.MQLite.Models
{
    public static class PendingMessageExtensions
    {
        public static T BodyAs<T>(this PendingMessage message, IBodySerializer serializer)
        {
            return (T)serializer.Deserialize(message.Body);
        }
    }
}