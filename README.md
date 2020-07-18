# CIVersionPropsWriter

Utility for generated Microsoft.NET.Sdk project version properties.

Versioning:

* Dev Branch => 0.1.0-alpha2
* Master Branch => 0.1.0-rc3
* Release tag (v0.1.0) => 0.1.0

## Support

* Appveyor
* CircleCI
* Github Actions

## Usage

### In your `Directory.Build.props`:

```
<Project>
	<Import Project="VersionInfo.props" />
</Project>
```

With a developer file:

```
<Project>
	<PropertyGroup>
		<Version>0.0.0-dev</Version>
	</PropertyGroup>
</Project>
```

### In your CI build

Setup environment variable:

```
  ASSEMBLY_FILE_VERSION: "0.1.0"
```

Generate the VersionInfo.props

```
CIVersionPropsWriter.exe --output VersionInfo.props
```

## Sample Output

```
<Project>
	<PropertyGroup>
		<VersionPrefix>0.1.0</VersionPrefix>
		<VersionSuffix>alpha11</VersionSuffix>
		<Version>0.1.0-alpha11</Version>
		<AssemblyVersion>0.1.0.0</AssemblyVersion>
		<FileVersion>0.1.0.11</FileVersion>
		<InformationalVersion>0.1.0-alpha11+e7b8589f8b075430c4ad2a0b649c3117c2c7e9eb</InformationalVersion>
		<RepositoryCommit>e7b8589f8b075430c4ad2a0b649c3117c2c7e9eb</RepositoryCommit>
	</PropertyGroup>
</Project>
```
