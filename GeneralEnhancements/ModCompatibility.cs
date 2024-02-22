using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralEnhancements
{
    public static class ModCompatibility
    {
        public static bool hasQSB { get; private set; }
        public static bool hasNH { get; private set; }
        public static bool hasCheatAndDebug { get; private set; }
        public static bool hasVisibleStranger { get; private set; }
        public static bool hasTimeSaver { get; private set; }

        public static void Initialize(ModMain mod)
        {
            bool Exists(string modID) => mod.ModHelper.Interaction.ModExists(modID);

            hasQSB = Exists("Raicuparta.QuantumSpaceBuddies");
            hasNH = Exists("xen.NewHorizons");
            hasCheatAndDebug = Exists("Glitch.AltDebugMenu");
            hasVisibleStranger = Exists("xen.Decloaked");
            hasTimeSaver = Exists("Bwc9876.TimeSaver");

            Log.Print($"{hasQSB} {hasNH} {hasCheatAndDebug} {hasVisibleStranger} {hasTimeSaver}");
        }
    }
}