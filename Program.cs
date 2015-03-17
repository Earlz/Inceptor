using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Earlz.Inceptor
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Count() != 4)
            {
                Console.WriteLine("usage: target.dll inceptor.dll saveto.dll methodnames.txt");
            }
            var targetModule = ModuleDefinition.ReadModule(args[0]);
            var inceptorModule = ModuleDefinition.ReadModule(args[1]);

            var inceptorType = inceptorModule.Types.First(x => x.FullName == "Earlz.InceptorAssembly.InceptorInterceptor");
            var inceptor = inceptorType.Methods.First(x => x.Name == "Check");

            StringBuilder sb = new StringBuilder();
            foreach(var t in targetModule.Types)
            {
                foreach(var m in t.Methods)
                {
                    //note: it's all backwards ugh
                   // var il = m.Body.GetILProcessor();
                   // il.
                    if (!m.HasBody) continue;
                    if (t.IsValueType) continue; //don't try to handle structs yet
                    int after=10;
                    var first = m.Body.Instructions[0];
                    if (m.ReturnType.FullName == "System.Void")
                    {
                        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ret));
                        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Brtrue_S, first));
                        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ceq));
                        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldnull));
                    }
                    else
                    {
                        m.Body.InitLocals = true;
                        //int local = m.Body.Variables.Count();
                        var local = new VariableDefinition(m.ReturnType);
                        m.Body.Variables.Add(local);
                        
                        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ret));
                        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldloc, local));
                        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Brtrue_S, first));
                        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ceq));
                        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldnull));
                        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldloc, local));
                        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Stloc, local));
                        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Castclass, m.ReturnType));
                    }
                    m.Body.Instructions.Insert(0, (Instruction.Create(OpCodes.Call, m.Module.Import(inceptor))));
                    var name = MethodName(t, m);
                    sb.AppendLine(name);
                    if(m.HasParameters)
                    {
                        for(int i=0;i<m.Parameters.Count;i++)
                        {
                            m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldarg, i));
                        }
                    }
                    else
                    {
                        m.Body.Instructions.Insert(0, (Instruction.Create(OpCodes.Ldnull)));
                    }
                    m.Body.Instructions.Insert(0, (Instruction.Create(OpCodes.Ldstr, name)));
                    if (m.IsStatic)
                    {
                        m.Body.Instructions.Insert(0, (Instruction.Create(OpCodes.Ldnull)));
                    }
                    else
                    {
                        m.Body.Instructions.Insert(0, (Instruction.Create(OpCodes.Ldarg_0)));
                    }
                }
            }

            targetModule.Write(args[2]);
            File.WriteAllText(args[3], sb.ToString());
        }
        static string MethodName(TypeDefinition t, MethodDefinition m)
        {
            return m.FullName;
            /*
            string s = m.ReturnType.FullName + " " + t.FullName + "::" + m.FullName + "(";
            if (m.HasParameters)
            {
                foreach (var p in m.Parameters)
                {
                    s += p.ParameterType.FullName + ", ";
                }
                s=s.Substring(0, s.Length - 2);
            }
            s += ")";
            return s;
          * */
        }
    }
}
