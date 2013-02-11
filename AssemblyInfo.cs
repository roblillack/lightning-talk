using BlackBerry.ApplicationDescriptor;
using System.Reflection;
using System.Runtime.CompilerServices;

// <name>: Title of the Application on the home screen
[assembly: AssemblyTitle("Lightning Talk")]
// <description>: …
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("Robert Lillack <rob@burningsoda.com>, burningsoda.com")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// <id>: Unique Identifier of the Application
[assembly: ApplicationIdentifier ("com.burningsoda.lightning-talk")]
// <action> …
[assembly: RequestedPermissions (RestrictedFunctionality.AccessSharedData)]

[assembly: Icon ("icon.png")]
[assembly: AspectRatio (AspectRatio.LANDSCAPE)]
[assembly: NativeLibrary (Architecture.ARM, "libs/arm/libgdiplus.dll")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.
// <versionNumber> + <buildId>
[assembly: AssemblyVersion("1.0.0.*")]