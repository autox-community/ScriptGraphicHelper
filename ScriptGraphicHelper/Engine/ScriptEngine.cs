using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ScriptGraphicHelper.Engine
{
    public class ScriptEngine
    {
        private static List<MetadataReference> References;

        private readonly Domain alc;

        private WeakReference<Assembly> assembly;

        private readonly List<SyntaxTree> trees = new();

        public ScriptEngine()
        {
            this.alc = new Domain();

            if (References is null)
            {
                References = new List<MetadataReference>();
                try
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (!assembly.IsDynamic)
                        {
                            References.Add(MetadataReference.CreateFromFile(assembly.Location));
                        }
                    }

                }
                catch { }

            }
        }

        public void LoadScript(string path)
        {
            if (path.EndsWith(".cs") || path.EndsWith(".csx"))
            {
                this.trees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path, encoding: Encoding.UTF8));
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public void UnExecute()
        {
            this.alc.Unload();
        }

        public EmitResult Compile()
        {
            var assemblyName = Path.GetRandomFileName();
            var compilation = CSharpCompilation.Create(
                assemblyName,

                references: References,
                syntaxTrees: this.trees,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));


            using var dll = new MemoryStream();
            using var pdb = new MemoryStream();
            var result = compilation.Emit(dll, pdb);

            if (result.Success)
            {
                dll.Seek(0, SeekOrigin.Begin);
                pdb.Seek(0, SeekOrigin.Begin);
                this.assembly = new WeakReference<Assembly>(this.alc.LoadFromStream(dll, pdb));
            }

            return result;
        }

        public string? Execute(string entryType, string funcName, object[] args)
        {
            this.assembly.TryGetTarget(out var assembly);
            var type = assembly.GetType(entryType);

            var result = type.InvokeMember(funcName,
                BindingFlags.InvokeMethod |
                BindingFlags.Static |
                BindingFlags.Public,
                null, null,
                args
                ) as string;

            return result;
        }
    }
}