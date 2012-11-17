using System;
using System.Collections.Generic;
using System.Linq;

namespace FunksjonellFagdag
{
    class Listekvalifiseringer
    {
        private static IEnumerable<long> NaturligeTall()
        {
            var i = default(long);
            while (true)
            {
                yield return ++i;
            }
        } 

        public static void Run()
        {
            var s = from n in NaturligeTall()
                    where n * n < 100
                    select 2*n;

            var s2 = NaturligeTall().Where(n => n*n < 100)
                                    .Select(n => n*2);

            foreach (var l in s)
            {
                Console.WriteLine(l);
            }
            Console.ReadKey();
        }
    }
}
