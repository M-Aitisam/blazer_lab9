using Fluxor;

namespace blazer_lab9.Data
{
    public record CounterState(int Count);

    public class CounterFeature : Feature<CounterState>
    {
        public override string GetName() => "Counter";

        protected override CounterState GetInitialState() =>
            new CounterState(Count: 0);
    }
}
