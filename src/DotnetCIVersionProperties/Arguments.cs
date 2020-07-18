using System;
using System.IO;

namespace DotnetCIVersionProperties {

	internal sealed class Arguments {

		public Arguments(
				FileInfo output,
				Version assemblyFileVersion,
				string branch,
				string tag,
				int build,
				string sha1
			) {

			Output = output;
			AssemblyFileVersion = assemblyFileVersion;
			Branch = branch;
			Tag = tag;
			Build = build;
			Sha1 = sha1;
		}

		public FileInfo Output { get; }
		public Version AssemblyFileVersion { get; }
		public string Branch { get; }
		public string Tag { get; }
		public int Build { get; }
		public string Sha1 { get; }
	}
}
