﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpaceSimulator.PhysicsTest
{
    static class Program
    {
        /// <summary>
		/// The main entry point for the application.
		/// </summary>
		private static void Main()
        {
            using (var app = new PhysicsApp())
            {
                app.Run();
            }
        }
    }
}
