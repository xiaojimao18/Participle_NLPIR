using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Net;
using Participle_NLPIR;

namespace NLPOOV
{
    class Program
    {
        Participle participle = new Participle();
        HTTPTool httptool = new HTTPTool();
        string filename = "output.txt"; //输出文件，测试用

        //分析html文档，抽取其中的“文字部分”，输出到文件
        //具体是将贴子里用户发言div块中的非标签部分抽取出来
        //注意，由于是html文档，符号会用&XXX表示
        public void Extract(string html)
        {
            
            StreamWriter sw = File.AppendText(filename);
            try
            {
                int bg = 0, ed = 0, st, i, flag;
                while (bg >= 0 && ed >= 0)
                {
                    bg = html.IndexOf("<div class=\"t_fsz\">", ed + 1);
                    if (bg < 0) break;
                    ed = html.IndexOf("</div>", bg + 1);
                    if (ed < 0) break;

                    flag = 0;
                    st = -1;
                    for (i = bg; i < ed; i++)
                    {
                        if (html[i] == '<')
                        {
                            if (flag == 0 && st >= bg)
                            {
                                String content = html.Substring(st, i - st);
                                if (!content.Trim().Equals(""))
                                {
                                    Console.WriteLine("content: " + content);
                                    participle.Participle_NLPIR(content);
                                }
                                sw.Write(content);
                            }
                            flag++;
                        }
                        else if (html[i] == '>')
                        {
                            flag--;
                            if (flag == 0)
                            {
                                st = i + 1;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
            finally
            {
                sw.Close();
            }
        }
        
        //遍历论坛帖子
        public void SearchForum()
        {
            string url = "http://bitpt.cn/bbs/forum.php";
            Hashtable param = new Hashtable();
            param.Add("mod", "viewthread");
            param.Add("tid", "300000"); //帖子id会在后面被替换
            
            int tnum = 3000;
            for (int i = 100000; i < 100000 + tnum; i++) //遍历帖子id
            {
                param["tid"] = i.ToString();

                try
                {
                    Extract(httptool.GetHTML(url, param)); //获取该帖子html文档并分析
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                }
            }
        }

        static void Main(string[] args)
        {
            Program program = new Program();

            program.httptool.cc.Add(cookieData()); //手动加入需要用到的cookie
            program.SearchForum();

            Console.ReadKey();
        }

        static CookieCollection cookieData()
        {
            CookieCollection cookieCol = new CookieCollection();

            cookieCol.Add(new Cookie("WRoT_2132_sid","i7vnFG", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("WRoT_2132_saltkey", "SIdJzwYo", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("WRoT_2132_lastvisit", "1387725179", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("WRoT_2132_lastact", "1387728807%09forum.php%09", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("WRoT_2132_sendmail", "1", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("PHPSESSID", "8d999cc05ab62359f6f445b5ba677f21", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("btuid", "243", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("btpassword", "f6fe8cf776944423a5f9ba7d3e7bb35e", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("WL7ID_username", "cmonkey", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("WL7ID_activationauth", "MzI4CWNtb25rZXk%3D", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("hd_sid", "OPas3x", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("hd_hid", "0", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("WRoT_2132_auth", "d160rrwTSFJQA%2FdThM%2BFhZZ49fyPfW3yl%2FWiYiV0pP8LjMaKhJ%2BzrKm21JQLsgnjidrBo2OdCtbHN%2F28K6IUlw0", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("hd_auth", "6a06tweRW2xGI5oKzQC%2B6uwsC7Tskd1vd1qX4wYpyqDy", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("WRoT_2132_nofavfid", "1", "/", "bitpt.cn"));
            cookieCol.Add(new Cookie("WRoT_2132_ulastactivity", "99fceDxfwPq3KZ%2Frm4RdJCqIQdayfHMRFmrIeD7WH2ziykkTgmUA", "/", "bitpt.cn"));

            return cookieCol;
        }
    }
}
