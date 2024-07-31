using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ImageEncryptCompress
{
     class Program
    {
        static long SEED = 834;
       static int sz=11, TAB=5;
        public static byte go_next()
        {
            byte ret = 0;
            for (int i = 4; i >= 0; i--)
            {
                long x = (SEED >> (sz - 1)) & 1L;
                long y = SEED >> TAB & 1L;
                SEED *= 2;
                SEED += x ^ y;
                ret += (byte)((x ^ y) << i);
            }
            return ret;
        }

        [STAThread]
        static void Main()
        {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
       
         }
        
    }
}