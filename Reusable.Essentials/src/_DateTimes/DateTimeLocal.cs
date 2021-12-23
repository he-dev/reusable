using System;

namespace Reusable.Essentials;

public class DateTimeLocal : IDateTime
{
    public DateTime Now() => DateTime.Now;
}