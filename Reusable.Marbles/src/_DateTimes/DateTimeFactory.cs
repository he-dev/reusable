﻿using System;

namespace Reusable.Marbles;

public class DateTimeFactory : IDateTime
{
    private readonly Func<DateTime> _now;

    public DateTimeFactory(Func<DateTime> now) => _now = now;

    public DateTime Now() => _now();
}