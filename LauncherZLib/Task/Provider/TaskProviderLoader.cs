using System;
using System.Linq;
using System.Reflection;

namespace LauncherZLib.Task.Provider
{
    public class TaskProviderLoader
    {
        public void LoadAll()
        {
            string[] types;
            string requesting =
               @"D:\Documents\Visual Studio 2013\Projects\LauncherZ\LauncherZ\bin\Debug\Providers\TestProvider.dll";
            var asmName = AssemblyName.GetAssemblyName(requesting);
            var domain = AppDomain.CreateDomain("LauncherZProviders");
            Type loaderType = typeof (LoaderProxy);
            var loader =
                domain.CreateInstanceFrom(loaderType.Assembly.Location, loaderType.FullName).Unwrap() as LoaderProxy;
            if (loader != null)
            {
                domain.ReflectionOnlyAssemblyResolve += loader.OnReflectionOnlyAssemblyResolve;
                types = loader.GetTypes(requesting);
            }
            AppDomain.Unload(domain);
            
        }
    }

    class LoaderProxy : MarshalByRefObject
    {

        public Assembly OnReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs e)
        {
            var domain = AppDomain.CurrentDomain;
            Assembly asm = Assembly.ReflectionOnlyLoad(e.Name);
            return asm;
        }


        public string[] GetTypes(string assemblyPath)
        {
            Assembly asm = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
            if (asm != null)
            {
                return asm.GetExportedTypes().Select(type => type.FullName).ToArray();
            }
            return null;
        }



    }

}
