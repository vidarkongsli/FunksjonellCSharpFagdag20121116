using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace FunksjonellFagdag
{
    class Program
    {
        static void Main()
        {
            Run();
        }

        internal static void Run()
        {
            var ns = XNamespace.Get("http://data.stortinget.no");
            var stortingsPerioderUrl = new Uri(@"http://data.stortinget.no/eksport/stortingsperioder");
            const string stortingsPeriodeUrl = @"http://data.stortinget.no/eksport/representanter?stortingsperiodeid={0}";
            
            var stortingsperioder =
                from tekst in HentDataFraUrl(stortingsPerioderUrl)
                from xml in XDocument.Parse(tekst).Root.ToErrorProne()
                select (from p in xml.Descendants()
                        where p.Name == ns + "stortingsperiode"
                        select ToStortingsperiode(p, ns)).ToList();

            Console.WriteLine("Hvilken stortingsperiode ønsker du å søke i?");

            stortingsperioder.Do(sperioder => sperioder.Each( (i,sp) =>     
                Console.WriteLine("{0}\t{1:yyyy}-{2:yyyy}",
                    i + 1, sp.Start, sp.Slutt)
            ));
            
            var valgtPeriode = from i in BrukersPeriodeValg().Last()
                               from perioder in stortingsperioder.Value.ToMaybe()
                               select perioder.ElementAt(i - 1).Navn;

            valgtPeriode.Do(p =>Console.WriteLine("Du valgte perioden {0}", p));

            var xDoc = from p in valgtPeriode.ToErrorProne()
                       from tekst in HentDataFraUrl(
                           new Uri(string.Format(stortingsPeriodeUrl, p.Value)))
                       select XDocument.Parse(tekst);

            NyttSoek().Each(n00B => xDoc.Do(
                doc => FinnStortingsrepresentanter(doc, ns, SoekeKriterium,
                    SkrivUtStortingsrepresentanter)));

            // TODO : Feilhåndtering
        }

        private static IEnumerable<bool> NyttSoek()
        {
            yield return true;
            var finished = false;
            while (!finished)
            {
                Console.WriteLine("Nytt søk? (j/n)");
                var selection = 'q';
                while (selection != 'j' && selection != 'n')
                {
                    selection = Console.ReadKey(true).KeyChar;
                }
                finished = selection == 'n';
                if (!finished) yield return true;
            }
        }

        private static string SoekeKriterium()
        {
            Console.Write("Hvilket etternavn vil du søke etter? ");
            var navn = Console.ReadLine();
            return navn;
        }

        private static IEnumerable<Maybe<int>> BrukersPeriodeValg()
        {
            var finished = false;
            while (!finished)
            {
                var key = Console.ReadKey(true);
                int selection;
                if (int.TryParse(key.KeyChar.ToString(CultureInfo.InvariantCulture),
                                 out selection))
                {
                    finished = true;
                    yield return selection.ToMaybe();
                }
                else
                {
                    yield return Maybe<int>.Nothing();
                }
            }
        }

        private static void SkrivUtStortingsrepresentanter(List<Stortingsrepresentant> stortingsrepresentanter)
        {
            Console.WriteLine("Representanter funnet: {0}",
                              stortingsrepresentanter.Count);
            if (stortingsrepresentanter.Count > 0)
            {
                Console.WriteLine("-------------------------------------------------------------------------------");
                Console.WriteLine("Representant:".PadRight(30)
                                  + "Fylke:".PadRight(15)
                                  + "Parti:".PadRight(25));
                Console.WriteLine("-------------------------------------------------------------------------------");
                stortingsrepresentanter.Each(r =>
                                             Console.WriteLine((r.Etternavn + ", "
                                                                + r.Fornavn).PadRight(30)
                                                               + r.Fylke.PadRight(15)
                                                               + r.Parti.PadRight(25))
                    );
            }

            Console.WriteLine("-------------------------------------------------------------------------------");
        }

        private static List<Stortingsrepresentant> FinnStortingsrepresentanter(XDocument stortingsperiodeXDoc, 
            XNamespace ns, Func<string> navn,
            Action<List<Stortingsrepresentant>> thenDo )
        {
            var stortingsrepresentanter =
                (from n in new[]{navn()}
                 from sp in stortingsperiodeXDoc.Root.IfNotNull(r => r.Descendants(ns + "representant"))
                 where sp.Descendants(ns + "etternavn").First().Value.Contains(n)
                 select new Stortingsrepresentant
                            {
                                Fornavn = sp.Descendants(ns + "fornavn").First().Value,
                                Etternavn = sp.Descendants(ns + "etternavn").First().Value,
                                Fylke = sp.Descendants(ns + "fylke").First().Descendants(ns + "navn").First().Value,
                                Parti = sp.Descendants(ns + "parti").First().Descendants(ns + "navn").First().Value
                            }).ToList();
            thenDo(stortingsrepresentanter);
            return stortingsrepresentanter;
        }

        internal static ErrorProne<string> HentDataFraUrl(Uri uri)
        {
            var webClient = new WebClient { Encoding = new UTF8Encoding() };
            return webClient.DownloadString(uri).ToErrorProne();
        }

        internal static Stortingsperiode ToStortingsperiode(XContainer p, XNamespace ns)
        {
            return new Stortingsperiode
            {
                Id = p.Descendants().Where(d => d.Name == ns + "id").Select(d => d.Value).First(),
                Start = DateTime.Parse(p.Descendants().Where(d => d.Name == ns + "fra").Select(d => d.Value).First()),
                Slutt = DateTime.Parse(p.Descendants().Where(d => d.Name == ns + "til").Select(d => d.Value).First())
            };
        }

    }
    internal class Stortingsrepresentant
    {
        public string Fornavn { get; set; }
        public string Etternavn { get; set; }
        public string Fylke { get; set; }
        public string Parti { get; set; }
    }

    internal class Stortingsperiode
    {
        public string Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime Slutt { get; set; }
        public string Navn
        {
            get
            {
                return string.Format("{0:yyyy}-{1:yyyy}", Start, Slutt);
            }
        }
    }
}
