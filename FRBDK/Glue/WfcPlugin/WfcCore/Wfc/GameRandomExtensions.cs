using FlatRedBall.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WfcCore.Wfc
{
    public static class GameRandomExtensions
    {
        // https://gamedev.stackexchange.com/a/162994
        public static T InWeighted<T>(this GameRandom random, IEnumerable<T> list, Func<T, double> weight)
            where T : class
        {
            var total = list.Sum(weight);
            var rand = random.NextDouble() * total;

            return list.FirstOrDefault(i =>
            {
                total -= weight(i);
                return rand > total;
            });
        }
    }
}
