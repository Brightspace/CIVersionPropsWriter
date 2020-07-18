using System;
using System.IO;

namespace CIVersionPropsWriter {

	internal sealed class Arguments {

		public FileInfo Output { get; set; }
		public Version AssemblyFileVersion { get; set; }
		public string Branch { get; set; }
		public string Tag { get; set; }
		public int Build { get; set; }
	}
}
