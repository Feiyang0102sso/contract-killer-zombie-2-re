// Follows Unity 3.5.7's UnityEditor.MonoAOTRegistration.WriteCPlusPlusFileForStaticAOTModuleRegistration
// to generate Libraries/RegisterMonoModules.cpp (iOS, no FastICall, stripping disabled).
// Reads assemblies with Mono.Cecil 0.6 from Unity's bundled Mono; type and method traversal order matches the Unity editor.
//
// Usage: mono RegisterMonoModulesGen.exe <Managed dir> <out.cpp> <unityVersion>
//
// Correspondence with Unity's original logic (decompiled from Unity 3.5.7 UnityEditor.dll):
//   AssemblyReferenceChecker.CollectReferences: Directory.GetFiles for .dll, ignoreSystemDlls = false
//   BuildNativeMethodList: skip the assembly named "System"; collect names of methods that are static + PInvokeImpl + module name "__Internal"
//   WhoReferencesClass("UnityEngine.Gyroscope", true): in assemblies other than System*/UnityEngine/Mono.Posix,
//     search TypeReferences for type references starting with that name; if any, gEnableGyroscope = true
//   GenerateRegisterInternalCalls: only UnityEngine.dll; for each type, Methods first, then Constructors
//   GenerateRegisterClasses: with stripping disabled, only calls _RegisterAllClasses
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;

class RegisterMonoModulesGen
{
    static bool IsIgnoredSystemDll(string name)
    {
        return name.StartsWith("System") || name.Equals("UnityEngine") || name.Equals("Mono.Posix");
    }

    static string SymbolName(string assemblyName)
    {
        string name = assemblyName.Replace(".", "_");
        name = name.Replace("-", "_");
        name = name.Replace(" ", "_");
        return name;
    }

