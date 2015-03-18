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
                    if (m.Name == ".ctor" || m.Name == "..ctor") continue;
                    if (m.ReturnType.IsValueType) continue;
                    //if (m.ReturnType.FullName != "System.Void") continue;
                    var first = m.Body.Instructions[0];
                    var inject=new List<Instruction>();
                    if (m.IsStatic)
                    {
                       inject.Add( (Instruction.Create(OpCodes.Ldnull)));
                    }
                    else
                    {
                       inject.Add( (Instruction.Create(OpCodes.Ldarg_0)));
                    }
                    var name = MethodName(t, m);
                    sb.AppendLine(name);
                   inject.Add( (Instruction.Create(OpCodes.Ldstr, name)));
                    if(m.HasParameters)
                    {
                      //  m.Body.InitLocals = true;
                       // var paramsarr = new VariableDefinition(targetModule.TypeSystem.Object.Resolve());
                      //  m.Body.Variables.Add(paramsarr);

                        for(int i=0;i<m.Parameters.Count;i++)
                        {
                        //   inject.Add( Instruction.Create(OpCodes.Ldarg, i));
                        }
                        inject.Add(Instruction.Create(OpCodes.Ldnull));
                    }
                    else
                    {
                       inject.Add( (Instruction.Create(OpCodes.Ldnull)));
                    }

                   inject.Add( (Instruction.Create(OpCodes.Call, m.Module.Import(inceptor))));
                    if (m.ReturnType.FullName == "System.Void")
                    {
                       inject.Add( Instruction.Create(OpCodes.Ldnull));
                       inject.Add( Instruction.Create(OpCodes.Ceq));
                       inject.Add( Instruction.Create(OpCodes.Brtrue_S, first));
                       inject.Add( Instruction.Create(OpCodes.Ret));
                    }
                    else
                    {
                        m.Body.InitLocals = true;
                        //int local = m.Body.Variables.Count();
                        var local = new VariableDefinition(m.ReturnType);
                        m.Body.Variables.Add(local);
                        inject.Add( Instruction.Create(OpCodes.Castclass, m.ReturnType));
                       // inject.Add(Instruction.Create(OpCodes.Unbox_Any, m.ReturnType));
                        inject.Add( Instruction.Create(OpCodes.Stloc, local));
                        inject.Add(Instruction.Create(OpCodes.Ldloc, local));
                      //  inject.Add(Instruction.Create(OpCodes.Box, m.ReturnType));
                        inject.Add( Instruction.Create(OpCodes.Ldnull));
                        inject.Add( Instruction.Create(OpCodes.Ceq));
                        inject.Add( Instruction.Create(OpCodes.Brtrue_S, first));
                        inject.Add( Instruction.Create(OpCodes.Ldloc, local));
                        inject.Add( Instruction.Create(OpCodes.Ret));
                    }
                    inject.Reverse(); //this is mutable, btw
                    foreach(var instr in inject)
                    {
                        m.Body.Instructions.Insert(0, instr);
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
