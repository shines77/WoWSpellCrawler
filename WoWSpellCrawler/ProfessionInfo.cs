using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWSpellCrawler
{
    enum Professoion
    {
        None,
        All,

        Alchemy,
        BlackSmithing,
        Enchanting,
        Engineering,
        Herbalism,
        Leatherworking,
        Mining,
        Skinning,
        Tailoring,

        Cooking,
        FirstAids,
        Fishing,

        Maximnum
    }

    class ProfessionInfo
    {
        public int id = 0;
        public Professoion type = Professoion.None;
        public string name = "";

        public ProfessionInfo()
        {
        }
    }
}
