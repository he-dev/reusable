namespace Reusable
{
    public abstract class TypeConverter
    {
        public static TypeConverter Empty => new CompositeConverter();

        public abstract bool TryConvert(ConversionContext context, object arg, out object instance);
    }

    public abstract class SpecificConverter<TArg, TResult> : TypeConverter
    {
        public override bool TryConvert(ConversionContext context, object arg, out object instance)
        {
            //// Type conversion is not necessary.
            //if (arg.GetType() == context.Type)
            //{
            //    instance = arg;
            //    return true;
            //}

            instance = default(TArg);

            var canConvert = context.Type.IsAssignableFrom(typeof(TResult)) && arg is TArg;
            if (!canConvert)
            {
                return false;
            }
            instance = Convert((TArg)arg, context);
            return true;
        }

        public abstract TResult Convert(TArg value, ConversionContext context);
    }

    public abstract class GenericConverter<TArg> : TypeConverter
    {
        public abstract object Convert(TArg value, ConversionContext context);
    }
}
