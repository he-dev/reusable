using System.ComponentModel;

namespace Reusable.Commander
{
    public interface ICommandBag
    {
        bool CanThrow { get; set; }
        
        bool Async { get; set; }
    }
    
    public class SimpleBag : ICommandBag
    {
        [DefaultValue(false)]
        public bool CanThrow { get; set; }
        
        [DefaultValue(false)]
        public bool Async { get; set; }
    }       
}