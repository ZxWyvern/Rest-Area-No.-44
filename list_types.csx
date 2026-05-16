#r "D:\RestAreaNo44\Library\Bee\artifacts\1900b0aE.dag\VContainer.dll"
using System;
using System.Reflection;

var asm = Assembly.LoadFrom(@"D:\RestAreaNo44\Library\Bee\artifacts\1900b0aE.dag\VContainer.dll");
foreach (var t in asm.GetExportedTypes())
{
    if (t.FullName.Contains("Lifetime") || t.FullName.Contains("IContainer"))
        Console.WriteLine(t.FullName);
}
