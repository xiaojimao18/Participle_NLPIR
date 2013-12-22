// win_cDemo.cpp : 定义控制台应用程序的入口点。
//

#include "../../include/NLPIR.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#ifndef OS_LINUX
#pragma comment(lib, "../../bin/NLPIR.lib")
#endif

//Linux
#ifdef OS_LINUX
	#define _stricmp(X,Y) strcasecmp((X),(Y))
	#define _strnicmp(X,Y,Z) strncasecmp((X),(Y),(Z))
	#define strnicmp(X,Y,Z)	strncasecmp((X),(Y),(Z))
	#define _fstat(X,Y)     fstat((X),(Y))
	#define _fileno(X)     fileno((X))
	#define _stat           stat
	#define _getcwd         getcwd
	#define _off_t          off_t
	#define PATH_DELEMETER  "/"
#else
	#pragma warning(disable:4786)
	#define PATH_DELEMETER  "\\"
#endif
void SplitGBK(const char *sInput);
void SplitBIG5();
void SplitUTF8();
void testNewWord();

void testNewWord(int nCode)
{
	//初始化分词组件
	
	if(!NLPIR_Init("..",nCode))//数据在当前路径下，默认为GBK编码的分词
	{
		printf("ICTCLAS INIT FAILED!\n");
		return ;
	}
	char sInputFile[1024]="../test/test.TXT",sResultFile[1024];
	if (nCode==UTF8_CODE)
	{
		strcpy(sInputFile,"../test/test-utf8.TXT");
	}

	//NLPIR
	const char *sResult=NLPIR_GetFileKeyWords(sInputFile);
	FILE *fp=fopen("Result.txt","wb");
	fprintf(fp,sResult);
	fclose(fp);

	sResult=NLPIR_GetFileNewWords(sInputFile);
	fp=fopen("ResultNew.txt","wb");
	fprintf(fp,sResult);
	fclose(fp);

	NLPIR_NWI_Start();
	NLPIR_NWI_AddFile(sInputFile);
	NLPIR_NWI_Complete();
	const char *pNewWordlist=NLPIR_NWI_GetResult();
	printf("识别出的新词为：%s\n",pNewWordlist);

	strcpy(sResultFile,sInputFile);
	strcat(sResultFile,"_result1.txt");
	NLPIR_FileProcess(sInputFile,sResultFile);

	NLPIR_NWI_Result2UserDict();//新词识别结果

	strcpy(sResultFile,sInputFile);
	strcat(sResultFile,"_result2.txt");
	NLPIR_FileProcess(sInputFile,sResultFile);

	NLPIR_Exit();
}
void SplitGBK(const char *sInput)
{//分词演示

	//初始化分词组件
	if(!ICTCLAS_Init())//数据在当前路径下，默认为GBK编码的分词
	{
		printf("ICTCLAS INIT FAILED!\n");
		return ;
	}

	ICTCLAS_SetPOSmap(ICT_POS_MAP_SECOND);

	char sSentence[5000]="ICTCLAS在国内973专家组组织的评测中活动获得了第一名，在第一届国际中文处理研究机构SigHan组织的评测中都获得了多项第一名。";
	const char * sResult;


	int nCount;
	ICTCLAS_ParagraphProcessA(sSentence,&nCount);
	printf("nCount=%d\n",nCount);
	nCount = ICTCLAS_GetParagraphProcessAWordCount(sSentence);
	const result_t *pResult=ICTCLAS_ParagraphProcessA(sSentence,&nCount);

	int i=1;
	char *sWhichDic;
	for(i=0;i<nCount;i++)
	{
		sWhichDic="";
		switch (pResult[i].word_type)
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
		printf("No.%d:start:%d, length:%d,POS_ID:%d,Word_ID:%d, UserDefine:%s, Word:%s\n",
			i+1, pResult[i].start, pResult[i].length, pResult[i].iPOS, pResult[i].word_ID, sWhichDic,sSentence+pResult[i].start );
	}
	while(_stricmp(sSentence,"q")!=0)
	{
		sResult = ICTCLAS_ParagraphProcess(sSentence,1);
		printf("%s\nInput string now('q' to quit)!\n", sResult);// 
		gets(sSentence);
	}
	
	//导入用户词典前
	printf("未导入用户词典：\n");
	sResult = ICTCLAS_ParagraphProcess(sInput, 1);
	printf("%s\n", sResult);

	//导入用户词典后
	printf("\n导入用户词典后：\n");
	nCount = ICTCLAS_ImportUserDict("userdic.txt");//userdic.txt覆盖以前的用户词典
	//保存用户词典
	ICTCLAS_SaveTheUsrDic();
	printf("导入%d个用户词。\n", nCount);
	
	sResult = ICTCLAS_ParagraphProcess(sInput, 1);
	printf("%s\n", sResult);

	//动态添加用户词
	printf("\n动态添加用户词后：\n");
	ICTCLAS_AddUserWord("计算机学院   xueyuan");
	ICTCLAS_SaveTheUsrDic();
	sResult = ICTCLAS_ParagraphProcess(sInput, 1);
	printf("%s\n", sResult);


	//对文件进行分词
	//ICTCLAS_FileProcess("test2.txt","test2_result.txt",1);
	//ICTCLAS_FileProcess("testGBK.txt","testGBK_result.txt",1);


	//释放分词组件资源
	ICTCLAS_Exit();
}

