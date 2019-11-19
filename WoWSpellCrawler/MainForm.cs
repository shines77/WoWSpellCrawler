using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WoWSpellCrawler
{
    public partial class MainForm : Form
    {
        private static ProfessionInfo[] professionInfos = {
            new ProfessionInfo { id = 0,   type = Professoion.All,            name = "所有专业" },
            new ProfessionInfo { id = 1,   type = Professoion.Alchemy,        name = "炼金术"   },
            new ProfessionInfo { id = 2,   type = Professoion.BlackSmithing,  name = "锻造"     },
            new ProfessionInfo { id = 3,   type = Professoion.Enchanting,     name = "附魔"     },
            new ProfessionInfo { id = 4,   type = Professoion.Engineering,    name = "工程学"   },
            new ProfessionInfo { id = 5,   type = Professoion.Herbalism,      name = "草药学"   },
            new ProfessionInfo { id = 6,   type = Professoion.Leatherworking, name = "制皮"     },
            new ProfessionInfo { id = 7,   type = Professoion.Mining,         name = "采矿"     },
            new ProfessionInfo { id = 8,   type = Professoion.Skinning,       name = "剥皮"     },
            new ProfessionInfo { id = 9,   type = Professoion.Tailoring,      name = "裁缝"     },
            new ProfessionInfo { id = 10,  type = Professoion.Cooking,        name = "烹饪"     },
            new ProfessionInfo { id = 11,  type = Professoion.FirstAids,      name = "急救"     },
            new ProfessionInfo { id = 12,  type = Professoion.Fishing,        name = "钓鱼"     },
        };

        public MainForm()
        {
            InitializeComponent();
            InitCert();
        }

        //
        // See: https://www.cnblogs.com/ccsharp/p/3270344.html
        //
        private void InitCert()
        {
            // WebService客户端代理类
            //WebService svc = new WebService();

            // 打开本地计算机下的个人证书存储区
            X509Store certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            certStore.Open(OpenFlags.ReadOnly);

            // 根据名称查找匹配的证书集合，这里注意最后一个参数，传true的话会找不到
            X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindBySubjectName, "PowerShell User", false);

            // 将证书添加至客户端证书集合
            //svc.ClientCertificates.Add(certCollection[0]);
        }

        private void AddProfessionInfosToList()
        {
            cbxProfessions.Items.Clear();

            int infoLength = professionInfos.Length;
            ListItem[] listItems = new ListItem[infoLength];
            for (int i = 0; i < infoLength; i++)
            {
                ProfessionInfo info = professionInfos[i];
                listItems[i] = new ListItem(info.type, info.name);
                cbxProfessions.Items.Add(listItems[i]);
            }

            cbxProfessions.SelectedIndex = 0;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Add profession infos to ComboBox list.
            AddProfessionInfosToList();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int index = cbxProfessions.SelectedIndex;
            if (index >= 0 && index < professionInfos.Length)
            {
                int success = 0, failed = 0;
                Professoion type = professionInfos[index].type;
                HttpWebCrawler webGether = new HttpWebCrawler();
                string html = webGether.Start(type, out success, out failed);
                rtbResult.AppendText(html + "\r\n");
            }
        }
    }
}
