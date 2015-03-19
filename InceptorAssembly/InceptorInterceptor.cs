using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Earlz.InceptorAssembly
{

    public static class InceptorInterceptor
    {
        static InceptorInterceptor()
        {
            Interceptors = new Dictionary<string, Interceptor>();
        }
        public delegate object Interceptor(object targetThis, string name, object[] parameters);
        static Dictionary<string, Interceptor> Interceptors;
        static string locked = "";
        public static object Check(object targetThis, string name, object[] parameters) //, params object[] values)
        {
            // lock(locked)
            {
                if (Interceptors.ContainsKey(name))
                {
                    return Interceptors[name](targetThis, name, parameters);
                }
            }
            return null;
        }
        public static object[] bleh(ref int foo, params object[] p)
        {
            var arr=new object[2];
            arr[0]=foo;
            arr[1]=10;
            foo = 20;
            var tmp = (object[])Check(null, "foo", null);
            if (tmp != null)
            {
                return tmp;
            }
            Console.WriteLine("meh");
            return p;
        }
        public static void meh()
        {
            int tmp = 0;
            bleh(ref tmp, 10, 15, 50, 20);
        }
        public static object Coerce<T>(T anything)
        {
            return (object)anything;
        }
        public struct FooStruct
        {
            public int biz;
            public int baz;
        }
        public static void testit(ref FooStruct test, ref string me, ref bool ugh)
        {
            int foo=10;
            string blah="";
            var store=new object[10];
           // store[0] = test;
            store[0]=Coerce(test);
            //store[0]=Coerce(meh);
            store[0]=Coerce(blah);
            ugh = (bool)store[0];
        }
    }
}
