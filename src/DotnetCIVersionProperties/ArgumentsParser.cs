using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DotnetCIVersionProperties {

	internal static class ArgumentsParser {

		internal const string Usage = "Usage: dotnet ci-version-properties --output <path> [--version <version>]";
		internal const string VersionPrefixVariable = "VERSION_PREFIX";

		private delegate bool EnvironmentValueParser( string value, out string parsed );

		private static readonly IReadOnlyDictionary<string, EnvironmentValueParser> BuildNumberVariables =
			new SortedDictionary<string, EnvironmentValueParser> {
				{ Vendors.AppVeyor.BuildNumberVariable, NonWhiteSpaceParser },
				{ Vendors.CircleCi.BuildNumberVariable, NonWhiteSpaceParser },
				{ Vendors.GithubActions.BuildNumberVariable, NonWhiteSpaceParser }
			};

		private static readonly IReadOnlyDictionary<string, EnvironmentValueParser> BranchVariables =
			new SortedDictionary<string, EnvironmentValueParser> {
				{ Vendors.AppVeyor.BranchVariable, NonWhiteSpaceParser },
				{ Vendors.CircleCi.BranchVariable, NonWhiteSpaceParser },
				{ Vendors.GithubActions.RefVariable, GitBranchRefParser }
			};

		private static readonly IReadOnlyDictionary<string, EnvironmentValueParser> TagVariables =
			new SortedDictionary<string, EnvironmentValueParser> {
				{ Vendors.AppVeyor.TagVariable, NonWhiteSpaceParser },
				{ Vendors.CircleCi.TagVariable, NonWhiteSpaceParser },
				{ Vendors.GithubActions.RefVariable, GitTagRefParser }
			};

		private static readonly IReadOnlyDictionary<string, EnvironmentValueParser> Sha1Variables =
			new SortedDictionary<string, EnvironmentValueParser> {
				{ Vendors.AppVeyor.Sha1Variable, NonWhiteSpaceParser },
				{ Vendors.CircleCi.Sha1Variable, NonWhiteSpaceParser },
				{ Vendors.GithubActions.Sha1Variable, NonWhiteSpaceParser }
			};

		public enum ParseResult {

			Success = 0,
			NoArguments = 1,
			ExtraArguments = 2,

			MissingOutputPath = 10,
			MissingVersionPrefix = 11,
			MissingBuildNumber = 12,
			MissingBranch = 13,
			MissingSha1 = 14,

			InvalidVersionPrefix = 20,
			InvalidOutputPath = 21,
			InvalidBuildNumber = 22
		}

		internal static ParseResult TryParse(
				Func<string, string> environment,
				string[] arguments,
				TextWriter errors,
				out Arguments args
			) {

			args = null;

			FileInfo outputPath = null;
			Version versionPrefix = null;
			List<string> extra = new List<string>();

			if( arguments.Length == 0 ) {
				errors.WriteLine( Usage );
				return ParseResult.NoArguments;
			}

			int index = 0;
			while( index < arguments.Length ) {

				string arg = arguments[ index ];
				switch( arg ) {

					case "--versionPrefix": {

							if( index + 1 == arguments.Length ) {
								break;
							}

							string argValue = arguments[ ++index ];
							if( !Version.TryParse( argValue, out versionPrefix ) ) {

								errors.WriteLine( "Invalid assembly file version: {0}", argValue );
								return ParseResult.InvalidVersionPrefix;
							}

							break;
						}

					case "--output": {

							if( index + 1 == arguments.Length ) {
								break;
							}

							string argValue = arguments[ ++index ];
							try {
								outputPath = new FileInfo( argValue );
							} catch {
								errors.WriteLine( "Invalid output path: {0}", argValue );
								return ParseResult.InvalidOutputPath;
							}

							break;
						}

					default:
						extra.Add( arg );
						break;
				}

				index++;
			}

			if( extra.Count > 0 ) {
				errors.WriteLine( "Invalid arguments: {0}", string.Join( " ", extra ) );
				errors.WriteLine();
				errors.WriteLine( Usage );
				return ParseResult.ExtraArguments;
			}

			if( outputPath == null ) {

				errors.WriteLine( "Invalid arguments: output path not specified" );
				errors.WriteLine();
				errors.WriteLine( Usage );
				return ParseResult.MissingOutputPath;
			}

			if( versionPrefix == null ) {

				string afv = environment( VersionPrefixVariable );
				if( string.IsNullOrEmpty( afv ) ) {

					errors.WriteLine( $"{ VersionPrefixVariable } environment variable not set" );
					return ParseResult.MissingVersionPrefix;
				}

				if( !Version.TryParse( afv, out versionPrefix ) ) {

					errors.WriteLine( $"Invalid { VersionPrefixVariable } value: { afv }" );
					return ParseResult.InvalidVersionPrefix;
				}
			}

			if( versionPrefix.Major == -1
				|| versionPrefix.Minor == -1
				|| versionPrefix.Build == -1
				|| versionPrefix.Revision != -1 ) {

				errors.WriteLine( $"Assembly file version must be in the format MAJOR.MINOR.PATCH" );
				return ParseResult.InvalidVersionPrefix;
			}

			int buildNumber;
			{
				if( !TryGetFirstVariable( environment, BuildNumberVariables, out string bnVariable, out string bn ) ) {

					WriteMissingEnvironmentVariables( errors, BuildNumberVariables );
					return ParseResult.MissingBuildNumber;
				}

				if( !int.TryParse( bn, out buildNumber ) ) {

					errors.WriteLine( $"Invalid { bnVariable } value: { bn }" );
					return ParseResult.InvalidBuildNumber;
				}
			}

			if( !TryGetFirstVariable( environment, BranchVariables, out string branchVariable, out string branch ) ) {

				WriteMissingEnvironmentVariables( errors, BranchVariables );
				return ParseResult.MissingBranch;
			}

			if( !TryGetFirstVariable( environment, TagVariables, out string _, out string tag ) ) {
				tag = string.Empty;
			}

			if( !TryGetFirstVariable( environment, Sha1Variables, out string _, out string sha1 ) ) {

				WriteMissingEnvironmentVariables( errors, Sha1Variables );
				return ParseResult.MissingSha1;
			}

			args = new Arguments(
					output: outputPath,
					versionPrefix: versionPrefix,
					branch: branch,
					tag: tag,
					build: buildNumber,
					sha1: sha1
				);

			return ParseResult.Success;
		}

		private static void WriteMissingEnvironmentVariables(
				TextWriter errors,
				IReadOnlyDictionary<string, EnvironmentValueParser> definitions
			) {

			string names = string.Join( ", ", definitions.Keys );
			errors.WriteLine( $"One of [ { names } ] environment variables is required" );
		}

		private static bool TryGetFirstVariable(
				Func<string, string> environment,
				IReadOnlyDictionary<string, EnvironmentValueParser> definitions,
				out string variable,
				out string value
			) {

			foreach( KeyValuePair<string, EnvironmentValueParser> definition in definitions ) {

				string raw = environment( definition.Key );
				if( raw == null ) {
					continue;
				}

				if( definition.Value( raw, out string parsed ) ) {

					variable = definition.Key;
					value = parsed;
					return true;
				}
			}

			variable = null;
			value = null;
			return false;
		}

		private static bool NonWhiteSpaceParser( string value, out string parsed ) {

			if( !string.IsNullOrWhiteSpace( value ) ) {
				parsed = value;
				return true;
			}

			parsed = null;
			return false;
		}

		private static bool GitBranchRefParser( string value, out string branch ) {

			Match m = Regex.Match( value, "^refs/heads/(.+)" );
			if( m.Success ) {
				branch = m.Groups[ 1 ].Value;
			} else {
				branch = string.Empty;
			}

			return true;
		}

		private static bool GitTagRefParser( string value, out string tag ) {

			Match m = Regex.Match( value, "^refs/tags/(.+)" );
			if( m.Success ) {
				tag = m.Groups[ 1 ].Value;
			} else {
				tag = string.Empty;
			}

			return true;
		}

	}
}
