using System;

namespace FunksjonellFagdag
{
    public class Maybe<T>
    {
        private Maybe(T value)
        {
            HasValue = true;
            Value = value;
        }

        private Maybe()
        {
            HasValue = false;
        }

        public static Maybe<T> Nothing()
        {
            return new Maybe<T>();
        }

        // A.k.a. return
        // A.k.a. unit function
        public static Maybe<T> Just(T value)
        {
            return new Maybe<T>(value);
        }

        public T Value { get; private set; }
        public bool HasValue { get; private set; }

        public void Do(Action<T> action)
        {
            if (HasValue) action(Value);
        }
    }

    public static class MaybeExtensions
    {
        public static Maybe<T> ToMaybe<T>(this T value)
        {
            return Maybe<T>.Just(value);
        }

        public static Maybe<T2> Bind<T1, T2>(this Maybe<T1> a, Func<T1, Maybe<T2>> func)
        {
            return func(a.Value);
        }

        public static Maybe<T3> SelectMany<T1, T2, T3>(this Maybe<T1> a, Func<T1, Maybe<T2>> func, Func<T1, T2, T3> select)
        {
            return a.Bind(aval => func(aval).Bind(bval => select(aval, bval).ToMaybe()));
        }
    }
    public class ErrorProne<T>
    {
        private ErrorProne(Exception e)
        {
            IsError = true;
            Exception = e;
        }

        public static ErrorProne<T> Error(Exception e)
        {
            return new ErrorProne<T>(e);
        }

        public bool IsError { get; private set; }
        public Exception Exception { get; private set; }

        public void Do(Action<T> action)
        {
            if (!IsError) action(Value);
        }


        private ErrorProne(T value)
        {
            Value = value;
        }

        // A.k.a. return
        // A.k.a. unit function
        public static ErrorProne<T> Just(T value)
        {
            return new ErrorProne<T>(value);
        }

        public T Value { get; private set; }
    }

    public static class IdentityExtensions
    {
        public static ErrorProne<T> ToErrorProne<T>(this T value)
        {
            return ErrorProne<T>.Just(value);
        }

        public static ErrorProne<T2> Bind<T1, T2>(this ErrorProne<T1> a, Func<T1, ErrorProne<T2>> func)
        {
            if (a.IsError) return ErrorProne<T2>.Error(a.Exception);
            try
            {
                return func(a.Value);
            }
            catch (Exception e)
            {
                return ErrorProne<T2>.Error(e);
            }
        }

        public static ErrorProne<T3> SelectMany<T1, T2, T3>(this ErrorProne<T1> a, Func<T1, ErrorProne<T2>> func, Func<T1, T2, T3> select)
        {
            return a.Bind(aval => func(aval).Bind(bval => select(aval, bval).ToErrorProne()));
        }
    }

    class Funksjonssammensetning
    {
        public static void Run()
        {
            /* OPPRINNELIG - uten funksjonssammensetning
            Func<int, int> minusTwo = x => x - 2;
            Func<int, int> tenDivX = x => 10 / x;
            
            Func<int, int> formel = x => tenDivX(minusTwo(x));  
             */

            Func<int, ErrorProne<int>> minusTwo = x => (x - 2).ToErrorProne();
            Func<int, ErrorProne<int>> tenDivX = x => (10 / x).ToErrorProne();

            Func<int, ErrorProne<int>> formel =
                x => minusTwo(x).Bind(tenDivX);

            var uttrykk = from x in 2.ToErrorProne()
                          from y in (x - 2).ToErrorProne()
                          select 10/y;

            if (!uttrykk.IsError) Console.WriteLine(uttrykk.Value);
            Console.ReadKey();
        }
    }
}
