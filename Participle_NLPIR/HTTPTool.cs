using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;

namespace NLPOOV
{
    class HTTPTool
    {
        public Delegate callback = null;
        public CookieContainer cc = new CookieContainer();
        public HTTPTool(){}

        public void SetDelegate(Delegate dlt)
        {
            callback = dlt;
        }

        //异步获取网页html
        //回调函数 void MethodName(string )
        public void GetRequest(string url, Hashtable param, HtmlDelegate dlt)
        {
            this.callback = dlt;

            url = url.TrimEnd('/');
            string formData = "";
            foreach (DictionaryEntry de in param)
            {
                formData += de.Key.ToString() + "=" + de.Value.ToString() + "&";
            }
            if (formData.Length > 0)
            {
                formData = formData.Substring(0, formData.Length - 1);
                url += "?" + formData;
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);    
            request.CookieContainer = cc;
            
            IAsyncResult result = (IAsyncResult)request.BeginGetResponse(ResponseCallback_Html, request);
        }

        //若request.cookiecontainer不为null且里面没有SessionID，则会分配新SessionID
        public void PostRequest(string url, Hashtable param, HtmlDelegate dlt)
        {
            this.callback = dlt;

            url = url.TrimEnd('/');
            string formData = "";
            foreach (DictionaryEntry de in param)
            {
                formData += de.Key.ToString() + "=" + de.Value.ToString() + "&";
            }
            if (formData.Length > 0)
                formData = formData.Substring(0, formData.Length - 1);
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(formData);
       
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = cc;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            request.Referer = "http://bitpt.cn";
            request.Host = "bitpt.cn";
            //Referer: http://bitpt.cn/login.php
            //Accept: text/html, application/xhtml+xml, */*
            //Accept-Encoding: gzip, deflate
            //Accept-Language: zh-CN
            //User-Agent: Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)
            //Content-Type: application/x-www-form-urlencoded
            //Host: bitpt.cn

            Stream req = request.GetRequestStream();
            req.Write(data, 0, data.Length);
            req.Close();

            IAsyncResult result = (IAsyncResult)request.BeginGetResponse(ResponseCallback_Html, request);
        }


        public delegate void HtmlDelegate(string content);
        private void ResponseCallback_Html(IAsyncResult result)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
                //StreamReader reader = new StreamReader(response.GetResponseStream(), DBCSCodePage.DBCSEncoding.GetDBCSEncoding("gb2312"));
                cc.Add(response.Cookies);   //添加服务器返回的Cookie
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string html = reader.ReadToEnd();
                reader.Close();
                response.Close();

                if (callback != null)
                    callback.DynamicInvoke(html);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Source);
                Debug.WriteLine(e.StackTrace);
                Debug.WriteLine(e.Message);
            }
        }

        //同步的POST和GET
        //若request.cookiecontainer不为null且里面没有SessionID，则会分配新SessionID
        public string PostAndGetHTML(string targetURL, Hashtable param)
        {
            //处理参数param并转换为byte[]
            targetURL = targetURL.TrimEnd('/');
            string formData = "";
            foreach (DictionaryEntry de in param)
            {
                formData += de.Key.ToString() + "=" + de.Value.ToString() + "&";
            }
            if (formData.Length > 0)
                formData = formData.Substring(0, formData.Length - 1);
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(formData);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(targetURL);
            request.CookieContainer = cc;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            request.Referer = "http://bitpt.cn";
            request.Host = "bitpt.cn";
            Stream req = request.GetRequestStream();
            req.Write(data, 0, data.Length);
            req.Close();
            
            
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream rep = response.GetResponseStream();
            cc.Add(response.Cookies);

            //foreach (Cookie cookie in response.Cookies)
            //    Console.WriteLine(cookie.Name + ": " + cookie.Value);
            //                WriteCookieToSession(cc);
            string result = new StreamReader(rep, System.Text.Encoding.UTF8).ReadToEnd();
            return result;
        }

        public string GetHTML(string targetURL, Hashtable param)
        {
            //添加参数到URL
            targetURL = targetURL.TrimEnd('/');
            string formData = "";
            foreach (DictionaryEntry de in param)
            {
                formData += de.Key.ToString() + "=" + de.Value.ToString() + "&";
            }
            if (formData.Length > 0)
            {
                formData = formData.Substring(0, formData.Length - 1);
                targetURL += "?" + formData;
            }
            //建立请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(targetURL);
            request.CookieContainer = cc;


            HttpWebResponse response = (HttpWebResponse)request.GetResponse(); //回应
            //通过response可以获取http头部信息
            cc.Add(response.Cookies);   //添加服务器返回的Cookie

            //foreach (Cookie cookie in response.Cookies)
            //    Console.WriteLine(cookie.Name + ": " + cookie.Value);

            
            Stream rep = response.GetResponseStream();  //获取数据流
            string result = new StreamReader(rep, System.Text.Encoding.UTF8).ReadToEnd();    //读取全部数据
            return result;
        }
    }

}
