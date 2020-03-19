using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestLoadDuplicateAssemblies
{
    class Program
    {
        private static string _assembly1 = @"D:\Modding\AwesomeInventory\test\TestAssembly\bin\Debug\TestAssembly.dll";
        private static string _assembly2 = @"D:\Modding\AwesomeInventory\test\TestAssembly\bin\Debug\temp\TestAssembly.dll";


        static void Main(string[] args)
        {
            //Assembly assembly1 = Assembly.ReflectionOnlyLoadFrom(_assembly1);
            //PrintTypes(assembly1, "Types in assembly1");

            //Assembly assembly2 = Assembly.LoadFrom(_assembly2);
            //PrintTypes(assembly2, "Types in assembly2");

            byte[] assemblyBytes;

            assemblyBytes = File.ReadAllBytes(_assembly2);
            AppDomain.CurrentDomain.Load(assemblyBytes);

            assemblyBytes = File.ReadAllBytes(_assembly1);
            Assembly assembly = AppDomain.CurrentDomain.Load(assemblyBytes);




            assembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == "TestAssembly").First();
            PrintTypes(assembly, "Types of the assembly loaded in the current AppDomain");

            Console.ReadLine();
        }

        static void PrintTypes(Assembly assembly, string header)
        {
            Console.WriteLine(header);
            foreach(Type type in assembly.GetTypes())
            {
                Console.WriteLine(type.FullName);
            }

            Console.WriteLine();
        }
    }
}
