using System;
using System.IO;
using System.Runtime.InteropServices;
using FmodForFoxes;

namespace Gamelab.Services.Sound;

public class DesktopAndMacNativeFmodLibrary : INativeFmodLibrary
{
    private bool _loggingEnabled;

    public void Init(FmodInitMode mode, bool loggingEnabled = false)
    {
        this._loggingEnabled = loggingEnabled;
        NativeLibrary.SetDllImportResolver(
            mode.GetType().Assembly, // Scoping the dllimport switcher for the crossplatform fmod lib.
            (libraryName, assembly, dllImportSearchPath) =>
            {
                libraryName = Path.GetFileNameWithoutExtension(libraryName);
                if (dllImportSearchPath == null)
                {
                    dllImportSearchPath = DllImportSearchPath.AssemblyDirectory;
                }
                return NativeLibrary.Load(SelectDefaultLibraryName(libraryName, this._loggingEnabled), assembly, dllImportSearchPath);
            }
        );
    }


    private string SelectDefaultLibraryName(string libName, bool loggingEnabled = false)
    {
        string name;

        if (OperatingSystem.IsWindows())
        {
            name = loggingEnabled ? $"{libName}L.dll" : $"{libName}.dll";
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsAndroid())
        {
            name = loggingEnabled ? $"lib{libName}L.so" : $"lib{libName}.so";
        }
        else if (OperatingSystem.IsMacOS())
        {
            name = loggingEnabled ? $"lib{libName}L.dylib" : $"lib{libName}.dylib";
        }
        else
        {
            throw new PlatformNotSupportedException();
        }

        return name;
    }
}