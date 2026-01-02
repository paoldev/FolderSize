//
// FolderSize
//
// Copyright (c) 2012-2026 paoldev
//
// Licensed under the MIT license.
// SPDX-License-Identifier: MIT
//

namespace FolderSize
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
