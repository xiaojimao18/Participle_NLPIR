﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Participle_NLPIR
{
    [StructLayout(LayoutKind.Explicit)]
    public struct result_t
    {
        [FieldOffset(0)]
        public int start;
        [FieldOffset(4)]
        public int length;
        [FieldOffset(8)]
        public int sPos;
        [FieldOffset(12)]
        public int sPosLow;
        [FieldOffset(16)]
        public int POS_id;
        [FieldOffset(20)]
        public int word_ID;
        [FieldOffset(24)]
        public int word_type;
        [FieldOffset(28)]
        public int weight;
    }

    class Participle
    {
        //词的表和词的频率
        Hashtable wordTable = new Hashtable();
        
        //是否初始化成功
        bool isInitSuccess = false;

        //dll文件所在的路径，注意根据实际情况修改
        const string path = @".\NLPIR\bin\ICTCLAS2014\NLPIR.dll";

        //给出Data文件所在的路径，注意根据实际情况修改
        const string dataPath = @".\NLPIR\";

        //词频输出文件
        const string resultFile = @".\result2000_v2.txt";

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_Init", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool NLPIR_Init(String sInitDirPath, int encoding);

        //特别注意，C语言的函数NLPIR_API const char * NLPIR_ParagraphProcess(const char *sParagraph,int bPOStagged=1);必须对应下面的申明
        //[DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi, EntryPoint = "NLPIR_ParagraphProcess")]
        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ParagraphProcess")]
        public static extern IntPtr NLPIR_ParagraphProcess(String sParagraph, int bPOStagged = 1);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_Exit", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool NLPIR_Exit();

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_ImportUserDict", CallingConvention = CallingConvention.Cdecl)]
        public static extern int NLPIR_ImportUserDict(String sFilename);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_FileProcess", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool NLPIR_FileProcess(String sSrcFilename, String sDestFilename, int bPOStagged = 1);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_FileProcessEx", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool NLPIR_FileProcessEx(String sSrcFilename, String sDestFilename);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_GetParagraphProcessAWordCount", CallingConvention = CallingConvention.Cdecl)]
        static extern int NLPIR_GetParagraphProcessAWordCount(String sParagraph);

        //NLPIR_GetParagraphProcessAWordCount
        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_ParagraphProcessAW", CallingConvention = CallingConvention.Cdecl)]
        static extern void NLPIR_ParagraphProcessAW(int nCount, [Out, MarshalAs(UnmanagedType.LPArray)] result_t[] result);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_AddUserWord", CallingConvention = CallingConvention.Cdecl)]
        static extern int NLPIR_AddUserWord(String sWord);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_SaveTheUsrDic", CallingConvention = CallingConvention.Cdecl)]
        static extern int NLPIR_SaveTheUsrDic();

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_DelUsrWord", CallingConvention = CallingConvention.Cdecl)]
        static extern int NLPIR_DelUsrWord(String sWord);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_NWI_Start", CallingConvention = CallingConvention.Cdecl)]
        static extern bool NLPIR_NWI_Start();

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_NWI_Complete", CallingConvention = CallingConvention.Cdecl)]
        static extern bool NLPIR_NWI_Complete();

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_NWI_AddFile", CallingConvention = CallingConvention.Cdecl)]
        static extern bool NLPIR_NWI_AddFile(String sText);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_NWI_AddMem", CallingConvention = CallingConvention.Cdecl)]
        static extern bool NLPIR_NWI_AddMem(String sText);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi, EntryPoint = "NLPIR_NWI_GetResult")]
        public static extern IntPtr NLPIR_NWI_GetResult(bool bWeightOut = false);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_NWI_Result2UserDict", CallingConvention = CallingConvention.Cdecl)]
        static extern uint NLPIR_NWI_Result2UserDict();

        public Participle()
        {
            Init_NLPIR();
        }

        ~Participle() 
        {
            NLPIR_Exit();
        }

        public bool Init_NLPIR()
        {
            if (!NLPIR_Init(dataPath, 0))
            {
                System.Console.WriteLine("Init ICTCLAS failed!");
                this.isInitSuccess = false;
            }
            else
            {
                System.Console.WriteLine("Init ICTCLAS success!");
                this.isInitSuccess = true;
            }
            return this.isInitSuccess;
        }

        //分词
        public void Participle_NLPIR(string s)
        {
            // 判断初始化是否成功
            if (this.isInitSuccess == false)
            {
                return;
            }

            // 空内容直接返回
            s = s.Trim();
            if (s.Equals(""))
            {
                return;
            }
            
            //String s = "ICTCLAS在高富帅国内973专家组组织的评测中活动获得了第一名，在第一届国际中文处理研究机构SigHan组织的评测中都获得了多项第一名。";

            //先得到结果的词数
            /*
            int count = NLPIR_GetParagraphProcessAWordCount(s);
            System.Console.WriteLine("NLPIR_GetParagraphProcessAWordCount success!");
            result_t[] result = new result_t[count];//在客户端申请资源
            NLPIR_ParagraphProcessAW(count, result);//获取结果存到客户的内存中

            int i = 1;
            foreach (result_t r in result)
            {
                String sWhichDic = "";
                switch (r.word_type)
                {
                    case 0:
                        sWhichDic = "核心词典";
                        break;
                    case 1:
                        sWhichDic = "用户词典";
                        break;
                    case 2:
                        sWhichDic = "专业词典";
                        break;
                    default:
                        break;
                }
                Console.WriteLine("No.{0}:start:{1}, length:{2},POS_ID:{3},Word_ID:{4}, UserDefine:{5}\n", i++, r.start, r.length, r.POS_id, r.word_ID, sWhichDic);//, s.Substring(r.start, r.length)
            }
            */
            //StringBuilder sResult = new StringBuilder(6000); //准备存储空间

            //NLPIR_FileProcess("output.txt", "result.txt");

            
            IntPtr intPtr = NLPIR_ParagraphProcess(s);      //切分结果保存为IntPtr类型
            String str = Marshal.PtrToStringAnsi(intPtr);   //将切分结果转换为string
            //Console.WriteLine(str);

            String[] words = str.Split(' ');

            foreach(String word in words)
            {
                //String word = wordOld.Trim().Split('/')[0];
                if (!word.Trim().Equals(""))
                {
                    if (wordTable.ContainsKey(word))
                    {
                        wordTable[word] = Convert.ToInt32(wordTable[word]) + 1;
                    }
                    else
                    {
                        wordTable.Add(word, "1");
                    }
                }
            }

            /*
            System.Console.WriteLine("Before Userdict imported:");
            String ss;
            Console.WriteLine("insert user dic:");
            ss = Console.ReadLine();
            while (!ss.Equals("") && ss[0] != 'q' && ss[0] != 'Q')
            {
                //用户词典中添加词
                int iiii = NLPIR_AddUserWord(ss);//词 词性 example:点击下载 vyou
                intPtr = NLPIR_ParagraphProcess(s, 1);
                str = Marshal.PtrToStringAnsi(intPtr);
                System.Console.WriteLine(str);
                NLPIR_SaveTheUsrDic(); // save the user dictionary to the file

                //删除用户词典中的词
                Console.WriteLine("delete usr dic:");
                ss = Console.ReadLine();
                iiii = NLPIR_DelUsrWord(ss);
                str = Marshal.PtrToStringAnsi(intPtr);
                System.Console.WriteLine(str);
                NLPIR_SaveTheUsrDic();
            }
            
            //测试新词发现与自适应分词功能
            NLPIR_NWI_Start();//新词发现功能启动
            NLPIR_NWI_AddFile("./NLPIR/test/test.TXT");//添加一个待发现新词的文件，可反复添加
            NLPIR_NWI_Complete();//新词发现完成


            intPtr = NLPIR_NWI_GetResult();
            str = Marshal.PtrToStringAnsi(intPtr);

            System.Console.WriteLine("新词识别结果:");
            System.Console.WriteLine(str);
            NLPIR_FileProcess("../test/屌丝，一个字头的诞生.TXT", "../test/屌丝，一个字头的诞生-分词结果.TXT");
            NLPIR_NWI_Result2UserDict();//新词识别结果导入分词库
            NLPIR_FileProcess("../test/屌丝，一个字头的诞生.TXT", "../test/屌丝，一个字头的诞生-自适应分词结果.TXT");
            */
        }

        public void OutputResult()
        {
            StreamWriter sw = File.AppendText(resultFile);

            for (IDictionaryEnumerator enu = wordTable.GetEnumerator(); enu.MoveNext() == true; )
            {
                sw.WriteLine(enu.Key + " " + enu.Value);
            }

            sw.Close();
        }

        public void addUserWord(String ss)
        {
            int iiii = NLPIR_AddUserWord(ss);//词 词性 example:点击下载 vyou
            System.Console.WriteLine("Add new word:" + ss);
            NLPIR_SaveTheUsrDic(); // save the user dictionary to the file
        }
    }
}
