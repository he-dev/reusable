using System;
using System.Collections.Generic;
using System.Text;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    [UseType, UseMember]
    [TrimStart("I"), TrimEnd("Properties")]
    [PlainSelectorFormatter]
    public interface IResourceProperties
    {
        UriString Uri { get; }
        
        bool Exists { get; }
        
        long Length { get; }
        
        DateTime CreateOn { get; }
        
        DateTime ModifiedOn { get; }
        
        MimeType Format { get; }

        Type DataType { get; }

        Encoding Encoding { get; }
        
        string ActualName { get; }
    }
}