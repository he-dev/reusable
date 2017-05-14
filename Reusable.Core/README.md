# Reusable.Core

This package contains the following utilities:

  - `Reusable`
    - `IDateTimeService` - The base interace for date-time services.
    - `Result` - Can be used to wrap the result value.
    - `Try` - Can be used to execute actions and return a `Result`.
  - `Resuable.Collections` - Custom collections.
    - `AutoKeyDictionary` - A dictionary that takes a delegate for the key that is a part of the value.
    - `EnumerableExtensions`
    - `ProjectionComparer` - A comparer for projection objects.
  - `Resuable.Commands`
    - `LinkCommand` - A command that links two other commands to execute them before and/or after the main command.
    - `LInkCommandComposer` - A composer for linked commands.
  - `Resuable.Data`
    - `AppConfigRepository` - A helper repository for reading the app.config.
    - `DataTableExtensions` - Helper extensions for adding columns and rows to datatables.
  - `Reusable.Data.SqlClient` - Sql Server reflection.
    - `SchemaInfo`
    - `ColumnInfo`
    - `TableInfo`
    - `SqlServer` - A service that provides Sql Server reflection methods.
    - `SqlConnectionExtensions` - Sql connection extensions for Sql Server reflection.

  - `Reusable.DateTimeServices`
    - `FixedDateTimeService` - Date and time for testing.
    - `LocalDateTimeService` - System date and time (local).
    - `UtcDateTimeService` - System date and time ().    
  
  - `Resuable.Data.Annotations`
    - `FormatAttribute` - Allows to specify the format for a property.
    - `IgnoreAttribute` - Allows to ignore a member.
  - `Resuable.Drawing`
    - `Color32` - A serializable color structure.
    - `ColorExtensions`
    - `ColorParser`
  - `Reusable.Extensions`
    - `Conditional` - Conditional extensions for common conditions.
    - `ExceptionExtensions`
    - `StringExtensions`
    - `StringInterpolation` - A utility for replacing named placeholders in strings.
    - `StringPrettifier` - A utility that creates pretty strings.
    - `TypeExtensions`
  - `Resuable.Sequences`  - Utilities for creating sequences.
    - `FibonacciSequence`
    - `FibonacciSequenceFactory`
    - `GeneratedSequence`  
    - `GeometricSequence`
    - `HarmonicSequence`
    - `LinearSequence`
    - `RegularSequence`
  - `Reusable.StringFormatting`
    - `CustomFormatter`
    - `CustomFormatterComposition`
  - `Reusable.StringFormatting.Formatters` - Custom string formatters.
    - `BracketFormatter`
    - `CaseFormatter`
    - `DecimalColorFormatter`
    - `HexadecimalColorFormatter`
    - `QuoteFormatter`
  - `Reusable.ComparisonResult` - Defines constants for common comparison results.
  - `Resuable.Reflection`
  - `Reusable.ResourceReader` - A utility for reading resources.
  - `Reusable.Usingifier` - A utility for making things _disposable_.
  - `Reusable.Validator`

---

Icon made by [Roundicons](http://www.flaticon.com/authors/roundicons) from www.flaticon.com is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a></div>

Icon made by [Vectors Market](http://www.flaticon.com/authors/vectors-market) from www.flaticon.com is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a></div>

