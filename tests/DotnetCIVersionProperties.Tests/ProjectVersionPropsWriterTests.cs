using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace DotnetCIVersionProperties.Tests {

	[TestFixture]
	internal sealed class ProjectVersionPropsWriterTests {

		[Test]
		public void FeatureBranch() {

			const string expectedProps = @"<Project>
	<PropertyGroup>
		<VersionPrefix>10.6.8</VersionPrefix>
		<VersionSuffix>alpha93</VersionSuffix>
		<Version>10.6.8-alpha93</Version>
		<AssemblyVersion>10.6.0.0</AssemblyVersion>
		<FileVersion>10.6.8.93</FileVersion>
		<InformationalVersion>10.6.8-alpha93+A94A8FE5CCB19BA61C4C0873D391E987982FBBD3</InformationalVersion>
		<RepositoryBranch>feature/test</RepositoryBranch>
		<RepositoryCommit>A94A8FE5CCB19BA61C4C0873D391E987982FBBD3</RepositoryCommit>
	</PropertyGroup>
</Project>
";
			AssertWrite(
					assemblyFileVersion: new Version( 10, 6, 8 ),
					branch: "feature/test",
					tag: string.Empty,
					build: 93,
					sha1: "A94A8FE5CCB19BA61C4C0873D391E987982FBBD3",
					expectedProps: expectedProps
				);
		}

		[Test]
		public void MasterBranch() {

			const string expectedProps = @"<Project>
	<PropertyGroup>
		<VersionPrefix>10.6.8</VersionPrefix>
		<VersionSuffix>rc93</VersionSuffix>
		<Version>10.6.8-rc93</Version>
		<AssemblyVersion>10.6.0.0</AssemblyVersion>
		<FileVersion>10.6.8.93</FileVersion>
		<InformationalVersion>10.6.8-rc93+A94A8FE5CCB19BA61C4C0873D391E987982FBBD3</InformationalVersion>
		<RepositoryBranch>master</RepositoryBranch>
		<RepositoryCommit>A94A8FE5CCB19BA61C4C0873D391E987982FBBD3</RepositoryCommit>
	</PropertyGroup>
</Project>
";
			AssertWrite(
					assemblyFileVersion: new Version( 10, 6, 8 ),
					branch: "master",
					tag: string.Empty,
					build: 93,
					sha1: "A94A8FE5CCB19BA61C4C0873D391E987982FBBD3",
					expectedProps: expectedProps
				);
		}

		[Test]
		public void ReleaseTag() {

			const string expectedProps = @"<Project>
	<PropertyGroup>
		<VersionPrefix>10.6.8</VersionPrefix>
		<VersionSuffix></VersionSuffix>
		<Version>10.6.8</Version>
		<AssemblyVersion>10.6.0.0</AssemblyVersion>
		<FileVersion>10.6.8.93</FileVersion>
		<InformationalVersion>10.6.8+A94A8FE5CCB19BA61C4C0873D391E987982FBBD3</InformationalVersion>
		<RepositoryBranch>master</RepositoryBranch>
		<RepositoryCommit>A94A8FE5CCB19BA61C4C0873D391E987982FBBD3</RepositoryCommit>
	</PropertyGroup>
</Project>
";
			AssertWrite(
					assemblyFileVersion: new Version( 10, 6, 8 ),
					branch: "master",
					tag: "v10.6.8",
					build: 93,
					sha1: "A94A8FE5CCB19BA61C4C0873D391E987982FBBD3",
					expectedProps: expectedProps
				);
		}

		private static void AssertWrite(
				Version assemblyFileVersion,
				string branch,
				string tag,
				int build,
				string sha1,
				string expectedProps
			) {

			StringBuilder sb = new StringBuilder();

			using( StringWriter strW = new StringWriter( sb ) ) {

				ProjectVersionPropsWriter.Write(
						output: strW,
						assemblyFileVersion: assemblyFileVersion,
						branch: branch,
						tag: tag,
						build: build,
						sha1: sha1
					);
			}

			string xml = sb.ToString();
			Assert.That( xml, Is.EqualTo( expectedProps ) );
		}
	}
}
