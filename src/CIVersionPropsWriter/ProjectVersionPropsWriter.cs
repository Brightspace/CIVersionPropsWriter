using System;
using System.IO;

namespace CIVersionPropsWriter {

	public static class ProjectVersionPropsWriter {

		public static void Write(
				TextWriter output,
				Version assemblyFileVersion,
				string branch,
				string tag,
				int build
			) {

			int major = assemblyFileVersion.Major;
			int minor = assemblyFileVersion.Minor;
			int patch = assemblyFileVersion.Build;

			string suffix;
			if( tag.Equals( $"v{ major }.{ minor }.{ patch }" ) ) {
				suffix = string.Empty;

			} else if( branch.Equals( "master" ) ) {
				suffix = $"rc{ build }";

			} else {
				suffix = $"alpha{ build }";
			}

			output.WriteLine( "<Project>" );
			output.WriteLine( "\t<PropertyGroup>" );
			output.WriteLine( $"\t\t<VersionPrefix>{ major }.{ minor }.{ patch }</VersionPrefix>" );
			output.WriteLine( $"\t\t<VersionSuffix>{ suffix }</VersionSuffix>" );
			output.WriteLine( $"\t\t<AssemblyVersion>{ major }.{ minor }.0.0</AssemblyVersion>" );
			output.WriteLine( $"\t\t<FileVersion>{ major }.{ minor }.{ patch }.{ build }</FileVersion>" );
			output.WriteLine( "\t</PropertyGroup>" );
			output.WriteLine( "</Project>" );
		}
	}
}
