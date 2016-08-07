using System.IO;
using System.Reflection;

namespace Seemon.Todo.Utilities
{
    internal static class AppInfo
    {
        private static Assembly currentAssembly = null;

        public static string ApplicationRootPath;
        public static string LogFilePath;
        public static string PortableStoragePath = string.Empty;
        public static string DeleteMarker = string.Empty;

        public static string Title;
        public static string Product;
        public static string Version;
        public static string FullVersion;
        public static string Copyright;
        public static string Company;
        public static string Description;

        static AppInfo()
        {
            currentAssembly = Assembly.GetEntryAssembly();
            ApplicationRootPath = Path.GetDirectoryName(currentAssembly.Location);
            LogFilePath = Path.Combine(ApplicationRootPath, "Log.txt");

#if PORTABLE
            PortableStoragePath = Path.Combine(ApplicationRootPath, "blobs.db");
#endif

            if (currentAssembly != null)
            {
                AssemblyTitleAttribute title = currentAssembly.GetCustomAttribute<AssemblyTitleAttribute>();
                if (title != null)
                    Title = title.Title;

                AssemblyProductAttribute product = currentAssembly.GetCustomAttribute<AssemblyProductAttribute>();
                if (product != null)
                    Product = product.Product;

                AssemblyFileVersionAttribute version = currentAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
                if (version != null)
                    Version = version.Version;

                AssemblyInformationalVersionAttribute fullVersion = currentAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (fullVersion != null)
                    FullVersion = fullVersion.InformationalVersion;

                AssemblyCompanyAttribute company = currentAssembly.GetCustomAttribute<AssemblyCompanyAttribute>();
                if (company != null)
                    Company = company.Company;

                AssemblyCopyrightAttribute copyright = currentAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
                if (copyright != null)
                    Copyright = copyright.Copyright;

                AssemblyDescriptionAttribute description = currentAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
                if (description != null)
                    Description = description.Description;
            }
        }
    }
}
