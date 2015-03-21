using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Earlz.InceptorAssembly
{

    public static class InceptorInterceptor
    {
        static InceptorInterceptor()
        {
            Interceptors = new Dictionary<string, Interceptor>();
        }
        public delegate object Interceptor(object targetThis, string name, object[] parameters);
        static volatile Dictionary<string, Interceptor> Interceptors;
        static string locked = "";
        public static object Check(object targetThis, string name, object[] parameters) 
        {
            // store pointer here because Interceptors can be replaced at any time
            var tmp = Interceptors;
            if (tmp.ContainsKey(name))
            {
                return tmp[name](targetThis, name, parameters);
            }
            return null;
        }
        static string ModifyLock = "";
        public static void Add(string name, Interceptor func)
        {
            lock (ModifyLock)
            {
                var d = new Dictionary<string, Interceptor>(Interceptors);
                d.Add(name, func);
                Interceptors = d; //assign here so it's lock-free and thread-safe.
            }
        }
        public static void Remove(string name)
        {
            lock(ModifyLock)
            {
                var d = new Dictionary<string, Interceptor>(Interceptors);
                if (d.ContainsKey(name))
                {
                    d.Remove(name);
                }
                Interceptors = d; //assign here so it's lock-free and thread-safe.
            }
        }
    }
}
