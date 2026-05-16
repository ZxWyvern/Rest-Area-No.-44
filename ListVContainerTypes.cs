using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        var asm = Assembly.LoadFrom(@"D:\RestAreaNo44\Library\Bee\artifacts\1900b0aE.dag\VContainer.dll");
        foreach (var t in asm.GetExportedTypes())
        {
            if (t.FullName.Contains("Lifetime") || t.FullName.Contains("IContainer") || t.Namespace == "VContainer.Unity")
                Console.WriteLine(t.FullName);
        }
    }
}
