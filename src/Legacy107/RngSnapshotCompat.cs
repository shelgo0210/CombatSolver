// Snapshot bridge for v0.107.1, retaining the exact xoshiro generator words.
// The later game's SerializableRng type and Rng.NextUnsignedLong API did not exist yet.
namespace MegaCrit.Sts2.Core.Saves
{
    internal sealed record SerializableRng
    {
        public uint seed { get; init; }
        public int counter { get; init; }
        public ulong state0 { get; init; }
        public ulong state1 { get; init; }
        public ulong state2 { get; init; }
        public ulong state3 { get; init; }
    }
}

namespace MegaCrit.Sts2.Core.Random
{
    using MegaCrit.Sts2.Core.Saves;
    internal static class LegacyRngSerialization
    {
        internal static SerializableRng ToSerializable(this Rng rng)
            => new()
            {
                seed = rng.Seed,
                counter = rng.Counter,
                state0 = rng._random._s0,
                state1 = rng._random._s1,
                state2 = rng._random._s2,
                state3 = rng._random._s3
            };

        internal static void LoadFromSerializable(this Rng rng, SerializableRng state)
        {
            if (state.seed != 0 && state.seed != rng.Seed)
                throw new InvalidOperationException(
                    "Cannot assign a different seed to an existing v0.107.1 RNG; construct a new native stream.");
            rng.Counter = state.counter;
            rng._random._s0 = state.state0;
            rng._random._s1 = state.state1;
            rng._random._s2 = state.state2;
            rng._random._s3 = state.state3;
        }

        internal static ulong NextUnsignedLong(this Rng rng)
        {
            rng.Counter = checked(rng.Counter + 1);
            return rng._random.NextULong();
        }
    }
}
