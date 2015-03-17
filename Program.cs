using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Earlz.Inceptor
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Count() != 3)
            {
                Console.WriteLine("usage: target.dll inceptor.dll saveto.dll");
            }
            var targetModule = ModuleDefinition.ReadModule(args[0]);
            var inceptorModule = ModuleDefinition.ReadModule(args[1]);

            var inceptorType = inceptorModule.Types.First(x => x.FullName == "Earlz.InceptorAssembly.InceptorInterceptor");
            var inceptor = inceptorType.Methods.First(x => x.Name == "Check");

            
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
                    m.Body.Instructions.Insert(0, (Instruction.Create(OpCodes.Ldstr, t.FullName+"::"+m.Name)));
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
        }
    }
}
