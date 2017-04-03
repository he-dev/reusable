using System;

namespace Reusable.Fuse.Testing
{
    public static class Verifier
    {
        public static ISpecificationContext<Action> Verify(Action action)
        {
            return new SpecificationContext<Action>(action, string.Empty, typeof(VerificationException));
        }

        public static TExpectedException Throws<TExpectedException>(this ISpecificationContext<Action> specificationContext) where TExpectedException : Exception
        {
            if (specificationContext == null) throw new ArgumentNullException(nameof(specificationContext));
            try
            {
                specificationContext.Value();
                throw new VerificationException($"Exception of type '{typeof(TExpectedException)}' was expected but none has been thrown.");
            }
            catch (TExpectedException ex)
            {
                return ex;
            }
            catch (Exception ex)
            {
                throw new VerificationException($"Exception of type '{typeof(TExpectedException)}' was expected but '{ex.GetType()}' has been thrown.", ex);
            }
        }

        //public static void DoesNotThrow(this ISpecificationContext<Action> specificationContext)
        //{
        //    if (specificationContext == null) throw new ArgumentNullException(nameof(specificationContext));
        //    try
        //    {
        //        specificationContext.Value();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new VerificationException($"Exception of type '{ex.GetType()}' was thrown but none has been expected.", ex);
        //    }
        //}
    }
}