void SplitGBK_Fanti(const char *sInput)
{//分词演示

	//初始化分词组件
	if(!ICTCLAS_Init("",GBK_FANTI_CODE))//数据在当前路径下，默认为GBK编码的分词
	{
		printf("ICTCLAS INIT FAILED!\n");
		return ;
	}

	ICTCLAS_SetPOSmap(ICT_POS_MAP_SECOND);

	char sSentence[5000]="ICTCLAS在国内专家组组织的评测中活动获得了第一名，在第一届国际中文处理研究机构SigHan组织的评测中都获得了多项第一名。ICTCLAS在國內專家組組織的評測中活動獲得了第一名，在第一屆國際中文處理研究機構SigHan組織的評測中都獲得了多項第一名。";
	const char * sResult;


	int nCount;
	ICTCLAS_ParagraphProcessA(sSentence,&nCount);
	printf("nCount=%d\n",nCount);
	int count = ICTCLAS_GetParagraphProcessAWordCount(sSentence);
	const result_t *pResult=ICTCLAS_ParagraphProcessA(sSentence,&nCount);

	int i=1;
	char *sWhichDic;
	for(i=0;i<nCount;i++)
	{
		sWhichDic="";
		switch (pResult[i].word_type)
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
		printf("No.%d:start:%d, length:%d,POS_ID:%d,Word_ID:%d, UserDefine:%s, Word:%s\n",
			i+1, pResult[i].start, pResult[i].length, pResult[i].iPOS, pResult[i].word_ID, sWhichDic,sSentence+pResult[i].start );
	}
	while(_stricmp(sSentence,"q")!=0)
	{
		sResult = ICTCLAS_ParagraphProcess(sSentence,0);
		printf("%s\nInput string now('q' to quit)!\n", sResult);// 
		scanf("%s",sSentence);
	}
	
	//导入用户词典前
	printf("未导入用户词典：\n");
	sResult = ICTCLAS_ParagraphProcess(sInput, 0);
	printf("%s\n", sResult);

	//导入用户词典后
	printf("\n导入用户词典后：\n");
	nCount = ICTCLAS_ImportUserDict("userdic.txt");//userdic.txt覆盖以前的用户词典
	//保存用户词典
	ICTCLAS_SaveTheUsrDic();
	printf("导入%d个用户词。\n", nCount);
	
	sResult = ICTCLAS_ParagraphProcess(sInput, 1);
	printf("%s\n", sResult);

	//动态添加用户词
	printf("\n动态添加用户词后：\n");
	ICTCLAS_AddUserWord("计算机学院   xueyuan");
	ICTCLAS_SaveTheUsrDic();
	sResult = ICTCLAS_ParagraphProcess(sInput, 1);
	printf("%s\n", sResult);


	//对文件进行分词
	ICTCLAS_FileProcess("test2.txt","test2_result.txt",1);
	ICTCLAS_FileProcess("testGBK.txt","testGBK_result.txt",1);


	//释放分词组件资源
	ICTCLAS_Exit();
}
void SplitBIG5()
{
	//初始化分词组件
	if(!ICTCLAS_Init("",BIG5_CODE))//数据在当前路径下，设置为BIG5编码的分词
	{
		printf("ICTCLAS INIT FAILED!\n");
		return ;
	}
	ICTCLAS_FileProcess("testBIG.txt","testBIG_result.txt");
	ICTCLAS_Exit();
}
void SplitUTF8()
{
	//初始化分词组件
	if(!ICTCLAS_Init("",UTF8_CODE))//数据在当前路径下，设置为UTF8编码的分词
	{
		printf("ICTCLAS INIT FAILED!\n");
		return ;
	}
//	ICTCLAS_FileProcess("testUTF.txt","testUTF_result.txt");
	ICTCLAS_FileProcess("test.xml","testUTF_result.xml");


	FILE *fp=fopen("TestUTF8-bigfile.txt","rt");
	if (fp==NULL)
	{
		printf("Error Open TestUTF8-bigfile.txt\n");
		ICTCLAS_Exit();
		return ;
	}
	char sLine[10241];
	int i,nCount,nDocID=1;
	//result_t res[1000];
	//while (fgets(sLine,10240,fp))
	{
		CICTCLAS *pICTCLAS=new CICTCLAS;
	/*	
		int nCountA=pICTCLAS->GetParagraphProcessAWordCount(sLine);
		pICTCLAS->ParagraphProcessAW(nCountA,res);
		for(i=0;i<nCountA;i++)
		{
			printf("No.%d:start:%d, length:%d,POS_ID:%d,Word_ID:%d\n",
				i+1, res[i].start, res[i].length, res[i].iPOS, res[i].word_ID);
		}

	*/
		fseek(fp,0,SEEK_END);
		int nSize=ftell(fp);

		fseek(fp,0,SEEK_SET);
		fread(sLine,nSize,1,fp);
		sLine[nSize]=0;
		const result_t *pResult=pICTCLAS->ParagraphProcessA(sLine,&nCount);
	    i=1;

		for(i=0;i<500&&i<nCount;i++)
		{
 			printf("\nNo.%d:start:%d, length:%d,POS_ID:%d,Word_ID:%d\n",
 				i+1, pResult[i].start, pResult[i].length, pResult[i].iPOS, pResult[i].word_ID);
			fwrite(sLine+pResult[i].start,sizeof(char),pResult[i].length,stdout);
		}
		delete pICTCLAS;
		printf("Processed docs %d\r",nDocID++);
	}

	ICTCLAS_Exit();
}
void testMultiThread()
{
	//初始化分词组件
	if(!ICTCLAS_Init("",UTF8_CODE))//数据在当前路径下，设置为UTF8编码的分词
	{
		printf("ICTCLAS INIT FAILED!\n");
		return ;
	}
	ICTCLAS_FileProcess("E:\\ictclas2010\\反馈\\中科院分词初体验\\long_utf8.txt","E:\\ictclas2010\\反馈\\中科院分词初体验\\long_utf8_result.txt");
	ICTCLAS_FileProcess("E:\\ictclas2010\\反馈\\中科院分词初体验\\long.txt","E:\\ictclas2010\\反馈\\中科院分词初体验\\long_result.txt");	
	ICTCLAS_Exit();
}
int main()
{
	//testNewWord(GBK_CODE);
	testNewWord(UTF8_CODE);

	const char *sInput = "@领袖未来俱乐部 ： @ICTCLAS张华平博士 是计算机领域的专家学者，专注于中文自然语言处理、信息检索、信息安全的学术交流与产业应用。吴裕待@简凡如是 来北京学习深造，在学习过程中多请教。也请@吴永夷 关注。 ";
	//分词
	//SplitGBK(sInput);
	return 1;
}

