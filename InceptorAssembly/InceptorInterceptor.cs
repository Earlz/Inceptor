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
                if(Interceptors.ContainsKey(name))
                {
                    return Interceptors[name](targetThis, name);
                }
            }
            return null;
        }
        public static void bleh()
        {
            var tmp = Check(null, "foo");
            if(tmp != null)
            {
                return;
            }
            Console.WriteLine("meh");
            return;
        }
    }
}
