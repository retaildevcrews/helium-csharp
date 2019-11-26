using System;

namespace Helium
{
    /// <summary>
    /// Assembly Versioning
    /// </summary>
    public class Version
    {
        static string _version = string.Empty;

        public static string AssemblyVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_version))
                {
                    string file = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    DateTime dt = System.IO.File.GetCreationTime(file);

                    var aVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

                    _version = string.Format($"{aVer.Major}.{aVer.Minor}.{dt.ToString("MMdd.HHmm")}");
                }

                return _version;
            }
        }
    }
}