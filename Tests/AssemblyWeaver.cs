using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;

public class AssemblyWeaver
{
    public Assembly Assembly;
    public string AfterAssemblyPath;
    public AssemblyWeaver(string assemblyPath, List<string> referenceAssemblyPaths = null)
    {

        if (referenceAssemblyPaths == null)
        {
            referenceAssemblyPaths = new List<string>();
        }
        assemblyPath = FixAssemblyPath(assemblyPath);

        AfterAssemblyPath = assemblyPath.Replace(".dll", "2.dll");
        File.Copy(assemblyPath, AfterAssemblyPath, true);
        var oldPdb = Path.ChangeExtension(assemblyPath, "pdb");
        var newPdb = Path.ChangeExtension(AfterAssemblyPath, "pdb");
        File.Copy(oldPdb, newPdb, true);

        var assemblyResolver = new MockAssemblyResolver();
        foreach (var referenceAssemblyPath in referenceAssemblyPaths)
        {
            var directoryName = Path.GetDirectoryName(referenceAssemblyPath);
            assemblyResolver.AddSearchDirectory(directoryName);
        }
        var readerParameters = new ReaderParameters
        {
            AssemblyResolver = assemblyResolver,
            ReadSymbols = true,
        };
        var moduleDefinition = ModuleDefinition.ReadModule(AfterAssemblyPath, readerParameters);
        var weavingTask = new ModuleWeaver
        {
            ModuleDefinition = moduleDefinition,
            AssemblyResolver = assemblyResolver,
            LogError = LogError,
            ReferenceCopyLocalPaths = referenceAssemblyPaths
        };

        weavingTask.Execute();
        moduleDefinition.Write(AfterAssemblyPath);

        Assembly = Assembly.LoadFrom(AfterAssemblyPath);
    }

    public static string FixAssemblyPath(string assemblyPath)
    {
#if (!DEBUG)
        assemblyPath = assemblyPath.Replace("Debug", "Release");
#endif
        return assemblyPath;
    }

    void LogError(string error)
    {
        Errors.Add(error);
    }

    public List<string> Errors = new List<string>();
}