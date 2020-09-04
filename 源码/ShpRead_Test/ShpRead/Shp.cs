using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
namespace ShpRead
{
    public class Shp
    {
        /* 下面数据成员是Shp文件头对应的分段信息，具体参考设计指导书*/
        byte[] filecode = { 0, 0, 0, 0 };           //文件代码
        byte[] rev1 = { 0, 0, 0, 0 };               //未使用
        byte[] rev2 = { 0, 0, 0, 0 };               //未使用
        byte[] rev3 = { 0, 0, 0, 0 };               //未使用
        byte[] rev4 = { 0, 0, 0, 0 };               //未使用
        byte[] rev5 = { 0, 0, 0, 0 };               //未使用
        byte[] filelength = { 0, 0, 0, 0 };         //文件长度
        byte[] filever = { 0, 0, 0, 0 };            //文件版本
        byte[] shpType = { 0, 0, 0, 0 };            //shp类型
        byte[] xmin = { 0, 0, 0, 0, 0, 0, 0, 0 };   //x最小值
        byte[] ymin = { 0, 0, 0, 0, 0, 0, 0, 0 };   //y最小值
        byte[] xmax = { 0, 0, 0, 0, 0, 0, 0, 0 };   //x最大值
        byte[] ymax = { 0, 0, 0, 0, 0, 0, 0, 0 };   //y最大值
        byte[] zmin = { 0, 0, 0, 0, 0, 0, 0, 0 };   //z最小值
        byte[] zmax = { 0, 0, 0, 0, 0, 0, 0, 0 };   //z最大值
        byte[] mmin = { 0, 0, 0, 0, 0, 0, 0, 0 };
        byte[] mmax = { 0, 0, 0, 0, 0, 0, 0, 0 };
        // ----- 以上是Shp文件文件头信息-----// //

        FileStream fs = null;
        BinaryReader bRead = null;
        OleDbDataAdapter myadapter;
        DataSet myds;
        public DataSet GetDataSet()
        {
            return myds;
        }
        public string GetFieldValue(int objectID,string fieldname)
        {
            /*
             * 个人注释 -- 2018.7.10
             * 暂时使用其他方式实现的
             * 如下：
             * myds.Tables["dataTable"].Rows[recordID - 1].ItemArray[3].ToString();
             * 通过筛选数据集中表的对应列数，找到对应行数据即可
             * 比如：在点数据集中，获得每个点数据对应列，然后获得第3行数据(点名称)即可
             * 
             */




            /* 根据给定的objectid和字段名，返回相应的字段值
             * 操作提示： DataTable的Select函数可以通过指定条件查询* 
             */
            string strField = "";


           
                //  此处书写代码,完成相应功能
                String filteExpressiom = "OBJECTID = " + objectID;
                try
                {
                    DataRow[] dr = myds.Tables["dataTable"].Select(filteExpressiom);
                    if(dr.Count()>0)
                    {
                        strField = dr[0][fieldname].ToString();
                    }
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.Message);
                }

            
            return strField;

        }
        public int OpenShpFile(string filename)
        {

            /*
            * 通过filename参数打开shp文件，并初始化数据集，以及将文件头信息通过CopyToStru填充到
            * 文件头分段信息中；dbf的链接字符串为：
            * provider=microsoft.jet.oledb.4.0;data source=文件所在目录;Extended Properties=dBASE IV;
            * Select 语句的写法为:  
            *    select * from dbf文件名
            * 由于参数是文件名，因此，需要从文件名中截取出文件所在路径和dbf文件名，此处的
            * dbf文件名为不带路径和扩展名的文件名！！！！  
            * shp文件是二进制格式的文件，因此需用二进制方式读取
            */
            byte[] mainHead = new byte[100];//100字节缓存区


            /*
             * 
             * 
             * 此处书写程序
             */

            #region shp文件 和 dbf文件
            if (filename != null)
            {
                String dbfname;
                String dbfpath = null;

                /**
                 * 个人注释 -- 2018.7.10
                 * filename字符串进行处理
                 * 例如：E:\\C#\\2018\\森林.shp
                 * 先按照\\分割成四部分，最后一部分为森林.shp
                 * 森林.shp按照"."分割出shp文件名--森林
                 * 再将前面三部分拼接起来，按照 \\ 组成E:\\C#\\2018
                 */
                String[] sp1 = new String[] { "\\" };
                String[] filename1 = filename.Split(sp1, StringSplitOptions.RemoveEmptyEntries);

                Char[] sp2 = new Char[] { '.' };
                String[] filename2 = filename1[filename1.Length - 1].Split(sp2);
                dbfname = filename2[0];
                    
                //拼接成文件所在目录
                for (int i = 0; i < filename1.Length - 1; i++)
                {
                    if (i != filename1.Length - 2)
                    {
                        dbfpath += filename1[i] + "\\";
                    }
                    if (i == filename1.Length - 2)
                    {
                        dbfpath += filename1[i];
                    }
                }

                OleDbConnection conn = null;
                try
                {
                    fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    bRead = new BinaryReader(fs);   
                    mainHead = bRead.ReadBytes(100);
                    CopyToStru(mainHead);

                    conn = new OleDbConnection("provider=microsoft.jet.oledb.4.0;data source=" + dbfpath + ";Extended Properties=dBASE IV;");
                    conn.Open();
                    String sql = "select * from " + dbfname;
                    myadapter = new OleDbDataAdapter(sql, conn);
                    myds = new DataSet();
                    myadapter.Fill(myds, "dataTable");
                    conn.Close();                  
                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.Message);
                }

                int fileCode = GetFileCode();
                //如果打开的是shp文件，返回1
                if (fileCode == 9994)
                {
                    return 1;
                }
                //如果打开的不是shp文件，返回-1
                else
                {
                    return -1;
                }              
            }
            #endregion

