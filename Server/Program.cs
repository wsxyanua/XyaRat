using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Server.Forms;

namespace Server
{
    static class Program
    {

        /*
        * _    _            ______             
        * \ \  / /          (_____ \       _    
        *  \ \/ /_   _  ____ _____) ) ____| |_  
        *   )  (| | | |/ _  (_____ ( / _  |  _) 
        *  / /\ \ |_| ( ( | |     | ( ( | | |__ 
        * /_/  \_\__  |\_||_|     |_|\_||_|\___)
        *       (____/                          
        */
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            form1 = new Form1();
            Application.Run(form1);
        }
        public static Form1 form1;
    }
}

