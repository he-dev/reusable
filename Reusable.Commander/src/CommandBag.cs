using System.ComponentModel;

namespace Reusable.Commander
{
    public interface ICommandBag
    {
        /// <summary>
        /// Specifies whether a command can throw exceptions and thus cancel the execution of other commands in chain or async.
        /// </summary>
        //bool CanThrow { get; set; }
        
        /// <summary>
        /// Specifies whether a command can be executed asynchronously.
        /// </summary>
        bool Async { get; set; }
    }
    
    public class SimpleBag : ICommandBag
    {
        //[DefaultValue(false)]
        //public bool CanThrow { get; set; }
        
        [DefaultValue(false)]
        public bool Async { get; set; }
    }       
}