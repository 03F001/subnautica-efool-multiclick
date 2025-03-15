using System.Reflection;
using System.Runtime.InteropServices;

using org.efool.subnautica.multiclick;

[assembly: AssemblyTitle(Info.title)]
[assembly: AssemblyDescription(Info.desc)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("efool")]
[assembly: AssemblyProduct(Info.name)]
[assembly: AssemblyCopyright("Copyright 2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion(Info.version)]
[assembly: AssemblyFileVersion(Info.version)]

[assembly: ComVisible(false)]

[assembly: Guid("1fa466d0-37ed-4e00-9e58-1ad9aec96dc3")]

namespace org.efool.subnautica.multiclick {
public static class Info
{
	public const string FQN = "org.efool.subnautica.multiclick";
	public const string name = "efool-multiclick";
	public const string title = "efool's Multiclick";
	public const string desc = "efool's multi-click mod for Subnautica";
	public const string version = "0.0.2";
}
}