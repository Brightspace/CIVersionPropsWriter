# CIVersionPropsWriter

Utility for generated Microsoft.NET.Sdk project version properties.

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

### In your CI build:

Setup environment variable:

```
  ASSEMBLY_FILE_VERSION: "0.1.0"
```

Generate the VersionInfo.props

```
CIVersionPropsWriter.exe --output VersionInfo.props
```
