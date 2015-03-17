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
        public delegate object Interceptor(object targetThis, string name);
        static Dictionary<string, Interceptor> Interceptors;
        static string locked = "";
        public static object Check(object targetThis, string name) //, params object[] values)
        {
            // lock(locked)
            {
                if (Interceptors.ContainsKey(name))
                {
                    return Interceptors[name](targetThis, name);
                }
            }
            return null;
        }
        public static object[] bleh(ref int foo, params object[] p)
        {
            foo = 20;
            var tmp = Check(null, "foo");
            if (tmp != null)
            {
                return p;
            }
            Console.WriteLine("meh");
            return p;
        }
        public static void meh()
        {
            int tmp = 0;
            bleh(ref tmp, 10, 15, 50, 20);
        }
    }
}
