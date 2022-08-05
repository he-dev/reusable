using System;

namespace Reusable.Marbles;

public class DateTimeLocal : IDateTime
{
    public DateTime Now() => DateTime.Now;
}