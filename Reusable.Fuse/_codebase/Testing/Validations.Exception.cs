using System;

namespace Reusable.Fuse.Testing
{
    public static class ExceptionValidation
    {
        public static void Throws<TExpectedException>(this ICurrent<Action> current, Action<TExpectedException> exceptionValidationAction = null)
            where TExpectedException : Exception
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            try
            {
                current.Value();
                throw new VerificationException($"Exception of type '{typeof(TExpectedException)}' was expected but none has been thrown.");
            }            
            catch (TExpectedException ex)
            {
                exceptionValidationAction?.Invoke(ex);
            }
            catch (Exception ex)
            {
                throw new VerificationException($"Exception of type '{typeof(TExpectedException)}' was expected but '{ex.GetType()}' has been thrown.", ex);
            }
        }

        public static void DoesNotThrow(this ICurrent<Action> current)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            try
            {
                current.Value();
            }
            catch (Exception ex)
            {
                throw new VerificationException($"Exception of type '{ex.GetType()}' was thrown but none has been expected.", ex);
            }
        }
    }    
}
