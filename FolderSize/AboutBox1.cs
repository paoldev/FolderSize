using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace FolderSize
{
    partial class AboutBox1 : Form
    {
        public AboutBox1()
        {
            InitializeComponent();
            this.Text = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
            this.labelCopyright.Text = AssemblyCopyright + " - " + AssemblyCompany;
            this.textBoxDescription.Text = AssemblyDescription;

            string url = AssemblyProjectUrl;
            this.linkRepositoryUrl.Text = url;
            this.linkRepositoryUrl.LinkArea = new LinkArea(this.linkRepositoryUrl.Text.Length - url.Length, url.Length);
        }

        #region Assembly Attribute Accessors

        public static string AssemblyTitle
        {
            get
            {
                // This attribute requires the next entry in csproj
                // <ItemGroup>
                //   <AssemblyAttribute Include="System.Reflection.AssemblyMetadataAttribute">
                //      <_Parameter1>CustomTitle</_Parameter1>
                //      <_Parameter2>MyCustomTitle</_Parameter2>
                // </AssemblyAttribute>
                //</ItemGroup>
                var customTitle = GetCustomMetadataAttribute("CustomTitle");
                if (!string.IsNullOrWhiteSpace(customTitle))
                {
                    return customTitle;
                }

                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            }
        }

        public static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
            }
        }

        public static string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public static string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public static string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public static string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        public static string AssemblyRepositoryUrl
        {
            get
            {
                return GetCustomMetadataAttribute("RepositoryUrl");
            }
        }

        public static string AssemblyProjectUrl
        {
            get
            {
                // This attribute requires the next entry in csproj
                // <ItemGroup>
                //   <AssemblyAttribute Include="System.Reflection.AssemblyMetadataAttribute">
                //      <_Parameter1>ProjectUrl</_Parameter1>
                //      <_Parameter2>MyProjectUrl</_Parameter2>
                // </AssemblyAttribute>
                //</ItemGroup>
                return GetCustomMetadataAttribute("ProjectUrl");
            }
        }

        private static string GetCustomMetadataAttribute(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attributes = assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute), false);
            var customMetadataAttribute = attributes?.Cast<AssemblyMetadataAttribute>().FirstOrDefault(x => x.Key.Equals(name));
            return customMetadataAttribute?.Value ?? string.Empty;
        }

        #endregion

        private void OnokButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OnlinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Link != null)
            {
                var url = e.Link.LinkData?.ToString() ?? linkRepositoryUrl.Text.Substring(e.Link.Start, e.Link.Length);
                if (!string.IsNullOrEmpty(url))
                {
                    //This method doesn't work; see https://github.com/dotnet/runtime/issues/58069
                    //var startInfo = new System.Diagnostics.ProcessStartInfo(url);
                    //startInfo.UseShellExecute = true;
                    //System.Diagnostics.Process.Start(startInfo);
                    System.Diagnostics.Process.Start("explorer.exe", url);
                }
            }
        }
    }
}
