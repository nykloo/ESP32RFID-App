using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESP32RFID
{
    static public class Constants
    {
        public static Dictionary<string, int> CrystalList = new Dictionary<string, int>() {
            {"Ashoka",0x18003000 },
            {"Chirutt",0x14403000 },

            {"Vader",0x5E003000 },
            {"Sidious",0x52403000 },
            {"Dooku",0x31403000 },
            {"Maul",0x46C03000 },
            {"Vader 8-Ball",0x3E183000 },

            {"Temple Guard",0x7B003000 },
            {"Maz",0x77403000 },

            {"Qui-Gon",0x0C803000 },
            {"Yoda",0x00C03000 },
            {"Yoda 8 Ball",0x5D183000 },

            {"Old Ben",0x29803000 },
            {"Old Luke",0x25C03000 },

            {"Mace 2",0x6F803000 },
            {"Mace",0x63C03000 },

            {"Snoke",0x1B183000 },

        };
    }
}
