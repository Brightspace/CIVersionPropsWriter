﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace CIVersionPropsWriter.Tests {

	[TestFixture]
	internal sealed class ArgumentsParserTests {

		private const string Usage = ArgumentsParser.Usage + "\r\n";

		private static IEnumerable<EnvironmentVariablesTestCase> VendorEnvironmentVariableTestCases() {

			yield return new EnvironmentVariablesTestCase(
					testCaseName: "AppVeyor",
					branchVariable: Vendors.AppVeyor.BranchVariable,
					tagVariable: Vendors.AppVeyor.TagVariable,
					buildNumberVariable: Vendors.AppVeyor.BuildNumberVariable
				);

			yield return new EnvironmentVariablesTestCase(
					testCaseName: "CircleCi",
					branchVariable: Vendors.CircleCi.BranchVariable,
					tagVariable: Vendors.CircleCi.TagVariable,
					buildNumberVariable: Vendors.CircleCi.BuildNumberVariable
				);
		}

		[TestCaseSource( nameof( VendorEnvironmentVariableTestCases ) )]
		public void AssemblyFileVersionArg(
				string branchVariable,
				string tagVariable,
				string buildNumberVariable
			) {

			Func<string, string> env = MockEnvironment( new Dictionary<string, string> {
				{ branchVariable, "master" },
				{ tagVariable, "tagger" },
				{ buildNumberVariable, "3433" }
			} );

			string[] arguments = new[] {
				"--assemblyFileVersion", "10.6.9",
				"--output", "versions.cs"
			};

			Arguments args = AssertSuccessfulTryParse( env, arguments );
			Assert.That( args.Output.Name, Is.EqualTo( "versions.cs" ) );
			Assert.That( args.AssemblyFileVersion, Is.EqualTo( new Version( 10, 6, 9 ) ) );
			Assert.That( args.Branch, Is.EqualTo( "master" ) );
			Assert.That( args.Tag, Is.EqualTo( "tagger" ) );
			Assert.That( args.Build, Is.EqualTo( 3433 ) );
		}

		[Test]
		[TestCaseSource( nameof( VendorEnvironmentVariableTestCases ) )]
		public void EnvironmentVariables(
				string branchVariable,
				string tagVariable,
				string buildNumberVariable
			) {

			Func<string, string> env = MockEnvironment( new Dictionary<string, string> {
				{ ArgumentsParser.AssemblyFileVersionVariable, "10.6.9" },
				{ branchVariable, "master" },
				{ tagVariable, "tagger" },
				{ buildNumberVariable, "3433" }
			} );

			string[] arguments = new[] {
				"--output", "versions.cs"
			};

			Arguments args = AssertSuccessfulTryParse( env, arguments );
			Assert.That( args.Output.Name, Is.EqualTo( "versions.cs" ) );
			Assert.That( args.AssemblyFileVersion, Is.EqualTo( new Version( 10, 6, 9 ) ) );
			Assert.That( args.Branch, Is.EqualTo( "master" ) );
			Assert.That( args.Tag, Is.EqualTo( "tagger" ) );
			Assert.That( args.Build, Is.EqualTo( 3433 ) );
		}

		private sealed class EnvironmentVariablesTestCase : TestCaseData {

			public EnvironmentVariablesTestCase(
					string testCaseName,
					string branchVariable,
					string tagVariable,
					string buildNumberVariable
				)
				: base( branchVariable, tagVariable, buildNumberVariable ) {

				SetName( testCaseName );
			}
		}

		[Test]
		[TestCase( "refs/heads/master", "master", "" )]
		[TestCase( "refs/heads/feature/xyz", "feature/xyz", "" )]
		[TestCase( "refs/tags/v1.2.0", "", "v1.2.0" )]
		[TestCase( "refs/tags/random", "", "random" )]
		[TestCase( "refs/remotes/origin/bump", "", "" )]
		public void GithubActions_Branch(
				string gitRef,
				string expectedBranch,
				string expectedTag
			) {

			Func<string, string> env = MockEnvironment( new Dictionary<string, string> {
				{ Vendors.GithubActions.BuildNumberVariable, "3433" },
				{ Vendors.GithubActions.RefVariable, gitRef }
			} );

			string[] arguments = new[] {
				"--assemblyFileVersion", "10.6.9",
				"--output", "versions.cs"
			};

			Arguments args = AssertSuccessfulTryParse( env, arguments );
			Assert.That( args.Output.Name, Is.EqualTo( "versions.cs" ) );
			Assert.That( args.AssemblyFileVersion, Is.EqualTo( new Version( 10, 6, 9 ) ) );
			Assert.That( args.Branch, Is.EqualTo( expectedBranch ) );
			Assert.That( args.Tag, Is.EqualTo( expectedTag ) );
			Assert.That( args.Build, Is.EqualTo( 3433 ) );
		}

		private Arguments AssertSuccessfulTryParse(
				Func<string, string> environment,
				string[] arguments
			) {

			StringBuilder errors = new StringBuilder();
			using( StringWriter errorsWriter = new StringWriter( errors ) ) {

				ArgumentsParser.ParseResult result = ArgumentsParser.TryParse(
						environment,
						arguments,
						errorsWriter,
						out Arguments args
					);

				Assert.That( errors.ToString(), Is.Empty, "No errors should have been output" );
				Assert.That( result, Is.EqualTo( ArgumentsParser.ParseResult.Success ), "Success result expected" );

				return args;
			}
		}

		private static IEnumerable<InvalidArgsTestCase> InvalidArgumentsTestCases() {

			Func<string, string> withAssemblyFileVersion = MockEnvironment( new Dictionary<string, string> {
				{ ArgumentsParser.AssemblyFileVersionVariable, "10.6.9" },
				{ Vendors.GithubActions.BuildNumberVariable, "3433" },
				{ Vendors.GithubActions.RefVariable, "refs/heads/master" }
			} );

			Func<string, string> withoutAssemblyFileVersion = MockEnvironment( new Dictionary<string, string> {
				{ Vendors.GithubActions.BuildNumberVariable, "3433" },
				{ Vendors.GithubActions.RefVariable, "refs/heads/master" }
			} );

			yield return new InvalidArgsTestCase(
					name: "NoArguments",
					environment: withAssemblyFileVersion,
					arguments: Array.Empty<string>(),
					expectedResult: ArgumentsParser.ParseResult.NoArguments,
					expectedErrors: Usage
				);

			yield return new InvalidArgsTestCase(
					name: "ExtraArguments - Before",
					environment: withAssemblyFileVersion,
					arguments: new[] { "--random", "--output", "out.props" },
					expectedResult: ArgumentsParser.ParseResult.ExtraArguments,
					expectedErrors: "Invalid arguments: --random\r\n\r\n" + Usage
				);

			yield return new InvalidArgsTestCase(
					name: "ExtraArguments - After",
					environment: withAssemblyFileVersion,
					arguments: new[] { "--output", "out.props", "--random" },
					expectedResult: ArgumentsParser.ParseResult.ExtraArguments,
					expectedErrors: "Invalid arguments: --random\r\n\r\n" + Usage
				);

			yield return new InvalidArgsTestCase(
					name: "MissingOutputPath",
					environment: withoutAssemblyFileVersion,
					arguments: new[] { "--assemblyFileVersion", "1.2.3" },
					expectedResult: ArgumentsParser.ParseResult.MissingOutputPath,
					expectedErrors: "Invalid arguments: output path not specified\r\n\r\n" + Usage
				);

			yield return new InvalidArgsTestCase(
					name: "IncompleteOutputPath",
					environment: withAssemblyFileVersion,
					arguments: new[] { "--output" },
					expectedResult: ArgumentsParser.ParseResult.MissingOutputPath,
					expectedErrors: "Invalid arguments: output path not specified\r\n\r\n" + Usage
				);

			yield return new InvalidArgsTestCase(
					name: "MissingAssemblyFileVersion",
					environment: withoutAssemblyFileVersion,
					arguments: new[] { "--output", "out.props" },
					expectedResult: ArgumentsParser.ParseResult.MissingAssemblyFileVersion,
					expectedErrors: "ASSEMBLY_FILE_VERSION environment variable not set\r\n"
				);

			yield return new InvalidArgsTestCase(
					name: "IncompleteAssemblyFileVersion",
					environment: withoutAssemblyFileVersion,
					arguments: new[] { "--output", "out.props", "--assemblyFileVersion" },
					expectedResult: ArgumentsParser.ParseResult.MissingAssemblyFileVersion,
					expectedErrors: "ASSEMBLY_FILE_VERSION environment variable not set\r\n"
				);

			yield return new InvalidArgsTestCase(
					name: "InvalidAssemblyFileVersion - Command Line",
					environment: withoutAssemblyFileVersion,
					arguments: new[] { "--output", "out.props", "--assemblyFileVersion", "junk" },
					expectedResult: ArgumentsParser.ParseResult.InvalidAssemblyFileVersion,
					expectedErrors: "Invalid assembly file version: junk\r\n"
				);

			yield return new InvalidArgsTestCase(
					name: "InvalidAssemblyFileVersion - Environment",
					environment: MockEnvironment( new Dictionary<string, string> {
						{ ArgumentsParser.AssemblyFileVersionVariable, "Junk" },
						{ Vendors.GithubActions.BuildNumberVariable, "3433" },
						{ Vendors.GithubActions.RefVariable, "refs/heads/master" }
					} ),
					arguments: new[] { "--output", "out.props" },
					expectedResult: ArgumentsParser.ParseResult.InvalidAssemblyFileVersion,
					expectedErrors: "Invalid ASSEMBLY_FILE_VERSION value: Junk\r\n"
				);

			yield return new InvalidArgsTestCase(
					name: "InvalidAssemblyFileVersion - Revision",
					environment: withoutAssemblyFileVersion,
					arguments: new[] { "--output", "out.props", "--assemblyFileVersion", "1.2.3.4" },
					expectedResult: ArgumentsParser.ParseResult.InvalidAssemblyFileVersion,
					expectedErrors: "Assembly file version must be in the format MAJOR.MINOR.PATCH\r\n"
				);


			yield return new InvalidArgsTestCase(
					name: "InvalidOutputPath",
					environment: withAssemblyFileVersion,
					arguments: new[] { "--output", "  " },
					expectedResult: ArgumentsParser.ParseResult.InvalidOutputPath,
					expectedErrors: "Invalid output path:   \r\n"
				);

			yield return new InvalidArgsTestCase(
					name: "MissingBuildNumber",
					environment: MockEnvironment( new Dictionary<string, string> {
						{ Vendors.GithubActions.RefVariable, "refs/heads/master" }
					} ),
					arguments: new[] { "--output", "out.props", "--assemblyFileVersion", "1.2.3" },
					expectedResult: ArgumentsParser.ParseResult.MissingBuildNumber,
					expectedErrors: "One of [ APPVEYOR_BUILD_NUMBER, CIRCLE_BUILD_NUM, GITHUB_RUN_NUMBER ] environment variables is required\r\n"
				);

			yield return new InvalidArgsTestCase(
					name: "InvalidBuildNumber",
					environment: MockEnvironment( new Dictionary<string, string> {
						{ Vendors.GithubActions.BuildNumberVariable, "junk" },
						{ Vendors.GithubActions.RefVariable, "refs/heads/master" }
					} ),
					arguments: new[] { "--output", "out.props", "--assemblyFileVersion", "1.2.3" },
					expectedResult: ArgumentsParser.ParseResult.InvalidBuildNumber,
					expectedErrors: "Invalid GITHUB_RUN_NUMBER value: junk\r\n"
				);

			yield return new InvalidArgsTestCase(
					name: "MissingBranch",
					environment: MockEnvironment( new Dictionary<string, string> {
						{ Vendors.CircleCi.BuildNumberVariable, "888" }

					} ),
					arguments: new[] { "--output", "out.props", "--assemblyFileVersion", "1.2.3" },
					expectedResult: ArgumentsParser.ParseResult.MissingBranch,
					expectedErrors: "One of [ APPVEYOR_REPO_BRANCH, CIRCLE_BRANCH, GITHUB_REF ] environment variables is required\r\n"
				);
		}

		[Test]
		[TestCaseSource( nameof( InvalidArgumentsTestCases ) )]
		public void TryParse_InvalidArguments(
				Func<string, string> environment,
				string[] arguments,
				ArgumentsParser.ParseResult expectedResult,
				string expectedErrors
			) {

			StringBuilder errors = new StringBuilder();
			using( StringWriter errorsWriter = new StringWriter( errors ) ) {

				ArgumentsParser.ParseResult result = ArgumentsParser.TryParse(
						environment,
						arguments,
						errorsWriter,
						out Arguments args
					);

				Assert.That( result, Is.EqualTo( expectedResult ), "Non success result expected" );
				Assert.That( errors.ToString(), Is.EqualTo( expectedErrors ), "Errors where expected" );
				Assert.That( args, Is.Null );
			}
		}

		private sealed class InvalidArgsTestCase : TestCaseData {

			public InvalidArgsTestCase(
					string name,
					Func<string, string> environment,
					string[] arguments,
					ArgumentsParser.ParseResult expectedResult,
					string expectedErrors
				) : base(
					environment,
					arguments,
					expectedResult,
					expectedErrors
				) {

				SetName( name );
			}
		}

		private static Func<string, string> MockEnvironment( Dictionary<string, string> values ) {

			return ( string variable ) => {
				if( values.TryGetValue( variable, out string value ) ) {
					return value;
				}
				return null;
			};
		}
	}
}
