using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Reusable.Lexing
{
    public class StateTransitionBuilder<TToken> : List<State<TToken>> where TToken : Enum
    {
        public void Add(TToken token, params TToken[] next) => Add(new State<TToken>(token, next));

        public StateTransition<TToken> Build()
        {
            var transitions = this.Aggregate(ImmutableDictionary<TToken, IImmutableList<State<TToken>>>.Empty, (mappings, state) =>
            {
                var nextStates =
                    from n in state.Next
                    join s in this on n equals s.Token
                    select s;

                return mappings.Add(state.Token, nextStates.ToImmutableList());
            });

            return new StateTransition<TToken>(transitions);
        }

        public static implicit operator StateTransition<TToken>(StateTransitionBuilder<TToken> builder) => builder.Build();
    }
}