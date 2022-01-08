using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;

namespace Reusable.Csv
{
    public static class CsvReaderExtensions
    {
        [NotNull, ItemNotNull, ContractAnnotation("csvReader: null => halt")]
        public static IEnumerable<IList<string>> AsEnumerable(this ICsvReader csvReader)
        {
            if (csvReader == null) throw new ArgumentNullException(nameof(csvReader));

            return 
                enumerable
                    .Repeat(async () => await csvReader.ReadLineAsync())
                    .TakeWhile(line => line.Result != null)
                    .Select(line => line.Result);
        }        
    }
}