namespace CIVersionPropsWriter {

	internal static class Vendors {

		internal static class AppVeyor {

			internal const string BuildNumberVariable = "APPVEYOR_BUILD_NUMBER";
			internal const string BranchVariable = "APPVEYOR_REPO_BRANCH";
			internal const string TagVariable = "APPVEYOR_REPO_TAG_NAME";
		}

		internal static class CircleCi {

			internal const string BuildNumberVariable = "CIRCLE_BUILD_NUM";
			internal const string BranchVariable = "CIRCLE_BRANCH";
			internal const string TagVariable = "CIRCLE_TAG";
		}

		internal static class GithubActions {

			internal const string BuildNumberVariable = "GITHUB_RUN_NUMBER";
			internal const string RefVariable = "GITHUB_REF";
		}
	}
}
