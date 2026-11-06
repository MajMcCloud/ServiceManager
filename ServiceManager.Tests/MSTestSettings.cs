using Microsoft.VisualStudio.TestTools.UnitTesting;

// Parallelization is deliberately off. ConfigBase holds Filename, ConfigPath,
// BeautifyOnSave and AutoReloadOnChange in static fields, and the extension loader
// caches instances in a static dictionary, so tests must not run side by side.
[assembly: DoNotParallelize]
