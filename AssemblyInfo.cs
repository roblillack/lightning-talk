using BlackBerry.ApplicationDescriptor;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle ("Lightning Talk")]
[assembly: AssemblyDescription ("Open source app to give small presentations. With bluetooth support.")]
[assembly: AssemblyCopyright ("Robert Lillack <rob@burningsoda.com>, burningsoda.com")]

[assembly: ApplicationIdentifier ("com.burningsoda.lightning-talk")]
[assembly: RequestedPermissions (RestrictedFunctionality.AccessSharedData)]

[assembly: Icon ("icon.png")]
[assembly: AspectRatio (AspectRatio.LANDSCAPE)]
[assembly: NativeLibrary (Architecture.ARM, "libs/arm/libgdiplus.dll")]

[assembly: AssemblyVersion ("1.0.0.*")]
