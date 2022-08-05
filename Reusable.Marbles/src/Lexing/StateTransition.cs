using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Reusable.Marbles.Lexing;

public class StateTransition<TToken> where TToken : Enum
{
    private readonly IImmutableDictionary<TToken, IImmutableList<State<TToken>>> _transitions;

    public StateTransition(IImmutableDictionary<TToken, IImmutableList<State<TToken>>> transitions) => _transitions = transitions;

    public IEnumerable<State<TToken>> Next(TToken token) => _transitions[token];
}