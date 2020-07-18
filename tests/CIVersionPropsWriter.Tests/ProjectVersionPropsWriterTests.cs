using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace CIVersionPropsWriter.Tests {

	[TestFixture]
	internal sealed class ProjectVersionPropsWriterTests {

		[Test]
		public void FeatureBranch() {

			const string expectedProps = @"<Project>
	<PropertyGroup>
		<Version>10.6</Version>
		<FileVersion>10.6.8.93</FileVersion>
		<InformationalVersion>10.6.8-alpha93</InformationalVersion>
	</PropertyGroup>
</Project>
";
			AssertWrite(
					assemblyFileVersion: new Version( 10, 6, 8 ),
					branch: "feature/test",
					tag: string.Empty,
					build: 93,
					expectedProps: expectedProps
				);
		}

		[Test]
		public void MasterBranch() {

			const string expectedProps = @"<Project>
	<PropertyGroup>
		<Version>10.6</Version>
		<FileVersion>10.6.8.93</FileVersion>
		<InformationalVersion>10.6.8-rc93</InformationalVersion>
	</PropertyGroup>
</Project>
";
			AssertWrite(
					assemblyFileVersion: new Version( 10, 6, 8 ),
					branch: "master",
					tag: string.Empty,
					build: 93,
					expectedProps: expectedProps
				);
		}

		[Test]
		public void ReleaseTag() {

			const string expectedProps = @"<Project>
	<PropertyGroup>
		<Version>10.6</Version>
		<FileVersion>10.6.8.93</FileVersion>
		<InformationalVersion>10.6.8</InformationalVersion>
	</PropertyGroup>
</Project>
";
			AssertWrite(
					assemblyFileVersion: new Version( 10, 6, 8 ),
					branch: "master",
					tag: "v10.6.8",
					build: 93,
					expectedProps: expectedProps
				);
		}

		private static void AssertWrite(
				Version assemblyFileVersion,
				string branch,
				string tag,
				int build,
				string expectedProps
			) {

			StringBuilder sb = new StringBuilder();

			using( StringWriter strW = new StringWriter( sb ) ) {

				ProjectVersionPropsWriter.Write(
						output: strW,
						assemblyFileVersion: assemblyFileVersion,
						branch: branch,
						tag: tag,
						build: build
					);
			}

			string xml = sb.ToString();
			Assert.That( xml, Is.EqualTo( expectedProps ) );
		}
	}
}
