using System;

namespace Reusable.FluentValidation.Testing.Validations
{
    public static class ExceptionValidation
    {
        public static void Throws<TExpectedException>(this IValidationContext<Action> context, Action<TExpectedException> exceptionValidationAction = null)
            where TExpectedException : Exception
        {
            try
            {
                context.Value();
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

        public static void DoesNotThrow(this IValidationContext<Action> context)
        {
            try
            {
                context.Value();
            }
            catch (Exception ex)
            {
                throw new VerificationException($"Exception of type '{ex.GetType()}' was thrown but none has been expected.", ex);
            }
        }
    }    
}
