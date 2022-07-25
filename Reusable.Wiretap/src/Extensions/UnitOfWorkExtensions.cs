using System;
using Reusable.Essentials;
using Reusable.Wiretap.Middleware;

namespace Reusable.Wiretap.Extensions;

public static class UnitOfWorkExtensions
{
    // public static UnitOfWork.Item SetCustomId(this UnitOfWork.Item unitOfWork, object value)
    // {
    //     return unitOfWork.Also(x => x.Correlation().Id = value);
    // }

    public static UnitOfWork.Item UseLayer(this UnitOfWork.Item unitOfWork, UnitOfWork.LayerFunc layer)
    {
        return unitOfWork.Also(x => x.Layer = layer);
    }
    
    public static UnitOfWork.Item SetException(this UnitOfWork.Item unitOfWork, Exception exception)
    {
        return unitOfWork.Also(x => x.Exception = exception);
    }
    
    public static UnitOfWork.Item EnableBuffer(this UnitOfWork.Item unitOfWork)
    {
        return unitOfWork.Also(x => x.Node<UnitOfWorkBuffer>().Enabled = true);
    }
    
    public static UnitOfWorkCorrelation Correlation(this UnitOfWork.Item unitOfWork)
    {
        return unitOfWork.Node<UnitOfWorkCorrelation>();
    }

    public static UnitOfWorkBuffer Buffer(this UnitOfWork.Item unitOfWork)
    {
        return unitOfWork.Node<UnitOfWorkBuffer>();
    }

    /// <summary>
    /// Gets the stopwatch in current scope.
    /// </summary>
    public static UnitOfWorkElapsed Stopwatch(this UnitOfWork.Item unitOfWork)
    {
        return unitOfWork.Node<UnitOfWorkElapsed>();
    }
}