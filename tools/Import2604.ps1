param([string]$SddHome = 'C:\SiemensEDA\XENTP2604\SDD_HOME')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$env:TEMP = Join-Path $root 'build'
$env:TMP = $env:TEMP
$out = Join-Path $root 'build\interop2604'
New-Item -ItemType Directory -Path $out -Force | Out-Null
Add-Type -TypeDefinition @"
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
public class Interop2604 : ITypeLibImporterNotifySink {
 [DllImport("oleaut32.dll", CharSet=CharSet.Unicode, PreserveSig=false)]
 static extern void LoadTypeLibEx(string file, int kind, out ITypeLib lib);
 public void ReportEvent(ImporterEventKind kind, int code, string message) { Console.WriteLine(message); }
 public Assembly ResolveRef(object typeLib) { throw new Exception("Unexpected external type library dependency"); }
 public static void Import(string source, string target, string ns, string guid, int major) {
  ITypeLib lib; LoadTypeLibEx(source, 2, out lib);
  IntPtr ptr; lib.GetLibAttr(out ptr);
  var a = (System.Runtime.InteropServices.ComTypes.TYPELIBATTR)Marshal.PtrToStructure(ptr, typeof(System.Runtime.InteropServices.ComTypes.TYPELIBATTR));
  Console.WriteLine("{0}: GUID={1} version={2}.{3} platform={4}",source,a.guid,a.wMajorVerNum,a.wMinorVerNum,a.syskind);
  lib.ReleaseTLibAttr(ptr);
  if (a.guid != new Guid(guid) || a.wMajorVerNum != major || a.wMinorVerNum != 0 ||
      a.syskind != System.Runtime.InteropServices.ComTypes.SYSKIND.SYS_WIN64)
    throw new Exception("Unexpected type library identity/version/platform: " + source);
  var asm = new TypeLibConverter().ConvertTypeLibToAssembly(lib,target,TypeLibImporterFlags.ImportAsX64,new Interop2604(),null,null,ns,null);
  asm.Save(System.IO.Path.GetFileName(target));
  Marshal.ReleaseComObject(lib);
 }
}
"@
[Interop2604]::Import("$SddHome\common\win64\lib\PDBEditor.tlb", "$out\Interop.MGCPCBPartsEditor.dll", 'MGCPCBPartsEditor','D434AC01-C1D4-4BDB-B994-8809BBE675CE',151)
[Interop2604]::Import("$SddHome\lm\win64\bin\LibraryManager.tlb", "$out\Interop.LibraryManager.dll", 'LibraryManager','BD83B6B2-BE4F-42BF-B83E-F11B676B82B6',151)
[Interop2604]::Import("$SddHome\common\win64\lib\MGCPCBReleaseEnvironment.tlb", "$out\Interop.MGCPCBReleaseEnvironmentLib.dll", 'MGCPCBReleaseEnvironmentLib','2B062199-EAFF-4705-8FA5-BC3C49B18F0A',1)
