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
                    if(m.ReturnType.IsGenericInstance)
                    {

                    }
                    var reloadParams = new List<Instruction>();
                    if (m.ReturnType.IsValueType || m.ReturnType.HasGenericParameters || m.ReturnType.IsGenericInstance || m.ReturnType.IsGenericParameter) continue; //ugh don't handle boxing shit yet
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
                        m.Body.InitLocals = true;
                        var objtype = m.Module.Import(targetModule.TypeSystem.Object.Resolve());
                        var paramsarr = new VariableDefinition(new ArrayType(objtype));
                        m.Body.Variables.Add(paramsarr);
                        inject.Add(Instruction.Create(OpCodes.Ldc_I4, m.Parameters.Count));
                        inject.Add(Instruction.Create(OpCodes.Newarr, objtype));
                        inject.Add(Instruction.Create(OpCodes.Dup));
                        inject.Add(Instruction.Create(OpCodes.Stloc, paramsarr));
                        for(int i=0;i<m.Parameters.Count;i++)
                        {
                            inject.Add(Instruction.Create(OpCodes.Dup));
                            var p=m.Parameters[i];
                            inject.Add(Instruction.Create(OpCodes.Ldc_I4, i));
                            if(p.ParameterType.FullName=="System.Boolean&")
                            {

                            }
                            if (p.ParameterType.IsGenericInstance || p.ParameterType.IsGenericParameter || p.ParameterType.IsPointer)
                            {
                                inject.Add(Instruction.Create(OpCodes.Ldnull));
                            }
                            else
                            {
                                if (p.ParameterType.IsByReference)
                                {


                                    inject.Add(Instruction.Create(OpCodes.Ldarg, p));
                                    inject.Add(Instruction.Create(OpCodes.Ldobj, p.ParameterType.GetElementType()));

                                    if (p.ParameterType.GetElementType().IsValueType)
                                    {
                                    reloadParams.Add(Instruction.Create(OpCodes.Ldarg, p));
                                    reloadParams.Add(Instruction.Create(OpCodes.Ldloc, paramsarr));
                                    reloadParams.Add(Instruction.Create(OpCodes.Ldc_I4, i));
                                        reloadParams.Add(Instruction.Create(OpCodes.Ldelema, new ByReferenceType(objtype)));
                                        //reloadParams.Add(Instruction.Create(OpCodes.Unbox, p.ParameterType.GetElementType()));
                                        reloadParams.Add(Instruction.Create(OpCodes.Ldobj, p.ParameterType.GetElementType()));
                                        reloadParams.Add(Instruction.Create(OpCodes.Stobj, p.ParameterType.GetElementType()));

                                        inject.Add(Instruction.Create(OpCodes.Box, p.ParameterType.GetElementType()));
                                    }
                                    else
                                    {
                                    reloadParams.Add(Instruction.Create(OpCodes.Ldarg, p));
                                    reloadParams.Add(Instruction.Create(OpCodes.Ldloc, paramsarr));
                                    reloadParams.Add(Instruction.Create(OpCodes.Ldc_I4, i));
                                        reloadParams.Add(Instruction.Create(OpCodes.Ldelem_Ref));
                                        reloadParams.Add(Instruction.Create(OpCodes.Stobj, p.ParameterType.GetElementType()));
                                        //reloadParams.Add(Instruction.Create(OpCodes.Starg, p));
                                    }
                                }
                                else
                                {
                                    reloadParams.Add(Instruction.Create(OpCodes.Ldloc, paramsarr));
                                    reloadParams.Add(Instruction.Create(OpCodes.Ldc_I4, i));

                                    inject.Add(Instruction.Create(OpCodes.Ldarg, p));
                                    if (p.ParameterType.IsValueType)
                                    {
                                        reloadParams.Add(Instruction.Create(OpCodes.Ldelem_Ref));
                                        reloadParams.Add(Instruction.Create(OpCodes.Unbox, p.ParameterType));
                                        reloadParams.Add(Instruction.Create(OpCodes.Ldobj, p.ParameterType.GetElementType()));
                                        reloadParams.Add(Instruction.Create(OpCodes.Starg, p));

                                        inject.Add(Instruction.Create(OpCodes.Box, p.ParameterType));
                                    }
                                    else
                                    {
                                        reloadParams.Add(Instruction.Create(OpCodes.Ldelem_Ref));
                                        reloadParams.Add(Instruction.Create(OpCodes.Castclass, p.ParameterType));
                                        reloadParams.Add(Instruction.Create(OpCodes.Starg, p));
                                    }
                                } 
                            }
                            inject.Add(Instruction.Create(OpCodes.Stelem_Ref));

                        }
                    }
                    else
                    {
                       inject.Add( (Instruction.Create(OpCodes.Ldnull)));
                    }

                   inject.Add( (Instruction.Create(OpCodes.Call, m.Module.Import(inceptor))));
                    if (m.ReturnType.FullName == "System.Void")
                    {

                        m.Body.InitLocals = true;
                        var local = new VariableDefinition(m.Module.Import(targetModule.TypeSystem.Object.Resolve())); //just use object
                        m.Body.Variables.Add(local);
                        inject.Add( Instruction.Create(OpCodes.Stloc, local));

                        foreach(var instr in reloadParams)
                        {
                            inject.Add(instr);
                        }
                        inject.Add( Instruction.Create(OpCodes.Ldloc, local));
                       inject.Add( Instruction.Create(OpCodes.Ldnull));
                       inject.Add( Instruction.Create(OpCodes.Ceq));
                       inject.Add( Instruction.Create(OpCodes.Brtrue_S, first));
                       inject.Add( Instruction.Create(OpCodes.Ret));
                    }
                    else
                    {
                        m.Body.InitLocals = true;
                        var local = new VariableDefinition(m.ReturnType);
                        m.Body.Variables.Add(local);
                        inject.Add( Instruction.Create(OpCodes.Castclass, m.ReturnType));
                        inject.Add( Instruction.Create(OpCodes.Stloc, local));
                        
                        foreach(var instr in reloadParams)
                        {
                            inject.Add(instr);
                        }

                        inject.Add(Instruction.Create(OpCodes.Ldloc, local));
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
