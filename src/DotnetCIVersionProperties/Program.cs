using System;
using System.IO;
using System.Text;

namespace DotnetCIVersionProperties {

	internal static class Program {

		internal static int Main( string[] arguments ) {

			Arguments args;

			ArgumentsParser.ParseResult exitCode = ArgumentsParser.TryParse( Environment.GetEnvironmentVariable, arguments, Console.Error, out args );
			if( exitCode != ArgumentsParser.ParseResult.Success ) {
				return (int)exitCode;
			}

			args.Output.Directory.Create();

			using( FileStream fs = new FileStream( args.Output.FullName, FileMode.Create, FileAccess.Write, FileShare.Read ) )
			using( StreamWriter sw = new StreamWriter( fs, Encoding.UTF8 ) ) {

				ProjectVersionPropsWriter.Write(
						output: sw,
						major: args.VersionPrefix.Major,
						minor: args.VersionPrefix.Minor,
						patch: args.VersionPrefix.Build,
						branch: args.Branch,
						tag: args.Tag,
						build: args.Build,
						sha1: args.Sha1
					);
			}

			return 0;
		}
	}
}
