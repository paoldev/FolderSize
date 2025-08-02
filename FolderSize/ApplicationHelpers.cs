using System.Diagnostics;
using System.Security.Principal;

namespace FolderSize
{
    public static class ApplicationHelpers
    {
        /// <summary>
        /// Check if the application is running under Administrator credentials.
        /// </summary>
        /// <returns>Returns true if the application is running under Administrator credentials.</returns>
        public static bool IsAdministrator()
        {
            using var winIdentity = WindowsIdentity.GetCurrent();
            var winPrincipal = new WindowsPrincipal(winIdentity);
            return winPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Restart the application under Administrator credentials, using the same
        /// command line arguments of the calling application.
        /// </summary>
        /// <remarks>The calling application is always terminated.</remarks>
        public static void RestartAsAdministrator()
        {
            RestartAsAdministrator(Environment.GetCommandLineArgs()[1..]);
        }

        /// <summary>
        /// Restart the application under Administrator credentials, with specifid
        /// command line arguments.
        /// </summary>
        /// <param name="arguments">The command line arguments to pass to the application.</param>
        /// <remarks>
        /// The calling application is always terminated.
        /// Use 'arguments = Environment.GetCommandLineArgs()[1..]' to pass the arguments of the calling application.
        /// </remarks>
        public static void RestartAsAdministrator(IEnumerable<string> arguments)
        {
            ProcessStartInfo currentStartInfo = new(Application.ExecutablePath, arguments)
            {
                Verb = "runas",
                UseShellExecute = true
            };

            Application.Exit();
            try
            {
                // Operation may be cancelled by the user, in which case a Win32Exception is triggered.
                Process.Start(currentStartInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
