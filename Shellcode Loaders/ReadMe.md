# What The F#

## Shellcode Loaders

This directory contains a few different POC scripts that demonstrate how to inject shellcode via F#.

To create F# shellcode, it needs to be in this format:

```
let mutable sc : byte[] = [|0xffuy;0x9duy;0x33uy;|]
//equivalent to \xff\x9d\x33 and 0xff,0x9d,0x33
```