using System.Runtime.CompilerServices;

// Allows internal DbSets to be lazy loaded
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

// Set internals visible for testing
[assembly: InternalsVisibleTo("Tests")]