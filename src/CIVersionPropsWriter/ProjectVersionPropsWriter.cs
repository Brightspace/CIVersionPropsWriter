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

			string package;
			if( tag.Equals( $"v{ major }.{ minor }.{ patch }" ) ) {
				package = string.Empty;

			} else if( branch.Equals( "master" ) ) {
				package = $"-rc{ build }";

			} else {
				package = $"-alpha{ build }";
			}

			output.WriteLine( "<Project>" );
			output.WriteLine( "\t<PropertyGroup>" );
			output.WriteLine( $"\t\t<Version>{ major }.{ minor }</Version>" );
			output.WriteLine( $"\t\t<FileVersion>{ major }.{ minor }.{ patch }.{ build }</FileVersion>" );
			output.WriteLine( $"\t\t<InformationalVersion>{ major }.{ minor }.{ patch }{ package }</InformationalVersion>" );
			output.WriteLine( "\t</PropertyGroup>" );
			output.WriteLine( "</Project>" );
		}
	}
}