    static ArrayList BuildNativeMethodList(List<AssemblyDefinition> assemblies)
    {
        ArrayList names = new ArrayList();
        foreach (AssemblyDefinition assembly in assemblies)
        {
            if ("System".Equals(assembly.Name.Name))
            {
                continue;
            }
            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                foreach (MethodDefinition method in type.Methods)
                {
                    if (method.IsStatic && method.IsPInvokeImpl && method.PInvokeInfo.Module.Name.Equals("__Internal"))
                    {
                        if (names.Contains(method.Name))
                        {
                            throw new SystemException("Duplicate native method found : " + method.Name);
                        }
                        names.Add(method.Name);
                    }
                }
            }
        }
        return names;
    }

    static bool ReferencesClass(List<AssemblyDefinition> assemblies, string klass)
    {
        foreach (AssemblyDefinition assembly in assemblies)
        {
            if (IsIgnoredSystemDll(assembly.Name.Name))
            {
                continue;
            }
            foreach (TypeReference reference in assembly.MainModule.TypeReferences)
            {
                if (reference.FullName.StartsWith(klass))
                {
                    return true;
                }
            }
        }
        return false;
    }

    static void WriteInternalCall(TypeDefinition type, MethodDefinition method, TextWriter output)
    {
        if (!method.IsInternalCall)
        {
            return;
        }
        string declaration = string.Format("\tvoid Register_{0}_{1}_{2} ();", type.Namespace, type.Name, method.Name);
        string call = string.Format("\tRegister_{0}_{1}_{2} ();", type.Namespace, type.Name, method.Name);
        call = call.Replace('.', '_');
        declaration = declaration.Replace('.', '_');
        output.WriteLine(declaration);
        output.WriteLine(call);
    }

    static int Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("usage: RegisterMonoModulesGen.exe <out.cpp> <unityVersion> <dll>...");
            return 2;
        }
        string outputPath = args[0];
        string unityVersion = args[1];

        // Unity lists the Managed dir with Directory.GetFiles; the caller passes the files instead, because the Mac mono
        // under maloader cannot enumerate directories (GetFiles returns nothing).
        // The original registration order is the byte order of file names (mscorlib after UnityEngine); sort explicitly here rather than relying on the caller
        string[] files = new string[args.Length - 2];
        Array.Copy(args, 2, files, 0, files.Length);
        Array.Sort(files, StringComparer.Ordinal);
        List<string> fileNames = new List<string>();
        List<AssemblyDefinition> assemblies = new List<AssemblyDefinition>();
        foreach (string file in files)
        {
            if (Path.GetExtension(file) != ".dll")
            {
                continue;
            }
            fileNames.Add(Path.GetFileName(file));
            assemblies.Add(AssemblyFactory.GetAssembly(file));
        }
        Console.WriteLine("assemblies: " + string.Join(", ", fileNames.ToArray()));

        ArrayList nativeMethods = BuildNativeMethodList(assemblies);
        using (StreamWriter writer = new StreamWriter(outputPath))
        {
            // The original was generated on a Mac; line endings are \n
            writer.NewLine = "\n";
            writer.WriteLine("#include \"RegisterMonoModules.h\"");
            writer.WriteLine(string.Empty);
            writer.WriteLine("extern bool gEnableGyroscope;");
            writer.WriteLine(string.Empty);
            writer.WriteLine("extern \"C\"\n{");
            writer.WriteLine("\ttypedef void* gpointer;");
            writer.WriteLine("\ttypedef int gboolean;");
            writer.WriteLine("#if !(TARGET_IPHONE_SIMULATOR)");
            writer.WriteLine("\tconst char*\t\t\tUnityIPhoneRuntimeVersion = \"{0}\";", unityVersion);
            writer.WriteLine("\tvoid\t\t\t\tmono_dl_register_symbol (const char* name, void *addr);");
            writer.WriteLine("\textern int \t\t\tmono_ficall_flag;");
            writer.WriteLine("\tvoid\t\t\t\tmono_aot_register_module(gpointer *aot_info);");
            writer.WriteLine("\textern gboolean\t\tmono_aot_only;");
            for (int index = 0; index < fileNames.Count; index++)
            {
                writer.WriteLine("\textern gpointer*\tmono_aot_module_{0}_info; // {1}",
                                 SymbolName(assemblies[index].Name.Name), fileNames[index]);
            }
            writer.WriteLine("#endif // !(TARGET_IPHONE_SIMULATOR)");
            foreach (string name in nativeMethods)
            {
                writer.WriteLine("\tvoid\t{0}();", name);
            }
            writer.WriteLine("}");
            writer.WriteLine("void RegisterMonoModules()");
            writer.WriteLine("{");
            if (ReferencesClass(assemblies, "UnityEngine.Gyroscope"))
            {
                writer.WriteLine("    gEnableGyroscope = true;");
            }
            else
            {
                writer.WriteLine("    gEnableGyroscope = false;");
            }
            writer.WriteLine("#if !(TARGET_IPHONE_SIMULATOR)");
            writer.WriteLine("\tmono_aot_only = true;");
            writer.WriteLine("\tmono_ficall_flag = {0};", "false");
            foreach (AssemblyDefinition assembly in assemblies)
            {
                writer.WriteLine("\tmono_aot_register_module(mono_aot_module_{0}_info);", SymbolName(assembly.Name.Name));
            }
            writer.WriteLine(string.Empty);
            foreach (string name in nativeMethods)
            {
                writer.WriteLine("\tmono_dl_register_symbol(\"{0}\", (void*)&{0});", name);
            }
            writer.WriteLine("#endif // !(TARGET_IPHONE_SIMULATOR)");
            writer.WriteLine("}");
            writer.WriteLine(string.Empty);

            AssemblyDefinition unityEngine = null;
            for (int index = 0; index < fileNames.Count; index++)
            {
                if (fileNames[index] == "UnityEngine.dll")
                {
                    unityEngine = assemblies[index];
                }
            }
            writer.Write("void RegisterAllStrippedInternalCalls ()\n{\n");
            foreach (TypeDefinition type in unityEngine.MainModule.Types)
            {
                foreach (MethodDefinition method in type.Methods)
                {
                    WriteInternalCall(type, method, writer);
                }
                foreach (MethodDefinition constructor in type.Constructors)
                {
                    WriteInternalCall(type, constructor, writer);
                }
            }
            writer.Write("\n}\n");
            writer.Write("void RegisterAllClasses() \n{\n");
            writer.Write("void _RegisterAllClasses();\n _RegisterAllClasses();\n");
            writer.Write("\n}\n");
        }
        Console.WriteLine("native methods: " + nativeMethods.Count + " -> " + outputPath);
        return 0;
    }
}
