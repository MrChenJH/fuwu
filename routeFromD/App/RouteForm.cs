using ClientRouteProcess;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Util;
using System.Configuration;

namespace WindowsRouteFormD
{

    public partial class RouteForm : Form
    {



        public RouteForm()
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

        }

        private Dictionary<string, string> routeDicWhere = new Dictionary<string, string>();

        public delegate void ControlAddEx(string progressText);
        public delegate void ControlAddR(string progressText);
        public ControlAddEx E;
        public ControlAddR R;
        public void AddR(string progressText)
        {

            textBox2.Text = progressText;
        }




        public void AddEx(string progressText)
        {
            textBox1.AppendText("\\n\\t" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\\n\\t" + progressText);

        }

        public Dictionary<string, plan> _dics = new Dictionary<string, plan>();

        public class plan
        {
            public string planTime { get; set; }

            public string arrivedPlanTime { get; set; }
        }


        /// <summary>
        /// 出场日期
        /// </summary>
        private string _NonFormDCCdateTime = string.Empty;

        /// <summary>
        /// 进场日期
        /// </summary>
        private string _NonFormDJCdateTime = string.Empty;

        public void Dow()
        {

            string sql = @"	SELECT b.stName, CONVERT(varchar,a.计划发车时间,108) 计划发车时间 ,CONVERT(varchar,a.实际发车,108)  实际发车
			,datediff(minute, CAST( a.计划发车时间 as time) ,CAST( a.实际发车 as time)) AS 异常时间
			, '' AS 简要说明, a.ActualRoute
		FROM tbldailyplan1 a
			INNER JOIN tblAbridge b
			ON a.ActualRoute = b.routeId
				AND a.始发 = b.abridge
					inner join 	 QzjcRoute   qz on  a.ActualRoute=qz.标准线路编码 and b.stName=qz.末站名称
		WHERE 早晚班 = '2'
			AND 日期 = '" + Convert.ToDateTime(日期).AddDays(-1).ToString("yyyy-MM-dd") + "'";
            DataTable sqlData = SqlHelper.ExecuteDataTable(sql);
            for (int i = 0; i < sqlData.Rows.Count; i++)
            {
                string jhf = Convert.ToString(sqlData.Rows[i]["计划发车时间"]);
                string zd = Convert.ToString(sqlData.Rows[i]["stName"]);
                string xlid = Convert.ToString(sqlData.Rows[i]["ActualRoute"]);
                sql = "select * from tbldailyplan1 where 日期 = '" + Convert.ToDateTime(日期).AddDays(-1).ToString("yyyy-MM-dd") + "' and  convert(varchar,计划发车时间,108)='" + jhf + "' and  早晚班 = '2' AND ActualRoute='" + xlid + "'";

                var existData = SqlHelper.ExecuteDataTable(sql);
                if (existData.Rows.Count > 0)
                {

                    sql = " select top 1 convert(varchar, 到达时刻,108) 到达时刻,convert(varchar, 驶离时刻,108) 驶离时刻  from tblRouteRFormX" + Convert.ToDateTime(日期).AddDays(-1).ToString("yyyyMMdd") + "  where 内部编号 =  '" + Convert.ToString(existData.Rows[0]["内部编号"]) + "'  and 站点名称 = '" + zd + "' order by 流水号 desc";
                    var existzdData = SqlHelper.ExecuteDataTable(sql);
                    if (existzdData.Rows.Count > 0)
                    {
                        sql = "update tbldailyplan1 set 实际发车='" + Convert.ToString(existzdData.Rows[0]["驶离时刻"]) + "',实际到达='" + Convert.ToString(existzdData.Rows[0]["到达时刻"]) + "' where 流水号=" + Convert.ToString(existData.Rows[0]["流水号"]);
                        SqlHelper.ExuteNonQuery(sql);
                    }
                }

            }
        }

        public void Doss()
        {

            string sql = @"SELECT b.stName, convert(varchar,a.计划发车时间,108) 计划发车时间, a.实际发车,datediff(minute, a.实际发车, a.计划发车时间) AS 异常时间,a.ActualRoute
                             FROM tbldailyplan1 a INNER JOIN tblAbridge b ON a.ActualRoute = b.routeId AND a.始发 = b.abridge inner join    QzjcRoute qz on a.ActualRoute = qz.标准线路编码 and b.stName = qz.首站名称
                             WHERE 早晚班 = '2' AND 日期 = '" + Convert.ToDateTime(日期).AddDays(-1).ToString("yyyy-MM-dd") + "'";
            DataTable sqlData = SqlHelper.ExecuteDataTable(sql);
            for (int i = 0; i < sqlData.Rows.Count; i++)
            {
                string jhf = Convert.ToString(sqlData.Rows[i]["计划发车时间"]);
                string zd = Convert.ToString(sqlData.Rows[i]["stName"]);
                string xlid = Convert.ToString(sqlData.Rows[i]["ActualRoute"]);
                sql = "select * from tbldailyplan1 where 日期 = '" + Convert.ToDateTime(日期).AddDays(-1).ToString("yyyy-MM-dd") + "' and  convert(varchar,计划发车时间,108)='" + jhf + "' and  早晚班 = '2' AND ActualRoute='" + xlid + "'";

                var existData = SqlHelper.ExecuteDataTable(sql);
                if (existData.Rows.Count > 0)
                {

                    sql = " select top 1 convert(varchar, 到达时刻,108) 到达时刻,convert(varchar, 驶离时刻,108) 驶离时刻  from tblRouteRFormX" + Convert.ToDateTime(日期).AddDays(-1).ToString("yyyyMMdd") + "  where 内部编号 =  '" + Convert.ToString(existData.Rows[0]["内部编号"]) + "'  and 站点名称 = '" + zd + "' order by 流水号 desc";
                    var existzdData = SqlHelper.ExecuteDataTable(sql);
                    if (existzdData.Rows.Count > 0)
                    {
                        sql = "update tbldailyplan1 set 实际发车='" + Convert.ToString(existzdData.Rows[0]["驶离时刻"]) + "',实际到达='" + Convert.ToString(existzdData.Rows[0]["到达时刻"]) + "' where 流水号=" + Convert.ToString(existData.Rows[0]["流水号"]);
                        SqlHelper.ExuteNonQuery(sql);
                    }
                }

            }


        }

        private void RunJsy()
        {

            string sql = "select 内部编号,convert(varchar(19), min(计划发车时刻),121)  计划发车时刻 from tblRouteRFormD where isnull(驾驶员,'')='' and 日期='" + 日期 + "' group by 内部编号";
            DataTable data = SqlHelper.ExecuteDataTable(sql);
            for (var i = 0; i < data.Rows.Count; i++)
            {
                string 内部编号 = Convert.ToString(data.Rows[i]["内部编号"]);
                string 计划发车时刻 = Convert.ToString(data.Rows[i]["计划发车时刻"]);
                string jsysql = "select top 1 workName from WorkCardRecord where 记录日期='" + 日期 + "' and CardCode='" + 内部编号 + "' order by 记录时刻";
                DataTable jsysqldata = SqlHelper.ExecuteDataTable(jsysql);
                if (jsysqldata.Rows.Count > 0)
                {
                    string 第四步 = "update tblRouteRFormD " +
                  "                     set 驾驶员 = '" + Convert.ToString(jsysqldata.Rows[0]["workName"]) + "'" +
                  "             where  内部编号='" + 内部编号 + "' and 计划发车时刻='" + 计划发车时刻 + "'";
                    SqlHelper.ExuteNonQuery(第四步);
                }
                else
                {
                    continue;
                }
            }


        }




        public class SxData
        {
            public string routeId { get; set; }

            public string stName { get; set; }

            public string abridge { get; set; }

        }

        public string 日期 = "2019-08-15";

        /// <summary>
        /// 处理数据
        /// </summary>
        private void ProcessXXX()
        {
            string formdSql = "select *,convert(varchar,计划到达时刻,108) 计划到,起点,终点,线路ID,convert(varchar,计划到达时刻,108) 计划到,convert(varchar,计划发车时刻,108) 计划发车    from tblRouteRFormD where 类型 in('进场', '出场', '区间') and 日期 ='" + 日期 + "' ";
            var data = SqlHelper.ExecuteDataTable(formdSql);
            for (int i = 0; i < data.Rows.Count; i++)
            {
                string 计划发车 = Convert.ToString(data.Rows[i]["计划发车"]);
                string 类型 = Convert.ToString(data.Rows[i]["类型"]);
                string 内部编号 = Convert.ToString(data.Rows[i]["内部编号"]);
                string 起点 = Convert.ToString(data.Rows[i]["起点"]);
                string 终点 = Convert.ToString(data.Rows[i]["终点"]);
                string 线路ID = Convert.ToString(data.Rows[i]["线路ID"]);
                //if (类型.Equals("出场"))
                //{
                //    string zd = Convert.ToString(data.Rows[i]["终点"]);
                //    string jhd = Convert.ToString(data.Rows[i]["计划到"]);
                //    string cc = "	select top 1 name,CONVERT(varchar,时刻,108) 时刻 from tblWeilanDetails where 日期='" + 日期 + "' and buscode='" + 内部编号 + "' and type='停车场' order by 时刻";
                //    DataTable ccD = SqlHelper.ExecuteDataTable(cc);
                //    if (ccD.Rows.Count > 0)
                //    {
                //        string names = Convert.ToString(ccD.Rows[0]["name"]);
                //        string[] ns = names.Split(' ');

                //        string upDateSQL = "UPDATE tblRouteRFormD SET  状态=1,起点='" + ns.Last() + "',实际发车时刻='" + string.Format("{0} {1}", 日期, Convert.ToString(ccD.Rows[0]["时刻"])) + "'     where 流水号='" + Convert.ToString(data.Rows[i]["流水号"]) + "'";
                //        SqlHelper.ExuteNonQuery(upDateSQL);

                //        string updayilSql = @"update a set a.实际发车='" + string.Format("{0} {1}", 日期, Convert.ToString(ccD.Rows[0]["时刻"])) + @"',a.实际到达='" + string.Format("{0} {1}", 日期, Convert.ToString(ccD.Rows[0]["时刻"])) + @"'   from tbldailyplan1  a
                //                             left join  tblRouteRFormD b on a.日期 = b.日期 and a.内部编号 = b.内部编号 and cast(a.计划发车时间 as time )= cast(b.计划发车时刻 as time)
                //                             where b.流水号 = " + Convert.ToString(data.Rows[i]["流水号"]);

                //        SqlHelper.ExuteNonQuery(updayilSql);

                //    }
                //}
                //if (类型.Equals("进场"))
                //{
                //    string jhd = Convert.ToString(data.Rows[i]["计划到"]);
                //    string cc = "	select top 1 name,convert(varchar, 时刻,108) 时刻 from tblWeilanDetails where 日期='" + 日期 + "' and buscode='" + 内部编号 + "'  and type='停车场' order by 时刻 desc";
                //    DataTable ccD = SqlHelper.ExecuteDataTable(cc);
                //    if (ccD.Rows.Count > 0)
                //    {
                //        string names = Convert.ToString(ccD.Rows[0]["name"]);
                //        string[] ns = names.Split(' ');
                //        string tblAsql = "select 里程 from tblAppearancesData where 开出站名 in( '" + Convert.ToString(data.Rows[i]["起点"]) + "','" + Convert.ToString(data.Rows[i]["终点"]) + "') and 到达站名 in( '" + Convert.ToString(data.Rows[i]["起点"]) + "','" + Convert.ToString(data.Rows[i]["终点"]) + "')";
                //        var tblData = SqlHelper.ExecuteDataTable(tblAsql);

                //        string lc = "0";
                //        string upDateSQL = "UPDATE tblRouteRFormD SET  终点='" + ns.Last() + "',实际到达时刻='" + string.Format("{0} {1}", 日期, ccD.Rows[0]["时刻"]) + "'   where 流水号='" + Convert.ToString(data.Rows[i]["流水号"]) + "'";
                //        SqlHelper.ExuteNonQuery(upDateSQL);
                //        if (tblData.Rows.Count > 0)
                //        {
                //            lc = Convert.ToString(tblData.Rows[0]["里程"]);
                //            upDateSQL = "UPDATE tblRouteRFormD SET 单程里程='" + lc + "',状态=2   where 流水号='" + Convert.ToString(data.Rows[i]["流水号"]) + "'";
                //            SqlHelper.ExuteNonQuery(upDateSQL);
                //        }

                //    }
                //}
                if (类型.Equals("区间"))
                {
                    string zd = Convert.ToString(data.Rows[i]["终点"]);
                    string jhd = Convert.ToString(data.Rows[i]["计划到"]);
                    string selectSql = "select * from tblRouteD where 站名='" + 起点 + "' and 线路ID='" + 线路ID + "'";
                    DataTable table = SqlHelper.ExecuteDataTable(selectSql);
                    if (table.Rows.Count == 0)
                    {

                        string cc = "	select top 1 name,CONVERT(varchar,时刻,108) 时刻 from tblWeilanDetails where 日期='" + 日期 + "' and buscode='" + 内部编号 + "' and name like '%" + 起点 + "%' and type='停车场' order by 时刻";
                        DataTable ccD = SqlHelper.ExecuteDataTable(cc);
                        //if (Convert.ToString(ccD.Rows[0]["name"]).Equals(起点))
                        //{
                        if (ccD.Rows.Count > 0)
                        {
                            string sjf = string.Format("{0} {1}", 日期, Convert.ToString(ccD.Rows[0]["时刻"]));
                            string upDateSQL = "UPDATE tblRouteRFormD SET  实际发车时刻='" + sjf + "',状态=1   where 流水号='" + Convert.ToString(data.Rows[i]["流水号"]) + "'";
                            SqlHelper.ExuteNonQuery(upDateSQL);
                            string updayilSql = @"update a set a.实际发车='" + string.Format("{0} {1}", 日期, Convert.ToString(ccD.Rows[0]["时刻"])) + @"',a.实际到达='" + string.Format("{0} {1}", 日期, Convert.ToString(ccD.Rows[0]["时刻"])) + @"'   from tbldailyplan1  a
                                             left join  tblRouteRFormD b on a.日期 = b.日期 and a.内部编号 = b.内部编号 and cast(a.计划发车时间 as time )= cast(b.计划发车时刻 as time)
                                             where b.流水号 = " + Convert.ToString(data.Rows[i]["流水号"]);

                            SqlHelper.ExuteNonQuery(updayilSql);
                        }
                        string rFormXsql1 = "select top 1 CONVERT(varchar,到达时刻,108) 到达时刻,CONVERT(varchar,驶离时刻,108)  驶离时刻 from tblRouteRFormX" + Convert.ToDateTime(日期).ToString("yyyyMMdd") + "  where 站点名称='" + 终点 + "' and 到达时刻>'" + string.Format("{0} {1}", 日期, 计划发车) + @"' order by 流水号";
                        DataTable data2 = SqlHelper.ExecuteDataTable(rFormXsql1);
                        if (data2.Rows.Count > 0)
                        {
                            string upDateSQL = "UPDATE tblRouteRFormD SET  实际到达时刻='" + string.Format("{0} {1}", 日期, Convert.ToString(data2.Rows[0]["到达时刻"])) + @"',状态=1   where 流水号='" + Convert.ToString(data.Rows[i]["流水号"]) + "'";
                            SqlHelper.ExuteNonQuery(upDateSQL);

                        }

                        //}
                    }
                    else
                    {
                        string rFormXsql = "select top 1 CONVERT(varchar,到达时刻,108) 到达时刻,CONVERT(varchar,驶离时刻,108)  驶离时刻 from tblRouteRFormX" + Convert.ToDateTime(日期).ToString("yyyyMMdd") + "  where 站点名称='" + 起点 + "'    order by 流水号";
                        DataTable data1 = SqlHelper.ExecuteDataTable(rFormXsql);
                        if (data1.Rows.Count > 0)
                        {
                            string upDateSQL = "UPDATE tblRouteRFormD SET  实际发车时刻='" + string.Format("{0} {1}", 日期, Convert.ToString(data1.Rows[0]["驶离时刻"])) + @"',状态=1   where 流水号='" + Convert.ToString(data.Rows[i]["流水号"]) + "'";
                            SqlHelper.ExuteNonQuery(upDateSQL);
                            string updayilSql = @"update a set a.实际发车='" + string.Format("{0} {1}", 日期, Convert.ToString(data1.Rows[0]["驶离时刻"])) + @"',a.实际到达='" + string.Format("{0} {1}", 日期, Convert.ToString(data1.Rows[0]["到达时刻"])) + @"'   from tbldailyplan1  a
                                             left join  tblRouteRFormD b on a.日期 = b.日期 and a.内部编号 = b.内部编号 and cast(a.计划发车时间 as time )= cast(b.计划发车时刻 as time)
                                             where b.流水号 = " + Convert.ToString(data.Rows[i]["流水号"]);

                            SqlHelper.ExuteNonQuery(updayilSql);
                        }
                        string rFormXsql1 = "select top 1 CONVERT(varchar,到达时刻,108) 到达时刻,CONVERT(varchar,驶离时刻,108)  驶离时刻 from tblRouteRFormX" + Convert.ToDateTime(日期).ToString("yyyyMMdd") + "  where 站点名称='" + 终点 + "'  and 到达时刻>'" + string.Format("{0} {1}", 日期, 计划发车) + @"' order by 流水号";
                        DataTable data2 = SqlHelper.ExecuteDataTable(rFormXsql1);
                        if (data2.Rows.Count > 0)
                        {
                            string upDateSQL = "UPDATE tblRouteRFormD SET  实际到达时刻='" + string.Format("{0} {1}", 日期, Convert.ToString(data2.Rows[0]["到达时刻"])) + @"',状态=1  where 流水号='" + Convert.ToString(data.Rows[i]["流水号"]) + "'";
                            SqlHelper.ExuteNonQuery(upDateSQL);

                        }

                    }

                
                }
            }
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private void ProcessData()
        {
            try
            {
                List<SxData> sxList = new List<SxData>();

                string sxSql = " select routeId,stName,abridge from tblAbridge ";
                DataTable sxData = SqlHelper.ExecuteDataTable(sxSql);
                for (int i = 0; i < sxData.Rows.Count; i++)
                {
                    SxData sx = new SxData();
                    sx.routeId = Convert.ToString(sxData.Rows[i]["routeId"]);
                    sx.stName = Convert.ToString(sxData.Rows[i]["stName"]);
                    sx.abridge = Convert.ToString(sxData.Rows[i]["abridge"]);
                    sxList.Add(sx);
                }

                string Rsql = "select * from tblroute";
                DataTable data222 = SqlHelper.ExecuteDataTable(Rsql);
                for (int i = 0; i < data222.Rows.Count; i++)
                {
                    string dataSql = "select 站名 from tblrouted where (上行始发=1 or 上行终点=1 or 下行终点=1 or 下行始发=1) and 线路ID='" + Convert.ToString(data222.Rows[i]["线路ID"]) + "' order by 线路ID ";

                    DataTable data111 = SqlHelper.ExecuteDataTable(dataSql);


                    List<string> strs = new List<string>();
                    for (int j = 0; j < data111.Rows.Count; j++)
                    {
                        strs.Add(Convert.ToString(data111.Rows[j]["站名"]));
                    }

                    routeDicWhere.Add(Convert.ToString(data222.Rows[i]["线路ID"]), "'" + string.Join("','", strs) + "'");
                }


                ///班次
                string sql = @"select
             [流水号]
            ,[自编号]
           ,[内部编号]
           ,[驾驶员]
           ,[驾驶员编号]
           ,[售票员]
           ,[售票员编号]
           ,[路牌]
           ,[方向]
           ,CONVERT(varchar,[计划发车时间],108)  计划发车时间
           ,[计划到达时间]
           ,[预估发车时间]
           ,[调整时刻编辑时间]
           ,[调整时刻编辑人]
           ,[编辑人]
           ,[编辑时间]
           ,[替换车号]
           ,[替换驾驶员]
           ,[ActualRoute]
           ,[实际发车]
           ,[实际到达]
           ,[是否进出场]
           ,[是否发车]
           ,[替换售票员]
           ,[始发]
           ,[类型]
           ,[早晚班]
           ,[是否起点]
           ,[进出场类型] from tbldailyplan1 where 日期='" + 日期 + "'    and 类型 in ('进场','出场','区间')";

                DataTable data = SqlHelper.ExecuteDataTable(sql);


                for (int i = 0; i < data.Rows.Count; i++)
                {
                    var fcsj = 日期 + " " + Convert.ToString(data.Rows[i]["计划发车时间"]);

                    if (Convert.ToDateTime(fcsj) < DateTime.Now)
                    {

                        ///路单
                        string stringFromsql = " select * from tblRouteRFormD where 内部编号='" + Convert.ToString(data.Rows[i]["替换车号"]) + "'  and CONVERT(varchar,计划发车时刻,108)='" + Convert.ToString(data.Rows[i]["计划发车时间"]) + "' ";
                        DataTable dataxxxx = SqlHelper.ExecuteDataTable(stringFromsql);
                        if (dataxxxx.Rows.Count == 0)
                        {
                            string dlzsql = " select * from tblRouteRFormX" + Convert.ToDateTime(日期).ToString("yyyyMMdd") + " where 内部编号='" + Convert.ToString(data.Rows[i]["替换车号"]) + "' ";
                            DataTable dlzData = SqlHelper.ExecuteDataTable(dlzsql);
                            if (dlzData.Rows.Count > 0)
                            {
                                if (Convert.ToString(data.Rows[i]["类型"]).Equals("区间"))
                                {
                                    ///路单
                                    string InsertRouteForm = "INSERT INTO tblRouteRFormD(日期,路单编号,线路ID,内部编号,行号,方向,起点,终点,驾驶员,乘务员,"
                                                           + "计划发车时刻,实际发车时刻,计划到达时刻,实际到达时刻,校时点快慢,快慢,单程里程,备注,是否确认,编辑人,最小单程限时,单程限时,发车间隔,趟次,描述,驾驶员工号,乘务员工号,路牌,类型) "
                                                           + "(select '{0}', '{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}',"
                                                           + "cast('{10}' as smalldatetime),cast('{11}' as smalldatetime),cast('{12}' as smalldatetime),cast('{13}'as smalldatetime ),"
                                                           + "'{14}','{15}','{16}','{17}','True' ,'{18}',{19},{20},{21},'{22}','{23}','{24}','{25}','{26}','{27}' )";
                                    var sj = new Random(100).Next();
                                    string[] parStrs = new string[] {
                                             日期,
                                             Convert.ToString(sj),
                                             Convert.ToString(data.Rows[i]["ActualRoute"]) ,
                                             Convert.ToString(data.Rows[i]["替换车号"]),
                                            "1",
                                            Convert.ToString(data.Rows[i]["类型"]),
                                            sxList.FirstOrDefault(t=>t.routeId==Convert.ToString(data.Rows[i]["ActualRoute"])&t.abridge==Convert.ToString(data.Rows[i]["始发"])).stName,
                                            sxList.FirstOrDefault(t=>t.routeId==Convert.ToString(data.Rows[i]["ActualRoute"])&t.abridge==Convert.ToString(data.Rows[i]["方向"])).stName,
                                            Convert.ToString(data.Rows[i]["驾驶员"]) ,
                                            Convert.ToString(data.Rows[i]["售票员"]) ,
                                            fcsj,
                                            DateTime.Now.ToString("yyyy-MM-dd"),
                                            DateTime.Now.ToString("yyyy-MM-dd"),
                                            DateTime.Now.ToString("yyyy-MM-dd"),
                                           "0",
                                           "0",
                                           "0",
                                           "255",
                                           "system",
                                           "0",
                                           "0",
                                           "0",
                                           "1",
                                            "",
                                            Convert.ToString(data.Rows[i]["驾驶员编号"]) ,
                                            Convert.ToString(data.Rows[i]["售票员编号"]) ,
                                            Convert.ToString(data.Rows[i]["路牌"]),
                                           "区间"
                                           };
                                    string sqlStr = string.Format(InsertRouteForm, parStrs);
                                    SqlHelper.ExuteNonQuery(sqlStr);
                                }
                                if (Convert.ToString(data.Rows[i]["类型"]).Equals("出场"))
                                {

                                    ///作业
                                    string ccsql = @"select 始发,CONVERT(varchar,[计划发车时间],108)  计划发车时间  from tbldailyplan1 where 日期='" + 日期 + "'  and 替换车号='" + Convert.ToString(data.Rows[i]["替换车号"]) + "' and 是否进出场=0  order by 计划发车时间";

                                    DataTable ccData = SqlHelper.ExecuteDataTable(ccsql);
                                    string zd = string.Empty;
                                    string jhdd = 日期;
                                    if (ccData.Rows.Count > 0)
                                    {
                                        zd = sxList.FirstOrDefault(t => t.routeId == Convert.ToString(data.Rows[i]["ActualRoute"]) & t.abridge == Convert.ToString(ccData.Rows[0]["始发"])).stName;
                                        jhdd = 日期 + " " + Convert.ToString(ccData.Rows[0]["计划发车时间"]);
                                    }


                                    string InsertRouteForm = "INSERT INTO tblRouteRFormD(日期,路单编号,线路ID,内部编号,行号,方向,起点,终点,驾驶员,乘务员,"
                                                           + "计划发车时刻,实际发车时刻,计划到达时刻,实际到达时刻,校时点快慢,快慢,单程里程,备注,是否确认,编辑人,最小单程限时,单程限时,发车间隔,趟次,描述,驾驶员工号,乘务员工号,路牌,类型) "
                                                           + "(select '{0}', '{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}',"
                                                           + "cast('{10}' as smalldatetime),cast('{11}' as smalldatetime),cast('{12}' as smalldatetime),cast('{13}'as smalldatetime ),"
                                                           + "'{14}','{15}','{16}','{17}','True' ,'{18}',{19},{20},{21},'{22}','{23}','{24}','{25}','{26}','{27}' )";
                                    var sj = new Random(100).Next();
                                    string[] parStrs = new string[] {
                                             日期,
                                             Convert.ToString(sj),
                                             Convert.ToString(data.Rows[i]["ActualRoute"]) ,
                                             Convert.ToString(data.Rows[i]["替换车号"]),
                                            "1",
                                            Convert.ToString(data.Rows[i]["类型"]),
                                            "",
                                            zd,
                                            Convert.ToString(data.Rows[i]["驾驶员"]) ,
                                            Convert.ToString(data.Rows[i]["售票员"]) ,
                                            fcsj,
                                            DateTime.Now.ToString("yyyy-MM-dd"),
                                           jhdd,
                                            DateTime.Now.ToString("yyyy-MM-dd"),
                                           "0",
                                           "0",
                                           "0",
                                           "255",
                                           "system",
                                           "0",
                                           "0",
                                           "0",
                                           "1",
                                            "",
                                            Convert.ToString(data.Rows[i]["驾驶员编号"]) ,
                                            Convert.ToString(data.Rows[i]["售票员编号"]) ,
                                            Convert.ToString(data.Rows[i]["路牌"]),
                                           "出场"
                                           };
                                    string sqlStr = string.Format(InsertRouteForm, parStrs);
                                    SqlHelper.ExuteNonQuery(sqlStr);
                                }
                                if (Convert.ToString(data.Rows[i]["类型"]).Equals("进场"))
                                {

                                    string ccsql = @"select top 1 方向  from tbldailyplan1 where 日期='" + 日期 + "'  and 替换车号='" + Convert.ToString(data.Rows[i]["替换车号"]) + "' and 是否进出场=0 order by 计划发车时间 desc";
                                    DataTable ccData = SqlHelper.ExecuteDataTable(ccsql);
                                    string sf = string.Empty;
                                    if (ccData.Rows.Count > 0)
                                    {
                                        sf = sxList.FirstOrDefault(t => t.routeId == Convert.ToString(data.Rows[i]["ActualRoute"]) & t.abridge == Convert.ToString(ccData.Rows[0]["方向"])).stName;
                                    }

                                    string dlzsql11 = "select top 1 * from tblRouteRFormX" + Convert.ToDateTime(日期).ToString("yyyyMMdd") + " where 内部编号='" + Convert.ToString(data.Rows[i]["替换车号"]) + "' and   站点名称 in (" + routeDicWhere[Convert.ToString(data.Rows[i]["ActualRoute"])] + ")   order by 流水号 desc  ";
                                    DataTable dlzsql11Data = SqlHelper.ExecuteDataTable(dlzsql11);
                                    string updateSql = "update tbldailyplan1 set 实际发车='" + Convert.ToString(dlzsql11Data.Rows[0]["驶离时刻"]) + "',实际到达='" + Convert.ToString(dlzsql11Data.Rows[0]["到达时刻"]) + "', where 流水号='" + Convert.ToString(data.Rows[0]["流水号"]) + "'";
                                    SqlHelper.ExuteNonQuery(dlzsql11);
                                    string InsertRouteForm = "INSERT INTO tblRouteRFormD(日期,路单编号,线路ID,内部编号,行号,方向,起点,终点,驾驶员,乘务员,"
                                                           + "计划发车时刻,实际发车时刻,计划到达时刻,实际到达时刻,校时点快慢,快慢,单程里程,备注,是否确认,编辑人,最小单程限时,单程限时,发车间隔,趟次,描述,驾驶员工号,乘务员工号,路牌,类型) "
                                                           + "(select '{0}', '{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}',"
                                                           + "cast('{10}' as smalldatetime),cast('{11}' as smalldatetime),cast('{12}' as smalldatetime),cast('{13}'as smalldatetime ),"
                                                           + "'{14}','{15}','{16}','{17}','True' ,'{18}',{19},{20},{21},'{22}','{23}','{24}','{25}','{26}','{27}')";

                                    var sj = new Random(100).Next();
                                    string[] parStrs = new string[] {
                                             日期,
                                             Convert.ToString(sj),
                                             Convert.ToString(data.Rows[i]["ActualRoute"]) ,
                                             Convert.ToString(data.Rows[i]["替换车号"]),
                                            "1",
                                            Convert.ToString(data.Rows[i]["类型"]),
                                            sf,
                                            "",
                                            Convert.ToString(data.Rows[i]["驾驶员"]) ,
                                            Convert.ToString(data.Rows[i]["售票员"]) ,
                                            fcsj,
                                            Convert.ToString(dlzsql11Data.Rows[0]["驶离时刻"]),
                                            DateTime.Now.ToString("yyyy-MM-dd"),
                                            DateTime.Now.ToString("yyyy-MM-dd"),
                                           "0",
                                           "0",
                                           "0",
                                           "255",
                                           "system",
                                           "0",
                                           "0",
                                           "0",
                                           "1",
                                            "",
                                            Convert.ToString(data.Rows[i]["驾驶员编号"]) ,
                                            Convert.ToString(data.Rows[i]["售票员编号"]) ,
                                            Convert.ToString(data.Rows[i]["路牌"]),
                                           "进场"
                                           };
                                    string sqlStr = string.Format(InsertRouteForm, parStrs);
                                    SqlHelper.ExuteNonQuery(sqlStr);
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("异常:" + ex.Message + "  sql:" + ex.Message + " _dateTime:" + _dateTime.ToString("yyyy-MM-dd") + "  now" + 日期);
            }
        }

        /// <summary>
        /// 日期
        /// </summary>
        private DateTime _dateTime = new DateTime();


        private DateTime _cTime = new DateTime();
        public class Route
        {
            public string 线路 { get; set; }

            public string 方向 { get; set; }

            public string 长度 { get; set; }
        }

        public class RouteSRange
        {
            public string 线路 { get; set; }
            public int 线路内序号 { get; set; }
            public string 类型 { get; set; }

            public string 站名 { get; set; }
        }

        public List<RouteSRange> _routeSRangeS = new List<RouteSRange>();

        public List<Route> _routeS = new List<Route>();

        private void Form1_Load(object sender, EventArgs e)
        {

            E = new ControlAddEx(AddR);

            R = new ControlAddR(AddR);

            _routeS = (from p in SqlHelper.ExecuteDataTable(@"select 线路ID,'上行' as 方向,ISNULL(CAST(上行长度 as varchar),'0')  as 长度 from tblRoute  
                                                                  union all 
                                                                 select 线路ID,'下行' as 方向,ISNULL(CAST(下行长度 as varchar),'0')  as 长度 from tblRoute    ").AsEnumerable()
                       select new Route
                       {
                           线路 = p.Field<string>("线路ID"),
                           方向 = p.Field<string>("方向"),
                           长度 = p.Field<string>("长度")
                       }).ToList();


            var data = SqlHelper.ExecuteDataTable(@"select 线路id,线路内序号,'上行始发'  as 类型,站名  from  tblrouted  where 上行始发=1
                                                                  union all
                                                   select 线路id, 线路内序号,'上行终点'  as 类型,站名   from tblrouted  where 上行终点 = 1
                                                                  union all
                                                   select 线路id, 线路内序号,'下行始发'  as 类型,站名   from tblrouted  where 下行始发 = 1
                                                                   union all
                                                   select 线路id, 线路内序号,'下行终点'  as 类型,站名   from tblrouted  where 下行终点 = 1");

            foreach (DataRow row in data.Rows)
            {
                _routeSRangeS.Add(new RouteSRange
                {
                    线路 = Convert.ToString(row["线路id"]),
                    线路内序号 = Convert.ToInt32(row["线路内序号"]),
                    类型 = Convert.ToString(row["类型"]),
                    站名 = Convert.ToString(row["站名"]),
                });
            }

            ///更新 实际发，实际到，单边里程

            Thread Formd = new Thread(() =>
            {
                while (true)
                {
                    ProcessData();
                    Thread.Sleep(50000);
                }
            });
            Formd.IsBackground = true;
            Formd.Start();

            Thread FormdXXX = new Thread(() =>
            {
                while (true)
                {
                    ProcessXXX();
                    Thread.Sleep(5000);
                }
            });
            FormdXXX.IsBackground = true;
            FormdXXX.Start();

            Doss();
            Dow();
        }





        private void WindowsRouteFormD_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void WindowsRouteFormD_FormClosed(object sender, FormClosedEventArgs e)
        {


            //关闭这个程序
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
