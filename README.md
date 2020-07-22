# dotnet-ci-version-properties

.NET Core tool for generating project version properties within a CI environment.

Versioning:

* Dev Branch => 0.1.0-alpha.2
* Master Branch => 0.1.0-rc.3
* Release tag (v0.1.0) => 0.1.0

## Support

* Appveyor
* CircleCI
* Github Actions

## Usage

### In your repository

`Directory.Build.props`:

```
<Project>
	<Import Project="VersionInfo.props" />
</Project>
```

`VersionInfo.props`:

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
VERSION_PREFIX: "0.1.0"
```

Overwrite `VersionInfo.props`

```
dotnet tool install --global dotnet-ci-version-properties
dotnet ci-version-properties --output VersionInfo.props
```

## Sample Output

```
<Project>
	<PropertyGroup>
		<VersionPrefix>0.1.0</VersionPrefix>
		<VersionSuffix>alpha13</VersionSuffix>
		<Version>0.1.0-alpha13</Version>
		<AssemblyVersion>0.1.0.0</AssemblyVersion>
		<FileVersion>0.1.0.13</FileVersion>
		<InformationalVersion>0.1.0-alpha13+e3081f392c6af1b5f6c842afa8972c0e27bdb6ef</InformationalVersion>
		<RepositoryBranch>feature/xyz/RepositoryBranch>
		<RepositoryCommit>e3081f392c6af1b5f6c842afa8972c0e27bdb6ef</RepositoryCommit>
	</PropertyGroup>
</Project>
```
