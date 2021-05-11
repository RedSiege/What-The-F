# Operating with FSharp

This is a short write up (based off of the presentation in this repo), that details how to operate using F#. 

There are several different methods to run F# code on a target host. This is a quick explanation of how each method works. They're ranked in order of priority (meaning when we're on an assessment, we'll try #1, then #2, etc.)

## 1. F# is already installed.

This is the easiest method by far. Typically, if Visual Studio is installed on the target host, then F# will likely be there. If F# is already installed, you can compile F# binaries (without standalone mode) and run them via execute-assembly.

You can check for the existence of F# by searching for the following two directories:

- C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\CommonExtensions\Microsoft\FSharp
- C:\Program Files\dotnet\sdk\5.0.103\FSharp

Alternatively, you could search for the FSharp.Core.dll in the global assembly cache.

It'd be quite easy to write an aggressor script to automate searching for the existence of FSharp. 

## 2. Resolve Assembly Dependencies in Memory

This method has the most promise. At the moment, it's just a proof-of-concept, but by turning this into a BOF, F# can become a mainstream language to use with CobaltStrike.

To see the proof-of-concept, check out the "UnmanagedFSharp" folder in this repo. The basic idea behind this method is we are using unmanaged code (written in C) to load our managed code (written in F#), but since FSharp.Core.dll does not exist on the target environment, the unmanaged code will dynamically resolve that dependency error and Assembly.Load() the FSharp.Core.dll for us. The way this works is by including the bytearray of FSharp.Core.dll in the unmanaged code and using the AppDomain.AssemblyResolve callback function to load the FSharp.Core.dll into memory (via Assembly.Load()) when the dependency error pops up. For more details on how AssemblyResolve works, [check out Jean Maes' article.](https://redteamer.tips/a-tale-of-net-assemblies-cobalt-strike-size-constraints-and-reflection/)

At the moment, to use this method, you'll need to download the "UnmanagedFSharp" repo, add in your F# assembly's hex (where our HelloWorld hex is now), compile it and upload it to disk. We get that uploading to disk isn't ideal, but this is just a proof-of-concept. It should be rather easy to port this concept into a BOF (Cobalt Strike Beacon Object File), which will allow executing F# assemblies in memory, just like execute-assembly. 

If anyone is interested in collaborating on authoring the BOF, please ping @JoeLeonJr or @ChrisTruncer.

## 3. Drop FSI (and dependencies) to disk

We've used this method on actual red team assessments and it's worked decently well. The idea is to grab the Microsoft-signed FSI.exe binary and all needed dependencies, zip them, transfer them to the target and then unzip them. From there, you can run an fsharp script file, such as those in the "Shellcode Loaders" directory in this repo.

Required files:
- fsi.exe
- FSharp.Core.dll
- FSharp.Compiler.Interactive.Settings.dll
- FSharp.Compiler.Private.dll
- Microsoft.Build.Utilities.Core.dll

## 4. Compile a Standalone Binary

When compiling your F# source code, use the --standalone flag. This will include all of the relevant dependencies in your exe, so it can execute on a target machine without any F# dlls installed. The challenge with this method is it creates enormous files. A simple hello world file in exe format is 1.5Mb. This is a restriction with CobaltStrike, but not necessarily other C2 tools.

## 5. Drop FSharp.Core.dll to disk

When compiling your F# source code, don't use the standalone flag, just compile it normally. Then transfer FSharp.Core.dll to the target's folder where you'll be operating from. For example: if using execute-assembly in Cobalt Strike, drop this dll in the folder where the spawnas binary lives. Importantly, you'll probably need local admin rights to do this since most of these binaries are located in C:\Windows\System32 or C:\Windows\SysWOW64. Then you can just use execute-assembly.

Alternatively, you can upload your compiled F# file and the FSharp.Core.dll file into the same folder (any folder) on the target machine and kick off execution manually. You don't benefit from executing an assembly in memory, but at least you don't need local admin rights.

## 6. Add FSharp.Core.dll to the Global Assembly Cache (GAC)

When you install FSharp.Core.dll to the GAC, you can run F# binaries anywhere on disk (or via execute-assembly), without having to worry about where the DLL is located on disk. To do this, you'll need local admin rights. Also, assuming you're not on a machine with Visual Studio installed, you'll need to transfer a few GAC-related utility tools to the target host in order to install the dll. [Here's an article detailing that process.](http://seesharpdeveloper.blogspot.com/2015/07/using-gacutil-on-non-development-server.html
)

