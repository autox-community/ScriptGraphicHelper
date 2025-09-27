using System.Runtime.Loader;

namespace ScriptGraphicHelper.Utils.Engine
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