            return -1;
        }
        public int GetNextShp(out int shptype, out object shpContent, out int recordID)
        {
            shptype = GetFileType();
            recordID = 0;

            #region 读取点shp, shptype = 1
            if (GetFileType() == 1)//Point
            {

                shpContent = new PointShp();
                PointShp tmp = (PointShp)shpContent;
                /*
                 * 
                 * 此处补充代码，完成点对象的读取
                 * 
                 */
                byte[] pointInfo = new byte[28]; //28字节缓存区
                pointInfo = bRead.ReadBytes(28);
                if (pointInfo.Length > 0)
                {
                    byte[] recordIDByte = new byte[4];  //4字节缓存区
                    int index = 0;
                    for (int i = 3; i >= 0; i--, index++)
                    {
                        recordIDByte[i] = pointInfo[index];
                    }
                    recordID = BitConverter.ToInt32(recordIDByte, 0);
                    tmp.RecordID = recordID;
                    tmp.pf.X = (float)BitConverter.ToDouble(pointInfo, 12);
                    tmp.pf.Y = (float)BitConverter.ToDouble(pointInfo, 20);

                    //需要使用GetFieldValue来读这个label，后期改一下
                    //tmp.label = myds.Tables["dataTable"].Rows[recordID - 1].ItemArray[8].ToString();
                    tmp.label = GetFieldValue(recordID , "名称");
                    return 1;
                }
                return 0;
            }
            #endregion 点处理完成

            # region 读取线代码  shptype = 3
            else if (GetFileType() == 3)
            {
                shpContent = new LinesShp();
                LinesShp tmp = shpContent as LinesShp;
                /*
                 * 
                 * 此处补充代码，完成线对象的读取
                 * 
                 */
                byte[] lineInfo1 = new byte[52];  //52字节缓冲区
                lineInfo1 = bRead.ReadBytes(52);
                if (lineInfo1.Length > 0)
                {
                    byte[] recordIDByte = new byte[4];  //4字节缓存区
                    int index = 0;
                    for (int i = 3; i >= 0; i--, index++)
                    {
                        recordIDByte[i] = lineInfo1[index];
                    }
                    recordID = BitConverter.ToInt32(recordIDByte, 0);  
                    tmp.RecordID = recordID;
                    tmp.Box[0] = BitConverter.ToDouble(lineInfo1, 12);
                    tmp.Box[1] = BitConverter.ToDouble(lineInfo1, 20);
                    tmp.Box[2] = BitConverter.ToDouble(lineInfo1, 28);
                    tmp.Box[3] = BitConverter.ToDouble(lineInfo1, 36);
                    tmp.NumParts = BitConverter.ToInt32(lineInfo1, 44);   //读段数
                    tmp.NumPoints = BitConverter.ToInt32(lineInfo1, 48);    //读点数
                    byte[] lineInfo2 = new byte[4 * tmp.NumParts + 16 * tmp.NumPoints]; //4*段数 +16*点数
                    lineInfo2 =  bRead.ReadBytes(4 * tmp.NumParts + 16 * tmp.NumPoints);

                    
                    int startNumPartsIndex = 0;
                    tmp.FirstPointIndexInParts = new Int32[tmp.NumParts];  //初始化线集合的大小
                    //读每一部分线开始的点在所有点集合中的索引号
                    for (int i = 0; i < tmp.NumParts; i++,startNumPartsIndex=startNumPartsIndex+4)
                    {
                        tmp.FirstPointIndexInParts[i] = BitConverter.ToInt32(lineInfo2, startNumPartsIndex);
                    }
                    
                    


                    //读所有点的集合
                    int startNumPointsIndex_X = 4*tmp.NumParts;
                    tmp.Points = new PointF[tmp.NumPoints]; //初始化点集合大小
                    for(int i = 0; i < tmp.NumPoints; i++, startNumPointsIndex_X = startNumPointsIndex_X + 16)
                    {
                        tmp.Points[i].X = (float)BitConverter.ToDouble(lineInfo2, startNumPointsIndex_X);
                    }

                    int startNumPointsIndex_Y = 4*tmp.NumParts+8;
                    for (int i = 0; i < tmp.NumPoints; i++, startNumPointsIndex_Y = startNumPointsIndex_Y + 16)
                    {
                        tmp.Points[i].Y = (float)BitConverter.ToDouble(lineInfo2, startNumPointsIndex_Y);
                    }

                    //tmp.label = myds.Tables["dataTable"].Rows[recordID - 1].ItemArray[3].ToString();
                    tmp.label = GetFieldValue(recordID, "道路名");

                    return 1;

                }

                return 0;
            }
            #endregion 线读取完成

            #region 其他的不理
            else
            {
                shptype = 0; recordID = 0; shpContent = null;
                return 0;
            }
            #endregion

           //return 1;
        }
        public void CloseShpFile()
        {
            if (bRead != null && fs != null)
            {
                bRead.Close();
                fs.Close();
            }
        }
        public int  CopyToStru(byte[] mainHead)
        {
                /*
                * 此函数填充Shp的属性成员，将文件头的100个字节分别填到相应的属性变量中
                * 
                */
            if(mainHead.Length!=100)
            {
                return -1;
            }
            int index = 0;

            //文件代码
            for (int i = 3;i>=0 ;i--, index++)
            {
                filecode[i] = mainHead[index];
            }

            //----以下书写你的代码--------//

            //未使用
            for (int i = 3; i >=0; i--, index++)
            {
                rev1[i] = mainHead[index];
            }

            for (int i = 3; i >=0; i--, index++)
            {
                rev2[i] = mainHead[index];
            }

            for (int i = 3; i >=0; i--, index++)
            {
                rev3[i] = mainHead[index];
            }

            for (int i = 3; i >=0; i--, index++)
            {
                rev4[i] = mainHead[index];
            }

            for (int i = 3; i >= 0; i--, index++)
            {
                rev5[i] = mainHead[index];
            }

            //文件长度

            for (int i = 3; i >=0; i--, index++)
            {
                filelength[i] = mainHead[index];
            }

            //文件版本

            for (int i = 0; i < 4; i++, index++)
            {
                filever[i] = mainHead[index];
            }


            //shp类型
            for (int i = 0; i < 4; i++, index++)
            {
                shpType[i] = mainHead[index];
            }

            //x最小值
            for (int i = 0; i < 8; i++, index++)
            {
                xmin[i] = mainHead[index];
            }

            //y最小值

            for (int i = 0; i < 8; i++, index++)
            {
                ymin[i] = mainHead[index];
            }


            //x最大值

            for (int i = 0; i < 8; i++, index++)
            {
                xmax[i] = mainHead[index];
            }

            //y最大值

            for (int i = 0; i < 8; i++, index++)
            {
                ymax[i] = mainHead[index];
            }

            //z最小值

            for (int i = 0; i < 8; i++, index++)
            {
                zmin[i] = mainHead[index];
            }

            //z最大值

            for (int i = 0; i < 8; i++, index++)
            {
                zmax[i] = mainHead[index];
            }
            //m最小值
            for (int i = 0; i < 8; i++, index++)
            {
                mmin[i] = mainHead[index];
            }
            //m最大值
            for (int i = 0; i < 8; i++, index++)
            {
                mmax[i] = mainHead[index];
            }

            return 1;
        }
        public int GetFileCode()
        {
            int kl = 0;
            //  此处补充代码，返回文件代码，若不是9994，请检查你的代码  ---//

            //字节顺序 --- 大
            kl = BitConverter.ToInt32(filecode,0);
            return kl;
        }
        public int GetFileLength()
        {
            int kl = 0;
            //补充代码----  返回文件长度---////

            //字节顺序 --- 大
            kl = BitConverter.ToInt32(filelength, 0);
            return kl * 2;// 为什么乘以2  ？？？？指导书中第三页倒数第六行有说明
        }
        public int GetFileVer()
        {
            int k1 = 0;
            /*
             * 功能： 返回文件版本 对应filever
             * 补充代码
             */

            //字节顺序 --- 小
            k1 = BitConverter.ToInt32(filever, 0);
            return k1;
        }
        public int GetFileType()
        {
            int kl = 0;
            //补充代码---  返回类型 对应shpType变量///

            //字节顺序  --- 小
            kl = BitConverter.ToInt32(shpType,0);
            return kl;
        }

        //以下字节顺序均为小
        public double GetXMin()
        {
            //获得边界盒中的x最小值---////
            double kl = 0;
            //代码书写处---///
            kl = BitConverter.ToDouble(xmin, 0);
            return kl; ;

        }

        
        //------以下函数完整，不需修改---////
        public double GetXMax()// 获得边界盒中的x最大值--//
        {
            double kl = 0;
            kl = BitConverter.ToDouble(xmax, 0);
            return kl;
        }
        public double GetYMin()// 获得边界盒中的Y最大值--，以下函数类似//
        {
            return BitConverter.ToDouble(ymin,0);
        }
        public double GetYMax()
        {
            return BitConverter.ToDouble(ymax, 0);
        }
        public double GetMMin()
        {
            return BitConverter.ToDouble(mmin, 0);
        }
        public double GetMMax()
        {
            return BitConverter.ToDouble(mmax, 0);
        }
        public double GetZMin()
        {
            return BitConverter.ToDouble(zmin, 0);
        }
        public double GetZMax()
        {
            return BitConverter.ToDouble(zmax, 0);
        }
    }

    public class PointShp  //点对象定义
    {
        public Int32 RecordID;   //shp文件中记录号
        public PointF pf;        //点的坐标
        public string label;     //点的标注，注意此标注不在shp文件中，而是在DBF数据库中
     }

    public class LinesShp //线对象的定义
     {
        public Int32 RecordID;  //shp文件中线的记录号
        public double[] Box = new double[4]; //此条线的包围盒
        public Int32 NumParts;               //此条线包含的部分数
        public Int32 NumPoints;              //此条线包含的点的总数
        public Int32[] FirstPointIndexInParts;// 每一部分的第一个点在所有点的集合中的索引号，从0开始；
        public PointF[] Points; // 所有点的集合
        public string label;    //标注，同点定义中的说明
        
      }

}
