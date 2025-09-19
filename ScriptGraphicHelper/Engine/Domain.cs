using System.Runtime.Loader;

namespace ScriptGraphicHelper.Engine
{
    public class Domain : AssemblyLoadContext
    {

        public Domain() : base(true)
        {
        }

        //protected override Assembly? Load(AssemblyName assemblyName)
        //{

        //}
    }
}
