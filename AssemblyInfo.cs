using BlackBerry.ApplicationDescriptor;
using System.Reflection;
using System.Runtime.CompilerServices;

// <name>: Title of the Application on the home screen
[assembly: AssemblyTitle("Barefoot Presenter")]
// <description>: …
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("Robert Lillack <rob@burningsoda.com>, burningsoda.com")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// <id>: Unique Identifier of the Application
[assembly: ApplicationIdentifier ("com.burningsoda.barefoot-presenter")]
// <action> …
[assembly: RequestedPermissions (Action.AccessSharedData)]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.
// <versionNumber> + <buildId>
[assembly: AssemblyVersion("1.0.*")]