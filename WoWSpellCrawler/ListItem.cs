using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWSpellCrawler
{
    /// <summary>
    /// 选择项类，用于ComboBox或者ListBox添加项
    /// </summary>
    class ListItem
    {
        private Professoion type = Professoion.None;
        private string name = string.Empty;

        public ListItem(Professoion type, string name)
        {
            this.type = type;
            this.name = name;
        }

        public override string ToString()
        {
            return this.name;
        }

        public Professoion Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
    }
}
