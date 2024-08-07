namespace CK.TypeScript.CodeGen
{
    /// <summary>
    /// Represent one of the dependencies list of the package.json.
    /// </summary>
    public enum DependencyKind
    {
        /// <summary>
        /// The dependency will be put in the package.json devDependencies list.
        /// </summary>
        DevDependency = 0,

        /// <summary>
        /// The dependency will be put in the package.json dependencies list.
        /// </summary>
        Dependency = 1,

        /// <summary>
        /// The dependency will be put in the package.json peerDependencies list.
        /// </summary>
        PeerDependency = 2
    }

    public static class DependencyKindExtensions
    {
        static readonly string[] _names = new[] { "devDependencies", "dependencies", "peerDependencies" };

        public static string GetJsonSectionName( this DependencyKind kind ) => _names[(int)kind];
    }
}
