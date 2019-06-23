using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Util;

namespace ProcessRepeatData
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 去重复
        /// </summary>
        private System.Timers.Timer timer = new System.Timers.Timer();

        /// <summary>
        /// 备份车辆在线数据
        /// </summary>
        private System.Timers.Timer timer1 = new System.Timers.Timer();

        private const int 秒 = 1000;
        private const int 分 = 秒 * 60;
        private const int 时 = 分 * 60;

        public Form1()
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var schedule = new Schedule();
            string time = DateTime.Now.ToString("yyyy-MM-dd");
            string hour = "23:30";
            string strHourTime = string.Format("{0} {1}", time, hour);

            schedule.Add(分 * 2, Convert.ToDateTime(strHourTime), delegate
             {
                 processRepeat();
             }).Add(时 * 24,
             Convert.ToDateTime(strHourTime), delegate
             {
                 processRepeat();
             }).Start();
        }

        /// <summary>
        /// 处理重复数据
        /// </summary>
        private void processRepeat()
        {
            string sql = @"select 内部编号,计划发车时刻,方向 from  tblRouteRFormD    
                           where 计划发车时刻>'2018-10-01'  
                           group by  内部编号,计划发车时刻,方向
                           having count(1)>1";
            DataTable repeatData = SqlHelper.ExecuteDataTable(sql);
            foreach (DataRow row in repeatData.Rows)
            {
                string excuteSql = "select 流水号 from  tblRouteRFormD where 内部编号='" + row["内部编号"].ToString() + "' and 计划发车时刻='" + Convert.ToDateTime(row["计划发车时刻"]).ToString("yyyy-MM-dd HH:mm:ss") + "' and 方向='" + row["方向"].ToString() + "'";
                DataTable msData = SqlHelper.ExecuteDataTable(excuteSql);
                if (msData.Rows.Count > 1)
                {
                    textBox1.Text = msData.Rows[0]["流水号"].ToString();
                    string deleteSql = "delete tblRouteRFormD where 流水号=" + msData.Rows[0]["流水号"].ToString();
                    SqlHelper.ExuteNonQuery(deleteSql);
                }
            }
        }

        private void bakOnline()
        {
            string sql = @"INSERT INTO [dbo].[tblOnlineVehicleRecording]
           ([日期]
           ,[内部编号]
           ,[最后通信时间])
             select 
              '" + DateTime.Now.ToString("yyyy-MM-dd") + @"',
             SRCID,
             max(UpdateTime) 通信日期 
             from  tblCommData20181211 where 命令字 = 65
             group by SRCID ";

            while (true)
            {  
                SqlHelper.ExuteNonQuery(sql);
            }
        }
    }
}
