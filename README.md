This is Inceptor. It is a utility to make it possible to intercept and practically replace any method in any assembly with your own code. 

Right now it is only being used as a mod extender for Citites: Skylines, though it should work with most any C# program or Unity game.

Example usage:

Inceptor.exe C:\dev\originalcities.dll C:\dev\Inceptor\InceptorAssembly\bin\Debug\InceptorAssembly.dll C:\dev\Assembly-CSharp.dll C:\dev\methods.txt

Note: You will need to copy InceptorAssembly.dll into the same folder as the output directory where the assembly will eb run from (ie, in C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities_Data\Managed )

originalcitites.dll is the original DLL you want to inject methods into(originally named Assembly-CSharp.dll). Assembly-CSharp.dll is the output assembly. Methods.txt is just a useful log output with all of the method names, good for copy/pasting into your mod.

See https://github.com/Earlz/CitiesTestMod for an example mod using it

Very WIP and it does pretty crazy stuff and I'm somewhat surprised it works. 

It uses Mono.Cecil to inject into every method of the assembly a call to the InceptorAssembly. By referencing InceptorAsembly in your own project you can add a method to the list it checks and intercept a method call

## TODO:

* Add a way to exclude methods from being injected into
* Properly handle value type return values, and generic types in both return values, base types, and parameters
* Documentation
* An easy way to automatically do this for Cities Skyline by looking up Steam directory etc (similar to Script Extender for Skyrim)

## Known Issues:

* There is a definite performance impact from using this. I've succeeded in keeping it lock-free, but adding a method call to every single method will add overhead.
* Returning value types does not work at all
* Returning or reading generic parameters and generic instantiations (ie, `List<string>`) does not work at all. These values will be set to null in the interceptor method
* Tested only on Windows