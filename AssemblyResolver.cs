using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO; 
using System.Windows.Forms;
using System.Globalization;

namespace BeeSys.Wasp3D.Utility
{
    public static class CAssemblyResolver
    {
        static List<String> _addInLoadPaths;

        static CAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            _addInLoadPaths = new List<string>();
        }
        public static bool ResolveFormDlls { get; set; }
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                string assemblyFileName = args.Name;
                if (assemblyFileName.EndsWith(".resources"))
                {                   
                    assemblyFileName = assemblyFileName.Substring(0, assemblyFileName.Length - ".resources".Length);                  
                }
                AssemblyName name = new AssemblyName(assemblyFileName);
                string assemblyName = name.Name + ".dll";
                lock (_addInLoadPaths)
                {
                    for (int pathCount = 0; pathCount < _addInLoadPaths.Count; pathCount++)
                    {
                        if (File.Exists(Path.Combine(_addInLoadPaths[pathCount], assemblyName)))
                        {
                            return Assembly.LoadFrom(Path.Combine(_addInLoadPaths[pathCount], assemblyName));
                        }
                    }
                }
                if (ResolveFormDlls)
                {
                    Assembly assembly = null;
                    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (asm.FullName == args.Name)
                        {
                            assembly = asm;
                        }
                    }
                    return assembly;
                }

            }
            catch (Exception ex)
            {
                //EventLogger.WriteLog(ModuleNameEnum.SharedLibraryAssemblyLoader, ex, args.Name);
            }
            return null;
        }


        public static void AddDefaultPath()
        {
            string sCommonPath = Environment.GetEnvironmentVariable("WaspCommon", EnvironmentVariableTarget.Machine);

            var STARTPATH = sCommonPath + "{0}Shared Resources";


            var _64BITPATH = sCommonPath + "{0}Shared Resources{0}X64";

            var _86BITPATH = sCommonPath + "{0}Shared Resources{0}x86";

            if (Environment.Is64BitProcess)
            {

                //RESOLVE SHARED RESOURCE -> 64 BIT DLL
                CAssemblyResolver.AddPath
                                       (
                                           Path.Combine
                                                       (
                                                           Application.StartupPath,
                                                           String.Format(CultureInfo.InvariantCulture, _64BITPATH, Path.DirectorySeparatorChar)
                                                       )
                                       );
            }
            else
            {

                //RESOLVE SHARED RESOURCE -> 64 BIT DLL
                CAssemblyResolver.AddPath
                                       (
                                           Path.Combine
                                                       (
                                                           Application.StartupPath,
                                                           String.Format(CultureInfo.InvariantCulture, _86BITPATH, Path.DirectorySeparatorChar)
                                                       )
                                       );
            }

            //RESOLVE SHARED RESOURCE

            CAssemblyResolver.AddPath
                                    (
                                        Path.Combine
                                                    (
                                                        Application.StartupPath,
                                                        String.Format(CultureInfo.InvariantCulture, STARTPATH, Path.DirectorySeparatorChar)
                                                    )
                                    );


        }

        public static void AddPath(string basePath)
        {
            try
            {

                lock (_addInLoadPaths)
                {

                    if (!_addInLoadPaths.Contains(basePath.ToLowerInvariant()))
                        _addInLoadPaths.Add(basePath.ToLowerInvariant());
                }
            }
            catch (Exception ex)
            {
                //EventLogger.WriteLog(ModuleNameEnum.SharedLibraryAssemblyLoader, ex, basePath);
            }
        }

        public static void RemovePath(string basePath)
        {
            try
            {
                lock (_addInLoadPaths)
                {
                    basePath = basePath.ToLowerInvariant();
                    if (_addInLoadPaths.Contains(basePath))
                        _addInLoadPaths.Remove(basePath);
                }
            }
            catch (Exception ex)
            {
                //EventLogger.WriteLog(ModuleNameEnum.SharedLibraryAssemblyLoader, ex, basePath);
            }
        }
    }
}
